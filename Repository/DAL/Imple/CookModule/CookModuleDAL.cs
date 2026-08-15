using Dapper;
using DTO.Models.CommonModel;
using DTO.Models.CookModule;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OFMS_API.Repository.DAL.Interface.CookModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFMS_API.Repository.DAL.Imple.CookModule
{
    public class CookModuleDAL : ICookModuleDAL
    {
        private readonly string _connectionString;

        public CookModuleDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<CookDashboardCountsTO> GetCookDashboardCounts(int cookUserId)
        {
            using var conn = new SqlConnection(_connectionString);
            string sql = @"
                SELECT 
                    SUM(CASE WHEN c.IdStatus = 8 THEN 1 ELSE 0 END) AS AssignedOrders,
                    SUM(CASE WHEN c.IdStatus = 10 THEN 1 ELSE 0 END) AS PreparingOrders,
                    SUM(CASE WHEN c.IdStatus = 11 THEN 1 ELSE 0 END) AS ReadyOrders,
                    SUM(CASE WHEN c.IdStatus IN (11, 6) AND CAST(c.UpdatedOn AS DATE) = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS CompletedToday,
                    SUM(CASE WHEN c.IdStatus IN (8, 9, 10) THEN 1 ELSE 0 END) AS PendingOrders
                FROM tblCookAssignment c
                WHERE c.CookUserId = @CookUserId AND c.IsActive = 1;
            ";

            var counts = await conn.QueryFirstOrDefaultAsync<CookDashboardCountsTO>(sql, new { CookUserId = cookUserId });
            return counts ?? new CookDashboardCountsTO();
        }

        public async Task<OutPutClass<CookOrderListResponseTO>> GetMyAssignedOrders(int cookUserId, FilterModelTO filter, bool completedHistory)
        {
            using var conn = new SqlConnection(_connectionString);
            var result = new OutPutClass<CookOrderListResponseTO> { List = new List<CookOrderListResponseTO>(), TotalCount = 0 };

            int pageNo = filter.PageNo ?? 1;
            int pageSize = filter.PageSize ?? 10;
            int offset = (pageNo - 1) * pageSize;

            var sb = new StringBuilder(@"
                SELECT 
                    g.IdCookAssignment,
                    ISNULL(g.IdOrderMaster, -g.IdCookAssignment) AS IdOrderMaster,
                    ISNULL(o.OrderNo, 'MERGED-BATCH') AS OrderNumber,
                    ISNULL(u.UserName, 'Multiple Customers') AS CustomerName,
                    g.AssignedOn,
                    g.AcceptedOn,
                    g.EstimatedPreparationTime,
                    g.IdStatus,
                    s.StatusName,
                    s.ColorCode AS StatusColorCode,
                    COALESCE(
                        g.TotalQuantity,
                        (SELECT SUM(Quantity) FROM tblOrderDetails WHERE IdOrderMaster = g.IdOrderMaster)
                    ) AS TotalItems,
                    g.CompletedOn,
                    g.IsMerged
                FROM (
                    SELECT 
                        MIN(IdCookAssignment) AS IdCookAssignment,
                        IdOrderMaster,
                        MIN(IdStatus) AS IdStatus,
                        MIN(AssignedOn) AS AssignedOn,
                        MIN(AcceptedOn) AS AcceptedOn,
                        MAX(EstimatedPreparationTime) AS EstimatedPreparationTime,
                        MIN(ReadyOn) AS CompletedOn,
                        MIN(CAST(IsMerged AS INT)) AS IsMerged,
                        MAX(TotalQuantity) AS TotalQuantity
                    FROM tblCookAssignment
                    WHERE CookUserId = @CookUserId AND IsActive = 1
                    GROUP BY ISNULL(IdOrderMaster, -IdCookAssignment), IdOrderMaster
                ) g
                LEFT JOIN tblOrderMaster o ON g.IdOrderMaster = o.IdOrderMaster
                LEFT JOIN tbluser u ON o.CustomerId = u.UserId
                LEFT JOIN dimStatus s ON g.IdStatus = s.IdStatus
                WHERE 1 = 1
            ");

            if (completedHistory)
            {
                // Status 11 (Ready) or order completed etc, we assume 11 is Cook Ready
                sb.Append(" AND g.IdStatus >= 11 "); 
            }
            else
            {
                // Active assignments (Assigned 8, Accepted 9, Preparing 10)
                sb.Append(" AND g.IdStatus IN (8, 9, 10, 101) "); // include any merged assigned states
            }

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                sb.Append(" AND (ISNULL(o.OrderNo, 'MERGED-BATCH') LIKE @Search OR ISNULL(u.UserName, 'Multiple') LIKE @Search) ");
            }

            string sortColumn = string.IsNullOrEmpty(filter.SortColumn) ? "g.AssignedOn" : filter.SortColumn;
            // Prevent basic injection on sort column
            if (!sortColumn.Contains(".")) sortColumn = "g." + sortColumn;
            string sortOrder = string.IsNullOrEmpty(filter.SortOrder) ? "DESC" : filter.SortOrder.ToUpper();

            var countQuery = $"SELECT COUNT(1) FROM ({sb.ToString()}) AS SubQuery";
            
            sb.Append($" ORDER BY {sortColumn} {sortOrder} ");
            sb.Append(" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY ");

            var parameters = new
            {
                CookUserId = cookUserId,
                Search = $"%{filter.SearchText}%",
                Offset = offset,
                PageSize = pageSize
            };

            var list = await conn.QueryAsync<CookOrderListResponseTO>(sb.ToString(), parameters);
            var total = await conn.ExecuteScalarAsync<int>(countQuery, parameters);

            result.List = list.ToList();
            result.TotalCount = total;

            return result;
        }

        public async Task<CookOrderDetailResponseTO> GetOrderDetailsForCook(int cookUserId, int orderId)
        {
            try { 
            using var conn = new SqlConnection(_connectionString);
            
            if (orderId <= 0)
            {
                int idCookAssignment = -orderId;
                string sqlMerged = @"
                    SELECT 
                        c.IdCookAssignment,
                        0 AS IdOrderMaster,
                        'MERGED-BATCH' AS OrderNumber,
                        GETDATE() AS OrderDate,
                        'Multiple Customers' AS CustomerName,
                        'N/A' AS ContactNumber,
                        'N/A' AS DeliveryAddress,
                        c.AssignedOn,
                        c.AcceptedOn,
                        c.StartCookingOn,
                        c.ReadyOn,
                        c.EstimatedPreparationTime,
                        c.ActualPreparationTime,
                        c.Remarks AS CookRemark,
                        c.IdStatus,
                        s.StatusName,
                        s.ColorCode AS StatusColorCode,
                        1 AS OrderStatusId
                    FROM tblCookAssignment c
                    LEFT JOIN dimStatus s ON c.IdStatus = s.IdStatus
                    WHERE c.IdCookAssignment = @IdCookAssignment AND c.CookUserId = @CookUserId AND c.IsActive = 1;
                ";
                var mergedDetails = await conn.QueryFirstOrDefaultAsync<CookOrderDetailResponseTO>(sqlMerged, new { IdCookAssignment = idCookAssignment, CookUserId = cookUserId });

                if (mergedDetails != null)
                {
                    string itemsSql = @"
                        SELECT 
                            0 AS IdOrderDetails,
                            i.ItemName,
                            c.TotalQuantity AS Qty,
                            NULL AS ItemRemark,
                            c.IdCookAssignment,
                            c.IdStatus,
                            s.StatusName
                        FROM tblCookAssignment c
                        INNER JOIN tblItemMaster i ON c.IdItemMaster = i.IdItemMaster
                        LEFT JOIN dimStatus s ON c.IdStatus = s.IdStatus
                        WHERE c.IdCookAssignment = @IdCookAssignment AND c.IsActive = 1;
                    ";
                    var items = await conn.QueryAsync<CookOrderItemTO>(itemsSql, new { IdCookAssignment = idCookAssignment });
                    mergedDetails.Items = items.ToList();
                }
                return mergedDetails;
            }

            string sql = @"
                SELECT 
                    c.IdCookAssignment,
                    c.IdOrderMaster,
                    o.OrderNo AS OrderNumber,
                    o.CreatedOn AS OrderDate,
                    u.UserName AS CustomerName,
                    u.Phone_Number AS ContactNumber,
                    a.AddressLine1 + ', ' + city.CityName AS DeliveryAddress,
                    c.AssignedOn,
                    c.AcceptedOn,
                    c.StartCookingOn,
                    c.ReadyOn,
                    c.EstimatedPreparationTime,
                    c.ActualPreparationTime,
                    c.Remarks AS CookRemark,
                    c.IdStatus,
                    s.StatusName,
                    s.ColorCode AS StatusColorCode,
                    o.IdStatus AS OrderStatusId
                FROM tblCookAssignment c
                INNER JOIN tblOrderMaster o ON c.IdOrderMaster = o.IdOrderMaster
                LEFT JOIN tbluser u ON o.CustomerId = u.UserId
                LEFT JOIN tblAddress a ON o.IdAddressMapping = a.IdAddress
                LEFT JOIN dimCity city ON a.IdCity = city.IdCity
                LEFT JOIN dimStatus s ON c.IdStatus = s.IdStatus
                WHERE c.IdOrderMaster = @OrderId AND c.CookUserId = @CookUserId AND c.IsActive = 1;
            ";

            var orderDetails = await conn.QueryFirstOrDefaultAsync<CookOrderDetailResponseTO>(sql, new { OrderId = orderId, CookUserId = cookUserId });
            
            if (orderDetails != null)
            {
                string itemsSql = @"
                    SELECT 
                        d.IdOrderDetails,
                        i.ItemName,
                        d.Quantity AS Qty,
                        NULL AS ItemRemark,
                        c.IdCookAssignment,
                        c.IdStatus,
                        s.StatusName
                    FROM tblCookAssignment c
                    INNER JOIN tblOrderDetails d ON c.IdOrderDetails = d.IdOrderDetails
                    INNER JOIN tblItemMaster i ON d.IdItemMaster = i.IdItemMaster
                    LEFT JOIN dimStatus s ON c.IdStatus = s.IdStatus
                    WHERE c.IdOrderMaster = @OrderId AND c.CookUserId = @CookUserId AND c.IsActive = 1;
                ";
                var items = await conn.QueryAsync<CookOrderItemTO>(itemsSql, new { OrderId = orderId, CookUserId = cookUserId });
                orderDetails.Items = items.ToList();
            }

            return orderDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> AcceptOrder(int cookUserId, AcceptOrderRequestTO request)
        {
            using var conn = new SqlConnection(_connectionString);
            string sql;
            object parameters;

            if (request.IdCookAssignments != null && request.IdCookAssignments.Any())
            {
                sql = @"
                    UPDATE tblCookAssignment 
                    SET IdStatus = 9, -- Accepted
                        AcceptedOn = GETDATE(),
                        EstimatedPreparationTime = @EstimatedMinutes,
                        Remarks = @Remark,
                        UpdatedOn = GETDATE(),
                        UpdatedBy = @CookUserId
                    WHERE IdCookAssignment IN @IdCookAssignments AND CookUserId = @CookUserId AND IdStatus = 8 AND IsActive = 1
                ";
                parameters = new {
                    request.EstimatedMinutes,
                    request.Remark,
                    IdCookAssignments = request.IdCookAssignments,
                    CookUserId = cookUserId
                };
            }
            else
            {
                sql = @"
                    UPDATE tblCookAssignment 
                    SET IdStatus = 9, -- Accepted
                        AcceptedOn = GETDATE(),
                        EstimatedPreparationTime = @EstimatedMinutes,
                        Remarks = @Remark,
                        UpdatedOn = GETDATE(),
                        UpdatedBy = @CookUserId
                    WHERE IdOrderMaster = @IdOrderMaster AND CookUserId = @CookUserId AND IdStatus = 8 AND IsActive = 1
                ";
                parameters = new {
                    request.EstimatedMinutes,
                    request.Remark,
                    request.IdOrderMaster,
                    CookUserId = cookUserId
                };
            }
            
            int rowsAffected = await conn.ExecuteAsync(sql, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateEstimatedTime(int cookUserId, UpdateEstimatedTimeRequestTO request)
        {
            using var conn = new SqlConnection(_connectionString);
            string sql;
            object parameters;

            if (request.IdOrderMaster <= 0)
            {
                sql = @"
                    UPDATE tblCookAssignment 
                    SET EstimatedPreparationTime = @EstimatedMinutes,
                        UpdatedOn = GETDATE(),
                        UpdatedBy = @CookUserId
                    WHERE IdCookAssignment = @IdCookAssignment AND CookUserId = @CookUserId AND IdStatus IN (8,9,10) AND IsActive = 1
                ";
                parameters = new {
                    request.EstimatedMinutes,
                    IdCookAssignment = -request.IdOrderMaster,
                    CookUserId = cookUserId
                };
            }
            else
            {
                sql = @"
                    UPDATE tblCookAssignment 
                    SET EstimatedPreparationTime = @EstimatedMinutes,
                        UpdatedOn = GETDATE(),
                        UpdatedBy = @CookUserId
                    WHERE IdOrderMaster = @IdOrderMaster AND CookUserId = @CookUserId AND IdStatus IN (8,9,10) AND IsActive = 1
                ";
                parameters = new {
                    request.EstimatedMinutes,
                    request.IdOrderMaster,
                    CookUserId = cookUserId
                };
            }
            
            int rowsAffected = await conn.ExecuteAsync(sql, parameters);

            return rowsAffected > 0;
        }

        public async Task<int> UpdateCookingStatus(int cookUserId, UpdateCookingStatusRequestTO request)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                string caSql;
                object parameters;

                if (request.IdCookAssignments != null && request.IdCookAssignments.Any())
                {
                    caSql = @"
                        UPDATE tblCookAssignment 
                        SET IdStatus = @NewStatusId,
                            Remarks = CASE WHEN @Remark IS NOT NULL AND @Remark != '' THEN @Remark ELSE Remarks END,
                            UpdatedOn = GETDATE(),
                            UpdatedBy = @CookUserId,
                            StartCookingOn = CASE WHEN @NewStatusId = 10 AND StartCookingOn IS NULL THEN GETDATE() ELSE StartCookingOn END,
                            ReadyOn = CASE WHEN @NewStatusId = 11 AND ReadyOn IS NULL THEN GETDATE() ELSE ReadyOn END,
                            ActualPreparationTime = CASE WHEN @NewStatusId = 11 AND StartCookingOn IS NOT NULL THEN DATEDIFF(MINUTE, StartCookingOn, GETDATE()) ELSE ActualPreparationTime END
                        WHERE IdCookAssignment IN @IdCookAssignments AND CookUserId = @CookUserId AND IsActive = 1
                    ";
                    parameters = new {
                        request.NewStatusId,
                        request.Remark,
                        IdCookAssignments = request.IdCookAssignments,
                        CookUserId = cookUserId
                    };
                }
                else
                {
                    caSql = @"
                        UPDATE tblCookAssignment 
                        SET IdStatus = @NewStatusId,
                            Remarks = CASE WHEN @Remark IS NOT NULL AND @Remark != '' THEN @Remark ELSE Remarks END,
                            UpdatedOn = GETDATE(),
                            UpdatedBy = @CookUserId,
                            StartCookingOn = CASE WHEN @NewStatusId = 10 AND StartCookingOn IS NULL THEN GETDATE() ELSE StartCookingOn END,
                            ReadyOn = CASE WHEN @NewStatusId = 11 AND ReadyOn IS NULL THEN GETDATE() ELSE ReadyOn END,
                            ActualPreparationTime = CASE WHEN @NewStatusId = 11 AND StartCookingOn IS NOT NULL THEN DATEDIFF(MINUTE, StartCookingOn, GETDATE()) ELSE ActualPreparationTime END
                        WHERE IdOrderMaster = @IdOrderMaster AND CookUserId = @CookUserId AND IsActive = 1
                    ";
                    parameters = new {
                        request.NewStatusId,
                        request.Remark,
                        request.IdOrderMaster,
                        CookUserId = cookUserId
                    };
                }

                int affected = await conn.ExecuteAsync(caSql, parameters, tx);

                if (affected == 0)
                {
                    tx.Rollback();
                    return 0;
                }

                int orderId = request.IdOrderMaster;
                if (orderId == 0 && request.IdCookAssignments != null && request.IdCookAssignments.Any())
                {
                    orderId = await conn.QueryFirstOrDefaultAsync<int>(
                        "SELECT TOP 1 IdOrderMaster FROM tblCookAssignment WHERE IdCookAssignment = @Id",
                        new { Id = request.IdCookAssignments.First() }, tx);
                }

                tx.Commit();
                return orderId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<List<CookReportItemTO>> GetCookHistoryReportData(CookReportFilterTO filter)
        {
            using var conn = new SqlConnection(_connectionString);
            
            var sql = @"
                SELECT 
                    c.IdCookAssignment,
                    c.IdOrderMaster,
                    o.OrderNo,
                    uCust.UserName AS CustomerName,
                    c.CookUserId,
                    uCook.UserName AS CookName,
                    ISNULL(im.ItemName, N'Merged / Bulk Order') AS ItemName,
                    ISNULL(c.TotalQuantity, od.Quantity) AS Quantity,
                    c.AssignedOn,
                    c.AcceptedOn,
                    c.StartCookingOn,
                    c.ReadyOn,
                    c.EstimatedPreparationTime,
                    c.ActualPreparationTime,
                    c.IdStatus,
                    s.StatusName,
                    c.IsMerged,
                    c.Remarks
                FROM tblCookAssignment c
                LEFT JOIN tblOrderMaster o ON c.IdOrderMaster = o.IdOrderMaster
                LEFT JOIN tblOrderDetails od ON c.IdOrderDetails = od.IdOrderDetails
                LEFT JOIN tblItemMaster im ON od.IdItemMaster = im.IdItemMaster OR c.IdItemMaster = im.IdItemMaster
                LEFT JOIN tblUser uCust ON o.CustomerId = uCust.UserId
                LEFT JOIN tblUser uCook ON c.CookUserId = uCook.UserId
                LEFT JOIN dimStatus s ON c.IdStatus = s.IdStatus
                WHERE c.IsActive = 1
            ";

            var builder = new System.Text.StringBuilder(sql);
            var dynamicParams = new DynamicParameters();

            if (filter.CookUserId.HasValue && filter.CookUserId.Value > 0)
            {
                builder.AppendLine("  AND c.CookUserId = @CookUserId");
                dynamicParams.Add("CookUserId", filter.CookUserId.Value);
            }

            if (filter.IdStatus.HasValue && filter.IdStatus.Value > 0)
            {
                if (filter.IdStatus.Value == 11)
                {
                    builder.AppendLine("  AND c.IdStatus >= 11");
                }
                else
                {
                    builder.AppendLine("  AND c.IdStatus = @IdStatus");
                    dynamicParams.Add("IdStatus", filter.IdStatus.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                builder.AppendLine("  AND (o.OrderNo LIKE @SearchText OR uCust.UserName LIKE @SearchText OR im.ItemName LIKE @SearchText OR uCook.UserName LIKE @SearchText)");
                dynamicParams.Add("SearchText", $"%{filter.SearchText.Trim()}%");
            }

            string targetDateCol = "ISNULL(c.ReadyOn, c.AssignedOn)";

            if (filter.FilterType == "Day" && filter.FromDate.HasValue)
            {
                builder.AppendLine($"  AND CAST({targetDateCol} AS DATE) = CAST(@FromDate AS DATE)");
                dynamicParams.Add("FromDate", filter.FromDate.Value);
            }
            else if (filter.FilterType == "Month" && filter.SelectedMonth.HasValue && filter.SelectedYear.HasValue)
            {
                builder.AppendLine($"  AND MONTH({targetDateCol}) = @SelectedMonth AND YEAR({targetDateCol}) = @SelectedYear");
                dynamicParams.Add("SelectedMonth", filter.SelectedMonth.Value);
                dynamicParams.Add("SelectedYear", filter.SelectedYear.Value);
            }
            else if (filter.FilterType == "Year" && filter.SelectedYear.HasValue)
            {
                builder.AppendLine($"  AND YEAR({targetDateCol}) = @SelectedYear");
                dynamicParams.Add("SelectedYear", filter.SelectedYear.Value);
            }
            else if (filter.FilterType == "Range")
            {
                if (filter.FromDate.HasValue)
                {
                    builder.AppendLine($"  AND {targetDateCol} >= @FromDate");
                    dynamicParams.Add("FromDate", filter.FromDate.Value);
                }
                if (filter.ToDate.HasValue)
                {
                    builder.AppendLine($"  AND {targetDateCol} <= @ToDate");
                    dynamicParams.Add("ToDate", filter.ToDate.Value.Date.AddDays(1).AddTicks(-1));
                }
            }

            builder.AppendLine(" ORDER BY c.IdCookAssignment DESC");

            var result = await conn.QueryAsync<CookReportItemTO>(builder.ToString(), dynamicParams);
            return result.AsList();
        }
    }
}
