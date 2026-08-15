using Dapper;
using DTO.Models.CommonModel;
using DTO.Models.Master.OrderMaster;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Repository.DAL.Interface.Master.CookAssignment;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.DAL.Imple.Master.CookAssignment
{
    public class CookAssignDAL : ICookAssignDAL
    {
        private readonly string connq;

        public CookAssignDAL(IConfiguration configuration)
        {
            connq = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task<int> CreateCookAssignmentDAL(List<CreateCookAssignmentTO> models)
        {
            using var conn = new SqlConnection(connq);
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();
            
            try
            {
                int totalInserted = 0;
                string sql = @"INSERT INTO tblCookAssignment 
                               (IdOrderMaster, IdOrderDetails, CookUserId, IdStatus, AssignedOn, EstimatedPreparationTime, Remarks, IsActive, CreatedOn, CreatedBy) 
                               VALUES 
                               (@IdOrderMaster, @IdOrderDetails, @CookUserId, @IdStatus, @AssignedOn, @EstimatedPreparationTime, @Remarks, @IsActive, @CreatedOn, @CreatedBy);
                               SELECT CAST(SCOPE_IDENTITY() as int);";

                foreach (var model in models)
                {
                    var parameter = new DynamicParameters();
                    parameter.Add("@IdOrderMaster", model.IdOrderMaster, DbType.Int32);
                    parameter.Add("@IdOrderDetails", model.IdOrderDetails, DbType.Int32);
                    parameter.Add("@CookUserId", model.CookUserId, DbType.Int32);
                    parameter.Add("@IdStatus", model.IdStatus, DbType.Int32);
                    parameter.Add("@AssignedOn", DateTime.Now, DbType.DateTime);
                    parameter.Add("@EstimatedPreparationTime", model.EstimatedPreparationTime, DbType.Int32);
                    parameter.Add("@Remarks", model.Remarks, DbType.String);
                    parameter.Add("@IsActive", true, DbType.Boolean);
                    parameter.Add("@CreatedOn", DateTime.Now, DbType.DateTime);
                    parameter.Add("@CreatedBy", model.CookUserId, DbType.Int32); 

                    var id = await conn.ExecuteScalarAsync<int>(sql, parameter, transaction: tran);
                    if (id > 0) totalInserted++;
                }
                
                tran.Commit();
                return totalInserted;
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }

        public async Task<int> UpdateKitchenStatusDAL(UpdateKitchenStatusTO model)
        {
            using var conn = new SqlConnection(connq);
            var parameter = new DynamicParameters();
            parameter.Add("@IdCookAssignment", model.IdCookAssignment, DbType.Int32);
            parameter.Add("@IdStatus", model.IdStatus, DbType.Int32);
            parameter.Add("@Remarks", model.Remarks, DbType.String);
            parameter.Add("@UpdatedBy", model.UpdatedBy, DbType.Int32);
            parameter.Add("@UpdatedOn", DateTime.Now, DbType.DateTime);

            string sql = @"UPDATE tblCookAssignment SET 
                           IdStatus = @IdStatus, 
                           Remarks = @Remarks, 
                           UpdatedBy = @UpdatedBy, 
                           UpdatedOn = @UpdatedOn 
                           WHERE IdCookAssignment = @IdCookAssignment";

            return await conn.ExecuteAsync(sql, parameter);
        }

        public async Task<List<CookAssignmentResponseTO>> GetCookAssignmentListDAL(FilterModelTO filterModelTO)
        {
            using var conn = new SqlConnection(connq);

            int pageNo = filterModelTO.PageNo ?? 1;
            int pageSize = filterModelTO.PageSize ?? 10;
            bool fetchAll = pageNo == 0 && pageSize == 0;
            int offset = fetchAll ? 0 : (pageNo - 1) * pageSize;

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@SearchText", string.IsNullOrWhiteSpace(filterModelTO.SearchText) ? null : $"%{filterModelTO.SearchText}%", DbType.String);
            parameters.Add("@IsActive", filterModelTO.isActive, DbType.Boolean);
            parameters.Add("@PageSize", pageSize, DbType.Int32);
            parameters.Add("@PageNo", pageNo, DbType.Int32);
            string sql = @"
    ;WITH Paginated AS (
        SELECT 
            c.IdCookAssignment,
            c.IdOrderMaster,
            c.IdOrderDetails,
            c.IdItemMaster,
            c.TotalQuantity,
            c.IsMerged,
            (SELECT STRING_AGG(CAST(m.IdOrderDetails AS VARCHAR), ',') FROM tblCookAssignmentMapping m WHERE m.IdCookAssignment = c.IdCookAssignment) AS MappedOrderDetailsIds,
            (SELECT STRING_AGG(CAST(m.IdOrderMaster AS VARCHAR), ',') FROM tblCookAssignmentMapping m WHERE m.IdCookAssignment = c.IdCookAssignment) AS MappedOrderMasterIds,
            (SELECT STRING_AGG(om.OrderNo, ', ') FROM tblCookAssignmentMapping m INNER JOIN tblOrderMaster om ON m.IdOrderMaster = om.IdOrderMaster WHERE m.IdCookAssignment = c.IdCookAssignment) AS MappedOrderNos,
            o.OrderNo, 
            c.CookUserId,
            u.UserName AS CookName,
            c.IdStatus,
            s.StatusName,
            c.AssignedOn,
            c.AcceptedOn,
            c.StartCookingOn,
            c.ReadyOn,
            c.EstimatedPreparationTime,
            c.ActualPreparationTime,
            c.Remarks,
            ROW_NUMBER() OVER (ORDER BY c.IdCookAssignment DESC) AS RowNum
        FROM tblCookAssignment c
        LEFT JOIN tblOrderMaster o ON c.IdOrderMaster = o.IdOrderMaster
        LEFT JOIN tblUser u ON c.CookUserId = u.UserId
        LEFT JOIN dimStatus s ON c.IdStatus = s.IdStatus
        WHERE (@SearchText IS NULL OR @SearchText = ''
               OR (o.OrderNo LIKE @SearchText 
                   OR u.UserName LIKE @SearchText 
                   OR s.StatusName LIKE @SearchText))
          AND (@IsActive IS NULL OR c.IsActive = @IsActive)
    )
    SELECT *
    FROM Paginated
    WHERE (@PageNo = 0 AND @PageSize = 0)
       OR RowNum BETWEEN ((@PageNo - 1) * @PageSize + 1)
                     AND (@PageNo * @PageSize);";
            var result = await conn.QueryAsync<CookAssignmentResponseTO>(sql, parameters);
            return result.ToList();
        }

        public async Task<List<MergeableItemResponseTO>> GetMergeableCookItemsDAL()
        {
            using var conn = new SqlConnection(connq);
            // Get all pending order items that are not yet assigned to any active cook assignment
            // (Either not assigned directly, or not part of a merged assignment)
            string sql = @"
                SELECT 
                    od.IdItemMaster,
                    im.ItemName,
                    od.Quantity,
                    om.IdOrderMaster,
                    om.OrderNo,
                    od.IdOrderDetails,
                    u.UserName AS CustomerName,
                    om.IdStatus AS CurrentStatusId,
                    s.StatusName AS CurrentStatusName
                FROM tblOrderDetails od
                INNER JOIN tblOrderMaster om ON od.IdOrderMaster = om.IdOrderMaster
                INNER JOIN tblItemMaster im ON od.IdItemMaster = im.IdItemMaster
                INNER JOIN tblUser u ON om.CustomerId = u.UserId
                LEFT JOIN dimStatus s ON om.IdStatus = s.IdStatus
                WHERE om.IdStatus = 2 -- Pending / Confirmed
                  AND od.IdOrderDetails NOT IN (
                      SELECT IdOrderDetails FROM tblCookAssignment WHERE IsActive = 1 AND IdOrderDetails IS NOT NULL
                      UNION
                      SELECT tblCookAssignmentMapping.IdOrderDetails FROM tblCookAssignmentMapping 
                      INNER JOIN tblCookAssignment ca ON tblCookAssignmentMapping.IdCookAssignment = ca.IdCookAssignment 
                      WHERE ca.IsActive = 1
                  )";

            var allPendingItems = await conn.QueryAsync<dynamic>(sql);

            var grouped = allPendingItems.GroupBy(x => new { x.IdItemMaster, x.ItemName })
                .Select(g => new MergeableItemResponseTO
                {
                    IdItemMaster = (int)g.Key.IdItemMaster,
                    ItemName = (string)g.Key.ItemName,
                    TotalQuantity = g.Sum(x => (int)x.Quantity),
                    OrderCount = g.Select(x => (int)x.IdOrderMaster).Distinct().Count(),
                    Orders = g.Select(x => new MergeableSourceOrderTO
                    {
                        IdOrderMaster = (int)x.IdOrderMaster,
                        OrderNo = (string)x.OrderNo,
                        IdOrderDetails = (int)x.IdOrderDetails,
                        Quantity = (int)x.Quantity,
                        CustomerName = (string)x.CustomerName,
                        CurrentStatusId = (int)x.CurrentStatusId,
                        CurrentStatusName = (string)x.CurrentStatusName
                    }).ToList()
                }).Where(g => g.OrderCount > 1).ToList(); // Only return items in multiple orders

            return grouped;
        }

        public async Task<int> AssignMergedCookItemDAL(MergedCookAssignmentRequestTO model)
        {
            using var conn = new SqlConnection(connq);
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();

            try
            {
                int totalQty = model.SourceOrders.Sum(x => x.Quantity);

                string sqlMaster = @"
                    INSERT INTO tblCookAssignment 
                    (IdItemMaster, TotalQuantity, IsMerged, CookUserId, IdStatus, AssignedOn, EstimatedPreparationTime, Remarks, IsActive, CreatedOn, CreatedBy) 
                    VALUES 
                    (@IdItemMaster, @TotalQuantity, 1, @CookUserId, @IdStatus, @AssignedOn, @EstimatedPreparationTime, @Remarks, 1, @CreatedOn, @CreatedBy);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new DynamicParameters();
                parameters.Add("@IdItemMaster", model.IdItemMaster, DbType.Int32);
                parameters.Add("@TotalQuantity", totalQty, DbType.Int32);
                parameters.Add("@CookUserId", model.CookUserId, DbType.Int32);
                parameters.Add("@IdStatus", model.IdStatus, DbType.Int32);
                parameters.Add("@AssignedOn", DateTime.Now, DbType.DateTime);
                parameters.Add("@EstimatedPreparationTime", model.EstimatedPreparationTime, DbType.Int32);
                parameters.Add("@Remarks", model.Remarks, DbType.String);
                parameters.Add("@CreatedOn", DateTime.Now, DbType.DateTime);
                parameters.Add("@CreatedBy", model.CookUserId, DbType.Int32);

                int newId = await conn.ExecuteScalarAsync<int>(sqlMaster, parameters, transaction: tran);

                if (newId > 0)
                {
                    string sqlMapping = @"
                        INSERT INTO tblCookAssignmentMapping 
                        (IdCookAssignment, IdOrderMaster, IdOrderDetails, Quantity, IdStatus, CreatedOn)
                        VALUES
                        (@IdCookAssignment, @IdOrderMaster, @IdOrderDetails, @Quantity, @IdStatus, @CreatedOn);";

                    foreach (var source in model.SourceOrders)
                    {
                        var mappingParams = new DynamicParameters();
                        mappingParams.Add("@IdCookAssignment", newId, DbType.Int32);
                        mappingParams.Add("@IdOrderMaster", source.IdOrderMaster, DbType.Int32);
                        mappingParams.Add("@IdOrderDetails", source.IdOrderDetails, DbType.Int32);
                        mappingParams.Add("@Quantity", source.Quantity, DbType.Int32);
                        mappingParams.Add("@IdStatus", model.IdStatus, DbType.Int32);
                        mappingParams.Add("@CreatedOn", DateTime.Now, DbType.DateTime);

                        await conn.ExecuteAsync(sqlMapping, mappingParams, transaction: tran);
                    }
                }

                tran.Commit();
                return newId;
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }

        public async Task<List<int>> UpdateMergedKitchenStatusDAL(UpdateKitchenStatusTO model)
        {
            using var conn = new SqlConnection(connq);
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();
            try
            {
                var affectedOrders = new List<int>();

                // Update Master
                var parameter = new DynamicParameters();
                parameter.Add("@IdCookAssignment", model.IdCookAssignment, DbType.Int32);
                parameter.Add("@IdStatus", model.IdStatus, DbType.Int32);
                parameter.Add("@Remarks", model.Remarks, DbType.String);
                parameter.Add("@UpdatedBy", model.UpdatedBy, DbType.Int32);
                parameter.Add("@UpdatedOn", DateTime.Now, DbType.DateTime);

                string sqlMaster = @"UPDATE tblCookAssignment SET 
                               IdStatus = @IdStatus, 
                               Remarks = @Remarks, 
                               UpdatedBy = @UpdatedBy, 
                               UpdatedOn = @UpdatedOn 
                               WHERE IdCookAssignment = @IdCookAssignment AND IsMerged = 1";

                int res = await conn.ExecuteAsync(sqlMaster, parameter, transaction: tran);

                if (res > 0)
                {
                    // Fetch affected orders before updating them
                    string sqlGetOrders = "SELECT DISTINCT IdOrderMaster FROM tblCookAssignmentMapping WHERE IdCookAssignment = @IdCookAssignment";
                    affectedOrders = (await conn.QueryAsync<int>(sqlGetOrders, parameter, transaction: tran)).ToList();

                    // Cascade to mapping table
                    string sqlMapping = @"UPDATE tblCookAssignmentMapping SET 
                                          IdStatus = @IdStatus, 
                                          UpdatedOn = @UpdatedOn 
                                          WHERE IdCookAssignment = @IdCookAssignment";
                    await conn.ExecuteAsync(sqlMapping, parameter, transaction: tran);
                }

                tran.Commit();
                return affectedOrders;
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }
    }
}
