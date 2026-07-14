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
                sqlquery.Append(@"
                    SELECT u.UserId, u.UserName, u.UserEmail, u.Phone_Number, u.Profile_Image, 
                           u.Date_Of_Birth, u.Created_At, u.Updated_At, u.IsActive,
                           r.RoleId, r.RoleName 
                    FROM tbluser u
                    LEFT JOIN tblUserRoleMapping m ON u.userid = m.UserId
                    LEFT JOIN tblroles r ON m.RoleId = r.RoleId
                    WHERE 1=1");

                if (!string.IsNullOrEmpty(filter.SearchText))
                {
                    sqlquery.Append(" AND (u.UserName LIKE @Search OR u.UserEmail LIKE @Search OR u.Phone_Number LIKE @Search OR r.RoleName LIKE @Search)");
                }

                string sortColumn = string.IsNullOrEmpty(filter.SortColumn) ? "u.UserId" : filter.SortColumn;
                // Avoid ambiguous column names if sorting by UserId etc
                if (!sortColumn.Contains(".")) {
                    sortColumn = "u." + sortColumn;
                }
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
                    IsActive = x.IsActive,
                    RoleId = x.RoleId,
                    RoleName = x.RoleName
                }).ToList();

                var countQuery = new StringBuilder(@"
                    SELECT COUNT(*) 
                    FROM tbluser u
                    LEFT JOIN tblUserRoleMapping m ON u.userid = m.UserId
                    LEFT JOIN tblroles r ON m.RoleId = r.RoleId
                    WHERE 1=1");

                if (!string.IsNullOrEmpty(filter.SearchText))
                {
                    countQuery.Append(" AND (u.UserName LIKE @Search OR u.UserEmail LIKE @Search OR u.Phone_Number LIKE @Search OR r.RoleName LIKE @Search)");
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

        /// <summary>
        /// Inserts a new address into tblAddress and creates the mapping in tblAddressMapping.
        /// EntityType = 'CUSTOMER' if RoleId == 6, otherwise 'EMPLOYEE'.
        /// Latitude and Longitude are left NULL (handled server-side, not exposed to frontend).
        /// </summary>
        public async Task<int> AddAddressWithMappingDAL(int userId, TblUserWithAddressTO model, SqlConnection conn, SqlTransaction tran)
        {
            // 1. Insert into tblAddress
            var addrParams = new DynamicParameters();
            addrParams.Add("@AddressLine1", model.AddressLine1, DbType.String);
            addrParams.Add("@AddressLine2", model.AddressLine2, DbType.String);
            addrParams.Add("@Landmark", model.Landmark, DbType.String);
            addrParams.Add("@Area", model.Area, DbType.String);
            addrParams.Add("@Locality", model.Locality, DbType.String);
            addrParams.Add("@IdCity", model.IdCity, DbType.Int32);
            addrParams.Add("@IdState", model.IdState, DbType.Int32);
            addrParams.Add("@IdCountry", model.IdCountry, DbType.Int32);
            addrParams.Add("@Pincode", model.Pincode, DbType.String);
            addrParams.Add("@Latitude", (object?)null, DbType.Decimal);
            addrParams.Add("@Longitude", (object?)null, DbType.Decimal);
            addrParams.Add("@IsActive", true, DbType.Boolean);
            addrParams.Add("@CreatedOn", DateTime.Now, DbType.DateTime);
            addrParams.Add("@CreatedBy", userId, DbType.Int32);

            string addrSql = @"INSERT INTO tblAddress
                (AddressLine1, AddressLine2, Landmark, Area, Locality, IdCity, IdState, IdCountry,
                 Pincode, Latitude, Longitude, IsActive, CreatedOn, CreatedBy)
                VALUES
                (@AddressLine1, @AddressLine2, @Landmark, @Area, @Locality, @IdCity, @IdState, @IdCountry,
                 @Pincode, @Latitude, @Longitude, @IsActive, @CreatedOn, @CreatedBy);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int idAddress = await conn.QuerySingleOrDefaultAsync<int>(addrSql, addrParams, transaction: tran);
            if (idAddress <= 0) return 0;

            // 2. Insert into tblAddressMapping
            // EntityType: CUSTOMER for RoleId=6, otherwise EMPLOYEE
            string entityType = (model.RoleId == 6) ? "CUSTOMER" : "EMPLOYEE";

            var mapParams = new DynamicParameters();
            mapParams.Add("@EntityType", entityType, DbType.String);
            mapParams.Add("@EntityId", userId, DbType.Int32);
            mapParams.Add("@IdAddress", idAddress, DbType.Int32);
            mapParams.Add("@IdAddressType", model.IdAddressType ?? 1, DbType.Int32);  // default: Home
            mapParams.Add("@IsDefault", model.IsDefaultAddress, DbType.Boolean);
            mapParams.Add("@IsActive", true, DbType.Boolean);
            mapParams.Add("@CreatedOn", DateTime.Now, DbType.DateTime);
            mapParams.Add("@CreatedBy", userId, DbType.Int32);

            string mapSql = @"INSERT INTO tblAddressMapping
                (EntityType, EntityId, IdAddress, IdAddressType, IsDefault, IsActive, CreatedOn, CreatedBy)
                VALUES
                (@EntityType, @EntityId, @IdAddress, @IdAddressType, @IsDefault, @IsActive, @CreatedOn, @CreatedBy);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int mappingId = await conn.QuerySingleOrDefaultAsync<int>(mapSql, mapParams, transaction: tran);
            return mappingId;
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
        " + (!string.IsNullOrEmpty(path) ? "Profile_Image = @Profile_Image," : "") + @"
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

            // Authenticate user AND fetch their actual role from tblUserRoleMapping
            string sql = @"
                SELECT u.*, ISNULL(m.RoleId, 0) AS RoleId
                FROM tbluser u
                LEFT JOIN tblUserRoleMapping m ON u.userid = m.UserId
                WHERE u.useremail = @Email AND u.Password = @Password";
            var user = await conn.QueryFirstOrDefaultAsync<TblUserTO>(
                sql,
                new { Email = tbluserlogin.Email, Password = hashedPassword }
            );

            if (user != null)
            {
                // Use the actual DB role, NOT whatever frontend sent
                string token = await GenerateToken(user, user.RoleId ?? 0);
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

        private async Task<string> GenerateToken(TblUserTO tbluserlogin, int roleId)
        {
            var jwtkey = _config["Jwt:Key"] ?? "";
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtkey));
            var credential = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim("userId",tbluserlogin.UserId.ToString()),
                new Claim("roleId",roleId.ToString())
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
