using Dapper;
using DTO.Models.CommonModel;
using DTO.Models.Master.UserMaster;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OFMS_API.DAL.Interface;
using OFMS_API.Helper.Common;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;

namespace OFMS_API.DAL.Imple
{
    public class userDAL : IuserDAL
    {
        private string connq;
        private readonly IConfiguration _config;
        public userDAL(IConfiguration configuration)
        {
            connq = configuration.GetConnectionString("DefaultConnection") ?? "";
            _config = configuration;
        }

        public async Task<OutPutClass<TblUserResponseTO>> GetAllCustomer(FilterModelTO filter)
        {
            using var conn = new SqlConnection(connq);

            int pageNo = filter.PageNo ?? 0;
            int pageSize = filter.PageSize ?? 0;
            int offset = (pageNo - 1) * pageSize;

            string sql = @"SELECT tbluser.*,tblroles.roleName,tblroles.roleId FROM tbluser
                         LEFT JOIN tblUserRoleMapping tblUserRoleMapping ON
                         tblUser.userid = tblUserRoleMapping.UserId 
                         LEFT JOIN tblroles tblroles ON tblroles.roleId = tblUserRoleMapping.RoleId  WHERE 1=1";

            if (filter.RoleId.HasValue && filter.RoleId > 0)
            {
                sql += " AND tblroles.RoleId = @RoleId";
            }
            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                sql += " AND (UserName LIKE @Search OR UserEmail LIKE @Search OR Phone_Number LIKE @Search)";
            }

            string sortColumn = string.IsNullOrEmpty(filter.SortColumn) ? "UserId" : filter.SortColumn;
            string sortOrder = string.IsNullOrEmpty(filter.SortOrder) ? "ASC" : filter.SortOrder.ToUpper();
            sql += $" ORDER BY {sortColumn} {sortOrder}";

            sql += " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var result = await conn.QueryAsync<TblUserResponseTO>(sql, new
            {
                Search = $"%{filter.SearchText}%",
                Offset = offset,
                PageSize = pageSize,
                Roleid = filter.RoleId
            });
            var list = result.ToList();

            string countSql = @"SELECT COUNT(*)
                              FROM tbluser
                              LEFT JOIN tblUserRoleMapping ON tbluser.UserId = tblUserRoleMapping.UserId
                              LEFT JOIN tblroles ON tblroles.roleId = tblUserRoleMapping.RoleId
                              WHERE 1=1";

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                countSql += " AND (UserName LIKE @Search OR UserEmail LIKE @Search OR Phone_Number LIKE @Search)";
            }
            if (filter.RoleId.HasValue && filter.RoleId > 0)
            {
                countSql += " AND tblroles.RoleId = @RoleId";
            }

            int total = await conn.ExecuteScalarAsync<int>(countSql, new
            {
                Search = $"%{filter.SearchText}%",
                RoleId = filter.RoleId
            });
            return new OutPutClass<TblUserResponseTO>
            {
                List = list.ToList(),
                TotalCount = total
            };
        }

        public async Task<TblUserResponseTO> GetUserByIdUser(int idUser)
        {
            using var conn = new SqlConnection(connq);

            string sql = @"SELECT tblUser.*,tblroles.roleName,tblroles.roleId FROM tblUser tblUser LEFT JOIN tblUserRoleMapping tblUserRoleMapping ON 
                        tblUser.userid = tblUserRoleMapping.UserId LEFT JOIN tblroles tblroles ON tblroles.roleId = 
                        tblUserRoleMapping.RoleId  WHERE tbluser.UserId = " + idUser + "";

            var data = await conn.QueryFirstOrDefaultAsync<TblUserResponseTO>(
                sql,
                new { UserId = idUser }
            );

            return data;
        }

        public async Task<OutPutClass<TblUserTO>> GetAllMemberList(FilterModelTO filter)
        {
            try
            {
                using var conn = new SqlConnection(connq);

                int pageNo = filter.PageNo ?? 1;
                int pageSize = filter.PageSize ?? 10;
                int offset = (pageNo - 1) * pageSize;

                var sqlquery = new StringBuilder();
                sqlquery.Append("SELECT  UserId, UserName, UserEmail, Phone_Number, Profile_Image, Date_Of_Birth,Created_At, Updated_At, IsActive FROM tbluser");

                if (!string.IsNullOrEmpty(filter.SearchText))
                {
                    sqlquery.Append(" AND (UserName LIKE @Search OR UserEmail LIKE @Search OR Phone_Number LIKE @Search)");
                }



                string sortColumn = string.IsNullOrEmpty(filter.SortColumn) ? "UserId" : filter.SortColumn;
                string sortOrder = string.IsNullOrEmpty(filter.SortOrder) ? "ASC" : filter.SortOrder.ToUpper();
                sqlquery.Append($" ORDER BY {sortColumn} {sortOrder}");

                sqlquery.Append(" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");

                var result = await conn.QueryAsync<TblUserTO>(sqlquery.ToString(), new
                {
                    Search = $"%{filter.SearchText}%",
                    Offset = offset,
                    PageSize = pageSize
                });

                var list = result.Select(x => new TblUserTO
                {
                    UserId = x.UserId,
                    UserName = x.UserName,
                    UserEmail = x.UserEmail,
                    Phone_Number = x.Phone_Number,
                    ProfileImage = x.ProfileImage,
                    Date_Of_Birth = x.Date_Of_Birth,
                    Created_At = x.Created_At,
                    Updated_At = x.Updated_At,
                    IsActive = x.IsActive
                }).ToList();

                var countQuery = new StringBuilder("SELECT COUNT(*) FROM tbluser");

                if (!string.IsNullOrEmpty(filter.SearchText))
                {
                    countQuery.Append(" AND (UserName LIKE @Search OR UserEmail LIKE @Search OR Phone_Number LIKE @Search)");
                }



                int total = await conn.ExecuteScalarAsync<int>(countQuery.ToString(), new
                {
                    Search = $"%{filter.SearchText}%",

                });

                return new OutPutClass<TblUserTO>
                {
                    List = list,
                    TotalCount = total
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> AddUserRoleMapping(int userid, int roleId, SqlConnection conn, SqlTransaction tran)
        {
            var parameter = new DynamicParameters();
            parameter.Add("@UserId", userid, DbType.Int32);
            parameter.Add("@RoleId", roleId, DbType.Int32);
            string sql = @" INSERT INTO tblUserRoleMapping (UserId, RoleId, CreatedOn, IsActive, CreatedBy)
                            VALUES (@UserId, @RoleId, GETDATE(), 1, @UserId) SELECT CAST(SCOPE_IDENTITY() AS INT);";
            int mappingid = await conn.QuerySingleOrDefaultAsync<int>(sql, parameter, transaction: tran);
            if (mappingid > 0)
            {
                return mappingid;
            }
            else
            {
                return 0;
            }
        }
        public async Task<int> AddNewCustomerDAL(TblUserTO customer, SqlConnection conn, SqlTransaction tran)
        {
            try
            {
                var pass = customer.Password ?? "";
                SHA256 sHA256 = SHA256.Create();
                byte[] bytes = Encoding.UTF8.GetBytes(pass);
                byte[] hasbyte = sHA256.ComputeHash(bytes);
                string path = "";
                if (customer.ProfileImage != null)
                {
                    path = Helper.Common.Utility.StoreFileInLocalFolder(customer.ProfileImage);
                }
                StringBuilder builder = new StringBuilder();

                foreach (var b in hasbyte)
                {
                    builder.Append(b.ToString("x2"));
                }
                string hasPass = builder.ToString();

                var parameter = new DynamicParameters();
                parameter.Add("@UserName", customer.UserName, DbType.String);
                parameter.Add("@UserEmail", customer.UserEmail, DbType.String);
                parameter.Add("@Password", hasPass, DbType.String);
                parameter.Add("@Phone_number", customer.Phone_Number, DbType.String);
                parameter.Add("@Date_of_birth", customer.Date_Of_Birth, DbType.Date);
                parameter.Add("@Profile_image", path, DbType.String);
                parameter.Add("@IsActive", customer.IsActive, DbType.String);
                parameter.Add("@created_at", DateTime.Now, DbType.DateTime);
                parameter.Add("@updated_at", DateTime.Now, DbType.DateTime);

                string sql = @" INSERT INTO tbluser (UserName,UserEmail, Password, Phone_number, Date_of_birth, Profile_image, IsActive, created_at, updated_at) 
            VALUES (@UserName,@UserEmail, @Password, @Phone_number, @Date_of_birth, @Profile_image, @IsActive, @created_at, @updated_at) 
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
                int userid = await conn.QuerySingleAsync<int>(sql, parameter, transaction: tran);
                if (userid > 0)
                {
                    return userid;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> EditUserTO(TblUserTO customer)
        {
            string path = "";

            if (customer.ProfileImage != null)
            {
                path = Helper.Common.Utility.StoreFileInLocalFolder(customer.ProfileImage);
            }

            using var conn = new SqlConnection(connq);

            var parameter = new DynamicParameters();
            parameter.Add("@UserId", customer.UserId);
            parameter.Add("@UserName", customer.UserName);
            parameter.Add("@UserEmail", customer.UserEmail);
            parameter.Add("@Phone_Number", customer.Phone_Number);
            parameter.Add("@Date_Of_Birth", customer.Date_Of_Birth);
            parameter.Add("@Profile_Image", path);
            parameter.Add("@IsActive", customer.IsActive);
            parameter.Add("@Updated_At", DateTime.Now);

            string sql = @"
    UPDATE tblUser
    SET
        UserName = @UserName,
        UserEmail = @UserEmail,
        Phone_Number = @Phone_Number,
        Date_Of_Birth = @Date_Of_Birth,
        Profile_Image = @Profile_Image,
        IsActive = @IsActive,
        Updated_At = @Updated_At
    WHERE UserId = @UserId";

            int data = await conn.ExecuteAsync(sql, parameter);
            return data;
        }

        public async Task<string> LoginDAL(TblUserLogin tbluserlogin)
        {
            var pass = tbluserlogin.Password ?? "";
            using var conn = new SqlConnection(connq);
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(pass);
            byte[] hashBytes = sha256.ComputeHash(bytes);
            string hashedPassword = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            string sql = "SELECT * FROM tbluser WHERE useremail = @Email AND Password = @Password";
            var user = await conn.QueryFirstOrDefaultAsync<TblUserTO>(
                sql,
                new { Email = tbluserlogin.Email, Password = hashedPassword }
            );

            if (user != null)
            {
                string token = await GenerateToken(user);
                return token;
            }
            else
            {
                return "";
            }
        }

        public async Task<List<TblRoleTO>> GetAllRoles()
        {
            using var conn = new SqlConnection(connq);

            string sql = "Select roleId,roleName,roleDescription from tblroles";
            var getRole = await conn.QueryAsync<TblRoleTO>(sql);
            return getRole.Select(x => new TblRoleTO
            {
                RoleId = x.RoleId,
                RoleName = x.RoleName,
                RoleDescription = x.RoleDescription,
            }).ToList();
        }

        private async Task<string> GenerateToken(TblUserTO tbluserlogin)
        {
            var jwtkey = _config["Jwt:Key"] ?? "";
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtkey));
            var credential = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim("userId",tbluserlogin.UserId.ToString()),
                new Claim("roleId",tbluserlogin.RoleId.ToString())
            };
            var gettoken = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(100),
                signingCredentials: credential
                );
            string token = await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(gettoken));
            return token;
        }

        //public Task<int> EditUserRoleTO(int UserId, int IdRole,SqlConnection conn ,SqlTransaction tran)
        //{
            
        //}
    }
}
