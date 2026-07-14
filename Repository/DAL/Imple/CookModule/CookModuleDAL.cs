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
                    g.IdOrderMaster,
                    o.OrderNo AS OrderNumber,
                    u.UserName AS CustomerName,
                    g.AssignedOn,
                    g.AcceptedOn,
                    g.EstimatedPreparationTime,
                    g.IdStatus,
                    s.StatusName,
                    s.ColorCode AS StatusColorCode,
                    (SELECT SUM(Quantity) FROM tblOrderDetails WHERE IdOrderMaster = g.IdOrderMaster) AS TotalItems,
                    g.CompletedOn
                FROM (
                    SELECT 
                        MIN(IdCookAssignment) AS IdCookAssignment,
                        IdOrderMaster,
                        MIN(IdStatus) AS IdStatus,
                        MIN(AssignedOn) AS AssignedOn,
                        MIN(AcceptedOn) AS AcceptedOn,
                        MAX(EstimatedPreparationTime) AS EstimatedPreparationTime,
                        MIN(ReadyOn) AS CompletedOn
                    FROM tblCookAssignment
                    WHERE CookUserId = @CookUserId AND IsActive = 1
                    GROUP BY IdOrderMaster
                ) g
                INNER JOIN tblOrderMaster o ON g.IdOrderMaster = o.IdOrderMaster
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
                sb.Append(" AND g.IdStatus IN (8, 9, 10) ");
            }

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                sb.Append(" AND (o.OrderNo LIKE @Search OR u.UserName LIKE @Search) ");
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
            string sql = @"
                UPDATE tblCookAssignment 
                SET EstimatedPreparationTime = @EstimatedMinutes,
                    UpdatedOn = GETDATE(),
                    UpdatedBy = @CookUserId
                WHERE IdOrderMaster = @IdOrderMaster AND CookUserId = @CookUserId AND IdStatus IN (8,9,10) AND IsActive = 1
            ";
            
            int rowsAffected = await conn.ExecuteAsync(sql, new {
                request.EstimatedMinutes,
                request.IdOrderMaster,
                CookUserId = cookUserId
            });

            return rowsAffected > 0;
        }

        public async Task<bool> UpdateCookingStatus(int cookUserId, UpdateCookingStatusRequestTO request)
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
                    return false;
                }

                // If cook is Ready (11), update OrderMaster to Ready (4) ONLY when all items are ready
                if (request.NewStatusId == 11)
                {
                    int orderId = request.IdOrderMaster;
                    if (orderId == 0 && request.IdCookAssignments != null && request.IdCookAssignments.Any())
                    {
                        orderId = await conn.QueryFirstOrDefaultAsync<int>(
                            "SELECT TOP 1 IdOrderMaster FROM tblCookAssignment WHERE IdCookAssignment = @Id",
                            new { Id = request.IdCookAssignments.First() }, tx);
                    }

                    string totalSql = "SELECT ISNULL(SUM(Quantity), 0) FROM tblOrderDetails WHERE IdOrderMaster = @OrderId";
                    string readySql = @"
                        SELECT ISNULL(SUM(d.Quantity), 0) 
                        FROM tblCookAssignment c
                        INNER JOIN tblOrderDetails d ON c.IdOrderDetails = d.IdOrderDetails
                        WHERE c.IdOrderMaster = @OrderId AND c.IdStatus = 11 AND c.IsActive = 1";

                    int totalItems = await conn.ExecuteScalarAsync<int>(totalSql, new { OrderId = orderId }, tx);
                    int readyQuantity = await conn.ExecuteScalarAsync<int>(readySql, new { OrderId = orderId }, tx);

                    if (totalItems > 0 && totalItems == readyQuantity)
                    {
                        string orderSql = @"
                            UPDATE tblOrderMaster
                            SET IdStatus = 4, -- Order Ready
                                UpdatedOn = GETDATE(),
                                UpdatedBy = @CookUserId
                            WHERE IdOrderMaster = @OrderId
                        ";
                        await conn.ExecuteAsync(orderSql, new { OrderId = orderId, CookUserId = cookUserId }, tx);
                    }
                }

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
