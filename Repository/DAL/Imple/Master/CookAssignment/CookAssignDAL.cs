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
    }
}
