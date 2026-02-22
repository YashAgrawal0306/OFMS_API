using Dapper;
using DTO.Models.CommonModel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OFMS_API.DAL.Interface;
using OFMS_API.Models;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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

        public async Task<OutPutClass<TblUserTO>> GetAllCustomer(FilterModelTO filter)
        {
            using var conn = new SqlConnection(connq);

            int pageNo = filter.PageNo ?? 0;
            int pageSize = filter.PageSize ?? 0;
            int offset = (pageNo - 1) * pageSize;

            string sql = "SELECT * FROM tbluser WHERE 1=1";

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                sql += " AND (UserName LIKE @Search OR UserEmail LIKE @Search OR Phone_Number LIKE @Search)";
            }

            string sortColumn = string.IsNullOrEmpty(filter.SortColumn) ? "UserId" : filter.SortColumn;
            string sortOrder = string.IsNullOrEmpty(filter.SortOrder) ? "ASC" : filter.SortOrder.ToUpper();
            sql += $" ORDER BY {sortColumn} {sortOrder}";

            sql += " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var result = await conn.QueryAsync<TblUserTO>(sql, new
            {
                Search = $"%{filter.SearchText}%",
                Offset = offset,
                PageSize = pageSize
            });
            var list = result.Select(x => new TblUserTO
            {
                UserId = x.UserId != 0 ? Convert.ToInt32(x.UserId) : 0,
                UserName = x.UserName,
                UserEmail = x.UserEmail,
                Phone_Number = x.Phone_Number,
                ProfileImage = x.ProfileImage,
                Date_Of_Birth = x.Date_Of_Birth == null ? (DateTime?)null : Convert.ToDateTime(x.Date_Of_Birth),
                Created_At = x.Created_At == null ? (DateTime?)null : Convert.ToDateTime(x.Created_At),
                Updated_At = x.Updated_At == null ? (DateTime?)null : Convert.ToDateTime(x.Updated_At),
                IsActive = x.IsActive == null ? (bool?)null : Convert.ToBoolean(x.IsActive),
                RoleId = x.RoleId == 0 ? 0 : Convert.ToInt32(x.RoleId)
            }).ToList();

            string countSql = "SELECT COUNT(*) FROM tbluser WHERE 1=1";
            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                countSql += " AND (UserName LIKE @Search OR UserEmail LIKE @Search OR PhoneNumber LIKE @Search)";
            }
            int total = await conn.ExecuteScalarAsync<int>(countSql, new { Search = $"%{filter.SearchText}%" });

            return new OutPutClass<TblUserTO>
            {
                List = list.ToList(),
                TotalUser = total
            };
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
                sqlquery.Append("SELECT  UserId, UserName, UserEmail, Phone_Number, Profile_Image, Date_Of_Birth,Created_At, Updated_At, IsActive, RoleId FROM tbluser WHERE roleId != 6");

                if (!string.IsNullOrEmpty(filter.SearchText))
                {
                    sqlquery.Append(" AND (UserName LIKE @Search OR UserEmail LIKE @Search OR Phone_Number LIKE @Search)");
                }

                if (filter.RoleId != 0)
                {
                    sqlquery.Append(" AND roleId = @RoleId");
                }

                string sortColumn = string.IsNullOrEmpty(filter.SortColumn) ? "UserId" : filter.SortColumn;
                string sortOrder = string.IsNullOrEmpty(filter.SortOrder) ? "ASC" : filter.SortOrder.ToUpper();
                sqlquery.Append($" ORDER BY {sortColumn} {sortOrder}");

                sqlquery.Append(" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");

                var result = await conn.QueryAsync<TblUserTO>(sqlquery.ToString(), new
                {
                    Search = $"%{filter.SearchText}%",
                    RoleId = filter.RoleId,
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
                    RoleId = x.RoleId
                }).ToList();

                var countQuery = new StringBuilder("SELECT COUNT(*) FROM tbluser WHERE roleId != 6");

                if (!string.IsNullOrEmpty(filter.SearchText))
                {
                    countQuery.Append(" AND (UserName LIKE @Search OR UserEmail LIKE @Search OR Phone_Number LIKE @Search)");
                }

                if (filter.RoleId != 0)
                {
                    countQuery.Append(" AND roleId = @RoleId");
                }

                int total = await conn.ExecuteScalarAsync<int>(countQuery.ToString(), new
                {
                    Search = $"%{filter.SearchText}%",
                    RoleId = filter.RoleId
                });

                return new OutPutClass<TblUserTO>
                {
                    List = list,
                    TotalUser = total
                };
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<int> AddNewCustomerDAL(TblUserTO customer)
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

            using var conn = new SqlConnection(connq);
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
            parameter.Add("@roleId", customer.RoleId, DbType.Int32);

            string sql = @" INSERT INTO tbluser (UserName,UserEmail, Password, Phone_number, Date_of_birth, Profile_image, IsActive, created_at, updated_at,roleId) 
            VALUES (@UserName,@UserEmail, @Password, @Phone_number, @Date_of_birth, @Profile_image, @IsActive, @created_at, @updated_at,@roleId)";
            await conn.ExecuteAsync(sql, parameter);
            return 1;
        }


        public async Task<string> LoginDAL(TblUserLogin tbluserlogin)
        {
            var pass = tbluserlogin.Password ?? "";
            using var conn = new SqlConnection(connq);
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(pass);
            byte[] hashBytes = sha256.ComputeHash(bytes);
            string hashedPassword = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            string sql = "SELECT * FROM tbluser WHERE useremail = @Email AND Password = @Password AND roleId =@RoleId ";
            var user = await conn.QueryFirstOrDefaultAsync<TblUserTO>(
                sql,
                new { Email = tbluserlogin.Email, Password = hashedPassword, roleId = tbluserlogin.RoleId }
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


    }
}
