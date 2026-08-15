using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using OFMS_API.BL.Imple;
using OFMS_API.BL.Interface;
using OFMS_API.DAL.Imple;
using OFMS_API.DAL.Interface;
using OFMS_API.Helper.Register;
using Repository.DAL.Imple.Master.DropDownItemMaster;
using Repository.DAL.Imple.Master.ImageMaster;
using Repository.DAL.Imple.Master.ItemMaster;
using Repository.DAL.Interface.Master.DropDownItemMaster;
using Repository.DAL.Interface.Master.ImageMaster;
using Repository.DAL.Interface.Master.ItemMaster;
using Serilog;
using Services.BL.Imple.Master.ImageMaster;
using Services.BL.Imple.Master.ItemMaster;
using Services.BL.Imple.Master.ItemMasterDropDownBL;
using Services.BL.Interface.Master.ImageMaster;
using Services.BL.Interface.Master.ItemMaster;
using Services.BL.Interface.Master.ItemMasterDropDownBL;
using Repository.DAL.Imple.Master.AddressMaster;
using Repository.DAL.Interface.Master.AddressMaster;
using Services.BL.Imple.Master.AddressMaster;
using Services.BL.Interface.Master.AddressMaster;
using Repository.DAL.Imple.Master.CookAssignment;
using Repository.DAL.Interface.Master.CookAssignment;
using Services.BL.Imple.Master.CookAssignment;
using Services.BL.Interface.Master.CookAssignment;
using Repository.DAL.Imple.Master.DeliveryAssignment;
using Repository.DAL.Interface.Master.DeliveryAssignment;
using Services.BL.Imple.Master.DeliveryAssignment;
using Services.BL.Interface.Master.DeliveryAssignment;
using System.Text;
using OFMS_API.Hubs;
using Repository.DAL.Interface.Notification;
using Repository.DAL.Imple.Notification;
using Services.BL.Interface.Notification;
using Services.BL.Imple.Notification;
using OFMS_API.Services;
using Repository.DAL.Interface.Chat;
using Repository.DAL.Imple.Chat;
using Services.BL.Interface.Chat;
using Services.BL.Imple.Chat;
using OFMS_API.Helper.Common;
using Microsoft.AspNetCore.SignalR;
var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();


builder.Host.UseSerilog();

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.CommonRegister();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
builder.Services.AddSignalR();
builder.Services.AddHostedService<ChatCleanupHostedService>();

builder.Services.AddScoped<IuserDAL, userDAL>();
builder.Services.AddScoped<IMenuCategoryDAL, menuCategoryDAL>();
builder.Services.AddScoped<IOrderDAL, OrderDAL>();
builder.Services.AddScoped<IItemMasterDAL, ItemMasterDAL>();
builder.Services.AddScoped<IImageMasterBL, ImageMasterBL>();
builder.Services.AddScoped<IItemMasterDropDownBL, ItemMasterDropDownBL>();

//bl class
builder.Services.AddScoped<IuserBL, UserBL>();
builder.Services.AddScoped<IMenuCategoryBL, MenuCategoryBL>();
builder.Services.AddScoped<IOrderBL, OrderBL>();
builder.Services.AddScoped<IItemMasterBL, ItemMasterBL>();
builder.Services.AddScoped<IImageMasterDAL, ImageMasterDAL>();
builder.Services.AddScoped<IItemMasterDropDownDAL, ItemMasterDropDownDAL>();
builder.Services.AddScoped<IAddressMasterDAL, AddressMasterDAL>();
builder.Services.AddScoped<IAddressMasterBL, AddressMasterBL>();
builder.Services.AddScoped<ICookAssignDAL, CookAssignDAL>();
builder.Services.AddScoped<ICookAssignBL, CookAssignBL>();
builder.Services.AddScoped<IDeliveryAssignmentDAL, DeliveryAssignmentDAL>();
builder.Services.AddScoped<IDeliveryAssignmentBL, DeliveryAssignmentBL>();

// Theme Management
builder.Services.AddScoped<Repository.DAL.Interface.Master.ThemeMaster.IThemeMasterDAL, Repository.DAL.Imple.Master.ThemeMaster.ThemeMasterDAL>();
builder.Services.AddScoped<Services.BL.Interface.Master.ThemeMaster.IThemeMasterBL, Services.BL.Imple.Master.ThemeMaster.ThemeMasterBL>();


// Customer Home Page Dashboard stats
builder.Services.AddScoped<Repository.DAL.Interface.Master.CustomerHome.ICustomerHomeDAL, Repository.DAL.Imple.Master.CustomerHome.CustomerHomeDAL>();
builder.Services.AddScoped<OFMS_API.BL.Interface.Master.CustomerHome.ICustomerHomeBL, OFMS_API.BL.Imple.Master.CustomerHome.CustomerHomeBL>();

// Cook Module (For the Cook's Dashboard)
builder.Services.AddScoped<OFMS_API.Repository.DAL.Interface.CookModule.ICookModuleDAL, OFMS_API.Repository.DAL.Imple.CookModule.CookModuleDAL>();
builder.Services.AddScoped<OFMS_API.Services.BL.Interface.CookModule.ICookModuleBL, OFMS_API.Services.BL.Imple.CookModule.CookModuleBL>();

// RBAC Permissions
builder.Services.AddScoped<OFMS_API.Repository.DAL.Interface.Permission.IPermissionDAL, OFMS_API.Repository.DAL.Imple.Permission.PermissionDAL>();
builder.Services.AddScoped<OFMS_API.BL.Interface.Permission.IPermissionBL, OFMS_API.BL.Imple.Permission.PermissionBL>();

// Notifications
builder.Services.AddScoped<INotificationDAL, NotificationDAL>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationMasterDAL, NotificationMasterDAL>();
builder.Services.AddScoped<INotificationMasterBL, NotificationMasterBL>();

// Cart Module
builder.Services.AddScoped<OFMS_API.DAL.Interface.ICartRepository, OFMS_API.DAL.Imple.CartRepository>();
builder.Services.AddScoped<OFMS_API.BL.Interface.ICartBL, OFMS_API.BL.Imple.CartBL>();

// Chat Module
builder.Services.AddScoped<IChatDAL, ChatDAL>();
builder.Services.AddScoped<IChatBL, ChatBL>();


// Updated CORS policy to include your Angular app's origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // allows any origin
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for SignalR over WebSocket with Tokens

    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/notificationHub") || path.StartsWithSegments("/chatHub")))
            {
                context.Token = accessToken;
                return Task.CompletedTask;
            }

            var token = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(token))
            {
                if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = token;
                }
                else
                {
                    context.Token = token.Substring("Bearer ".Length).Trim();
                }
            }

            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                message = "Token is required"
            });
            return context.Response.WriteAsync(result);
        }
    };
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
// CORS middleware should be placed here
app.UseCors("AllowAngularApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = "swagger";
    });
}
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        @"D:\Project\OFMS\OFMS_API\OFMS_API\Images\UserProfileImages\"),
    RequestPath = "/Images/UserProfileImages"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        @"D:\Project\OFMS\OFMS_API\OFMS_API\Images\ProductImages\"),
    RequestPath = "/ProductImages"
});

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ApiLoggingMiddleware>();

app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<ChatHub>("/chatHub");

app.Run();