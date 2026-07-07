using Dapper;
using DTO.Models.CommonModel;
using DTO.Models.Master.OrderMaster;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Repository.DAL.Interface.Master.DeliveryAssignment;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.DAL.Imple.Master.DeliveryAssignment
{
    public class DeliveryAssignmentDAL : IDeliveryAssignmentDAL
    {
        private readonly string connq;

        public DeliveryAssignmentDAL(IConfiguration configuration)
        {
            connq = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task<int> AssignDeliveryBoy(CreateDeliveryAssignmentTO model)
        {
            using var conn = new SqlConnection(connq);
            var parameter = new DynamicParameters();
            parameter.Add("@IdOrderMaster", model.IdOrderMaster, DbType.Int32);
            parameter.Add("@DeliveryBoyUserId", model.DeliveryBoyUserId, DbType.Int32);
            parameter.Add("@AssignedOn", DateTime.Now, DbType.DateTime);
            parameter.Add("@EstimatedDeliveryTime", model.EstimatedDeliveryTime, DbType.Int32);
            parameter.Add("@DeliveryRemarks", model.DeliveryRemarks, DbType.String);
            parameter.Add("@CreatedBy", model.CreatedBy, DbType.Int32);

            string sql = @"
                DECLARE @IdStatus INT = (SELECT TOP 1 IdStatus FROM dimStatus WHERE StatusName = 'Assigned');
                
                INSERT INTO tblDeliveryAssignment 
                (IdOrderMaster, DeliveryBoyUserId, IdStatus, AssignedOn, EstimatedDeliveryTime, DeliveryRemarks, IsActive, CreatedOn, CreatedBy) 
                VALUES 
                (@IdOrderMaster, @DeliveryBoyUserId, @IdStatus, @AssignedOn, @EstimatedDeliveryTime, @DeliveryRemarks, 1, GETDATE(), @CreatedBy);
                
                SELECT CAST(SCOPE_IDENTITY() as int);";

            return await conn.ExecuteScalarAsync<int>(sql, parameter);
        }

        public async Task<DeliveryAssignmentResponseTO> GetDeliveryAssignmentById(int idDeliveryAssignment)
        {
            using var conn = new SqlConnection(connq);
            string sql = @"
                SELECT 
                    da.IdDeliveryAssignment,
                    da.IdOrderMaster,
                    o.OrderNo,
                    da.DeliveryBoyUserId,
                    u.UserName AS DeliveryBoyName,
                    da.IdStatus,
                    s.StatusName,
                    da.AssignedOn,
                    da.AcceptedOn,
                    da.PickedUpOn,
                    da.DeliveredOn,
                    da.EstimatedDeliveryTime,
                    da.ActualDeliveryTime,
                    da.DeliveryRemarks
                FROM tblDeliveryAssignment da
                LEFT JOIN tblOrderMaster o ON da.IdOrderMaster = o.IdOrderMaster
                LEFT JOIN tblUser u ON da.DeliveryBoyUserId = u.UserId
                LEFT JOIN dimStatus s ON da.IdStatus = s.IdStatus
                WHERE da.IdDeliveryAssignment = @IdDeliveryAssignment";

            return await conn.QueryFirstOrDefaultAsync<DeliveryAssignmentResponseTO>(sql, new { IdDeliveryAssignment = idDeliveryAssignment });
        }

        public async Task<OutPutClass<DeliveryAssignmentResponseTO>> GetAllDeliveryAssignments(FilterModelTO filter)
        {
            using var conn = new SqlConnection(connq);

            int pageNo = filter.PageNo ?? 1;
            int pageSize = filter.PageSize ?? 10;
            int offset = (pageNo - 1) * pageSize;

            string sortColumn = string.IsNullOrWhiteSpace(filter.SortColumn) ? "da.IdDeliveryAssignment" : filter.SortColumn;
            string sortOrder = string.IsNullOrWhiteSpace(filter.SortOrder) ? "DESC" : filter.SortOrder;

            var parameters = new DynamicParameters();
            parameters.Add("@SearchText", string.IsNullOrWhiteSpace(filter.SearchText) ? null : $"%{filter.SearchText}%", DbType.String);
            parameters.Add("@IdStatus", filter.IdStatus > 0 ? filter.IdStatus : (int?)null, DbType.Int32);
            parameters.Add("@Offset", offset, DbType.Int32);
            parameters.Add("@PageSize", pageSize, DbType.Int32);

            string baseWhere = @"
                WHERE (@SearchText IS NULL OR o.OrderNo LIKE @SearchText OR u.UserName LIKE @SearchText)
                  AND (@IdStatus IS NULL OR da.IdStatus = @IdStatus)";

            string countQuery = $@"
                SELECT COUNT(1) 
                FROM tblDeliveryAssignment da
                LEFT JOIN tblOrderMaster o ON da.IdOrderMaster = o.IdOrderMaster
                LEFT JOIN tblUser u ON da.DeliveryBoyUserId = u.UserId
                {baseWhere}";

            int totalRecords = await conn.ExecuteScalarAsync<int>(countQuery, parameters);

            string dataQuery = $@"
                SELECT 
                    da.IdDeliveryAssignment,
                    da.IdOrderMaster,
                    o.OrderNo,
                    da.DeliveryBoyUserId,
                    u.UserName AS DeliveryBoyName,
                    da.IdStatus,
                    s.StatusName,
                    da.AssignedOn,
                    da.AcceptedOn,
                    da.PickedUpOn,
                    da.DeliveredOn,
                    da.EstimatedDeliveryTime,
                    da.ActualDeliveryTime,
                    da.DeliveryRemarks
                FROM tblDeliveryAssignment da
                LEFT JOIN tblOrderMaster o ON da.IdOrderMaster = o.IdOrderMaster
                LEFT JOIN tblUser u ON da.DeliveryBoyUserId = u.UserId
                LEFT JOIN dimStatus s ON da.IdStatus = s.IdStatus
                {baseWhere}
                ORDER BY {sortColumn} {sortOrder}
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var list = await conn.QueryAsync<DeliveryAssignmentResponseTO>(dataQuery, parameters);

            return new OutPutClass<DeliveryAssignmentResponseTO>
            {
                List = list.ToList(),
                TotalCount = totalRecords
            };
        }

        public async Task<int> AcceptDelivery(int idDeliveryAssignment, int updatedBy)
        {
            using var conn = new SqlConnection(connq);
            string sql = @"
                DECLARE @IdStatus INT = (SELECT TOP 1 IdStatus FROM dimStatus WHERE StatusName = 'Accepted');
                UPDATE tblDeliveryAssignment 
                SET IdStatus = @IdStatus, 
                    AcceptedOn = GETDATE(), 
                    UpdatedBy = @UpdatedBy, 
                    UpdatedOn = GETDATE() 
                WHERE IdDeliveryAssignment = @IdDeliveryAssignment";

            return await conn.ExecuteAsync(sql, new { IdDeliveryAssignment = idDeliveryAssignment, UpdatedBy = updatedBy });
        }

        public async Task<int> PickUpOrder(int idDeliveryAssignment, int updatedBy)
        {
            using var conn = new SqlConnection(connq);
            string sql = @"
                DECLARE @IdStatus INT = (SELECT TOP 1 IdStatus FROM dimStatus WHERE StatusName = 'Picked Up');
                UPDATE tblDeliveryAssignment 
                SET IdStatus = @IdStatus, 
                    PickedUpOn = GETDATE(), 
                    UpdatedBy = @UpdatedBy, 
                    UpdatedOn = GETDATE() 
                WHERE IdDeliveryAssignment = @IdDeliveryAssignment";

            return await conn.ExecuteAsync(sql, new { IdDeliveryAssignment = idDeliveryAssignment, UpdatedBy = updatedBy });
        }

        public async Task<int> MarkDelivered(int idDeliveryAssignment, int updatedBy)
        {
            using var conn = new SqlConnection(connq);
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();
            try
            {
                string sqlDelivery = @"
                    DECLARE @IdStatus INT = (SELECT TOP 1 IdStatus FROM dimStatus WHERE StatusName = 'Delivered');
                    UPDATE tblDeliveryAssignment 
                    SET IdStatus = @IdStatus, 
                        DeliveredOn = GETDATE(), 
                        UpdatedBy = @UpdatedBy, 
                        UpdatedOn = GETDATE() 
                    WHERE IdDeliveryAssignment = @IdDeliveryAssignment;
                    SELECT IdOrderMaster FROM tblDeliveryAssignment WHERE IdDeliveryAssignment = @IdDeliveryAssignment;";

                int idOrderMaster = await conn.ExecuteScalarAsync<int>(sqlDelivery, new { IdDeliveryAssignment = idDeliveryAssignment, UpdatedBy = updatedBy }, tran);

                if (idOrderMaster > 0)
                {
                    string sqlOrder = @"
                        DECLARE @OrderStatusId INT = (SELECT TOP 1 IdStatus FROM dimStatus WHERE StatusName = 'Completed');
                        UPDATE tblOrderMaster 
                        SET IdStatus = @OrderStatusId, 
                            UpdatedBy = @UpdatedBy, 
                            UpdatedOn = GETDATE() 
                        WHERE IdOrderMaster = @IdOrderMaster";
                        
                    await conn.ExecuteAsync(sqlOrder, new { IdOrderMaster = idOrderMaster, UpdatedBy = updatedBy }, tran);
                }

                tran.Commit();
                return 1;
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }

        public async Task<bool> CheckOrderExists(int idOrderMaster)
        {
            using var conn = new SqlConnection(connq);
            string sql = "SELECT COUNT(1) FROM tblOrderMaster WHERE IdOrderMaster = @IdOrderMaster";
            return await conn.ExecuteScalarAsync<int>(sql, new { IdOrderMaster = idOrderMaster }) > 0;
        }

        public async Task<bool> CheckDeliveryBoyExists(int deliveryBoyUserId)
        {
            using var conn = new SqlConnection(connq);
            string sql = "SELECT COUNT(1) FROM tblUser WHERE UserId = @UserId";
            return await conn.ExecuteScalarAsync<int>(sql, new { UserId = deliveryBoyUserId }) > 0;
        }

        public async Task<bool> CheckDuplicateAssignment(int idOrderMaster)
        {
            using var conn = new SqlConnection(connq);
            string sql = "SELECT COUNT(1) FROM tblDeliveryAssignment WHERE IdOrderMaster = @IdOrderMaster AND IsActive = 1";
            return await conn.ExecuteScalarAsync<int>(sql, new { IdOrderMaster = idOrderMaster }) > 0;
        }

        public async Task<bool> CheckIfDelivered(int idDeliveryAssignment)
        {
            using var conn = new SqlConnection(connq);
            string sql = @"
                SELECT COUNT(1) 
                FROM tblDeliveryAssignment 
                WHERE IdDeliveryAssignment = @IdDeliveryAssignment 
                AND IdStatus = (SELECT TOP 1 IdStatus FROM dimStatus WHERE StatusName = 'Delivered')";
            return await conn.ExecuteScalarAsync<int>(sql, new { IdDeliveryAssignment = idDeliveryAssignment }) > 0;
        }
    }
}
