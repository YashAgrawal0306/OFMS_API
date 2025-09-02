using Dapper;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
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

        public List<tbluser> GetAllCustomer()
        {
            using var conn = new SqlConnection(connq);
            string sql = "SELECT * FROM Customer";
            var list = conn.Query<tbluser>(sql).ToList();
            return list;
        }
        public async Task<int> AddNewCustomerDAL(tbluser customer)
        {
            SHA256 sHA256 = SHA256.Create();
                byte[] bytes = Encoding.UTF8.GetBytes(customer.Password);
                byte[] hasbyte = sHA256.ComputeHash(bytes);

                StringBuilder builder = new StringBuilder();

                foreach (var b in hasbyte)
                {
                    builder.Append(b.ToString("x2"));
                }
               string hasPass =  builder.ToString();
            
            using var conn = new SqlConnection(connq);
            var parameter = new DynamicParameters();
            parameter.Add("@UserName", customer.UserName, DbType.String);
            parameter.Add("@UserEmail", customer.UserEmail, DbType.String);
            parameter.Add("@Password", hasPass, DbType.String);
            parameter.Add("@Phone_number", customer.PhoneNumber, DbType.String);
            parameter.Add("@Date_of_birth", customer.DateOfBirth, DbType.Date);
            parameter.Add("@Profile_image", customer.ProfileImage, DbType.String);
            parameter.Add("@IsActive", customer.IsActive, DbType.String);
            parameter.Add("@created_at", DateTime.Now, DbType.DateTime);
            parameter.Add("@updated_at", DateTime.Now, DbType.DateTime);
            parameter.Add("@roleId",customer.RoleId, DbType.Int32);

            string sql = @" INSERT INTO tbluser (UserName,UserEmail, Password, Phone_number, Date_of_birth, Profile_image, IsActive, created_at, updated_at,roleId) 
            VALUES (@UserName,@UserEmail, @Password, @Phone_number, @Date_of_birth, @Profile_image, @IsActive, @created_at, @updated_at,@roleId)";
            await conn.ExecuteAsync(sql, parameter);
            return 1;
        }


        public async Task<string> LoginDAL(tbluserlogin tbluserlogin)
        {
            using var conn = new SqlConnection(connq);
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(tbluserlogin.Password);
            byte[] hashBytes = sha256.ComputeHash(bytes);
            string hashedPassword = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            string sql = "SELECT * FROM tbluser WHERE useremail = @Email AND Password = @Password AND roleId =@RoleId ";
            var user = await conn.QueryFirstOrDefaultAsync<tbluser>(
                sql,
                new { Email = tbluserlogin.Email, Password = hashedPassword , roleId = tbluserlogin.RoleId }
            );

            if (user != null)
            {
                string token = GenerateToken(user); 
                return token;
            }
            else
            {
                return null;
            }
        }

        private string GenerateToken(tbluser tbluserlogin)
        {
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credential = new SigningCredentials(securitykey,SecurityAlgorithms.HmacSha256);
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
            string token = new JwtSecurityTokenHandler().WriteToken(gettoken);
            return token;
        }
    }
}
