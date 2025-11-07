using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using OFMS_API.Models;
using OFMS_API.SerilogMiddleware;

public class ApiLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;

    public ApiLoggingMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        int userId =Convert.ToInt32(context.User.FindFirst("sub")?.Value
          ?? context.User.FindFirst("userid")?.Value
          ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var stopwatch = Stopwatch.StartNew();
        var log = new ApiLoggerTO
        {
            RequestTime = DateTime.Now,
            APIName = context.Request.Path,
            TimeStamp = DateTime.Now
        };

        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
            stopwatch.Stop();

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            if (context.Response.StatusCode >= 400)
            {
                var endpoint = context.GetEndpoint();
                var apiCodeAttr = endpoint?.Metadata.GetMetadata<ApiCodeAttribute>();

                log.ErrorCode = apiCodeAttr?.Code ?? "FE000";
                log.ResponseTime = DateTime.Now;
                log.ElapsedTime = Math.Round((decimal)stopwatch.Elapsed.TotalSeconds, 7);

                log.Response = responseText;

                log.ExceptionMessage = ExtractExceptionFromResponse(responseText)
                    ?? "API returned an error response.";
                log.ModuleId = 10;
                log.ModuleName = "OFMS";
                log.CreatedBy = userId;
                await SaveLogAsync(log);
            }

            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            var endpoint = context.GetEndpoint();
            var apiCodeAttr = endpoint?.Metadata.GetMetadata<ApiCodeAttribute>();

            log.ResponseTime = DateTime.Now;
            log.ElapsedTime = Math.Round((decimal)stopwatch.Elapsed.TotalSeconds, 7);
            log.Response = "Unhandled exception in middleware";
            log.ExceptionMessage = ex.ToString();
            log.ModuleName = "OFMS";
            log.ModuleId = 10;
            log.CreatedBy = userId;
            log.ErrorCode = apiCodeAttr?.Code ?? "FE000";
            await SaveLogAsync(log);

            throw;
        }
    }

    private async Task SaveLogAsync(ApiLoggerTO log)
    {
        var connectionString = _config.GetConnectionString("DefaultConnection");

        const string query = @"
            INSERT INTO ApiLogs 
                (TimeStamp, ModuleId, ModuleName, APIName, CreatedBy, RequestPayload, Response, RequestTime, ResponseTime, ErrorCode, ElapsedTime, ExceptionMessage)
            VALUES 
                (@TimeStamp, @ModuleId, @ModuleName, @APIName, @CreatedBy, @RequestPayload, @Response, @RequestTime, @ResponseTime, @ErrorCode, @ElapsedTime, @ExceptionMessage)";

        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(query, log);
        }
        catch
        {
        }
    }

    private string? ExtractExceptionFromResponse(string responseText)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(responseText))
                return null;

            var json = JObject.Parse(responseText);
            if (json.TryGetValue("exception", out var exceptionToken))
            {
                return exceptionToken.ToString();
            }
        }
        catch
        {
        }

        return null;
    }

  
}
