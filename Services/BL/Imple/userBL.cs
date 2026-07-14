using DTO.Models.CommonModel;
using DTO.Models.Master.UserMaster;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OFMS_API.BL.Interface;
using OFMS_API.DAL.Interface;

namespace OFMS_API.BL.Imple
{
    public class UserBL : IuserBL
    {
        private string connq;
        private readonly IConfiguration _config;
        private readonly IuserDAL dal;
        public UserBL(IuserDAL _dal, IConfiguration config)
        {
            _config = config;
            dal = _dal;
            connq = config.GetConnectionString("DefaultConnection") ?? "";
        }
        public async Task<OutPutClass<TblUserResponseTO>> GetAllCust(FilterModelTO filter)
        {
            return await dal.GetAllCustomer(filter);
        }

        public async Task<TblUserResponseTO> GetUserByIdUser(int idUser)
        {
            return await dal.GetUserByIdUser(idUser);
        }
        public async Task<int> AddNewCustomerBL(TblUserTO customerDTO)
        {

            using var conn = new SqlConnection(connq);
            await conn.OpenAsync();
            SqlTransaction tran = conn.BeginTransaction();
            int userid = await dal.AddNewCustomerDAL(customerDTO,conn,tran);
            if(userid > 0)
            {
               int mappingid =await dal.AddUserRoleMapping(userid,customerDTO.RoleId ?? 0,conn,tran);
                if(mappingid > 0)
                {
                    tran.Commit();
                    return mappingid;
                }
                else
                {
                    tran.Rollback();
                    return 0;
                }
            }
            return 0;
        }

        /// <summary>
        /// Creates a new user + optional address in a single atomic transaction.
        /// Steps: 1) Insert user, 2) Insert role mapping, 3) Insert address + mapping (if address provided).
        /// EntityType is auto-derived from RoleId. Lat/Long are null.
        /// </summary>
        public async Task<int> AddNewUserWithAddressBL(TblUserWithAddressTO model)
        {
            using var conn = new SqlConnection(connq);
            await conn.OpenAsync();
            SqlTransaction tran = conn.BeginTransaction();

            try
            {
                // Map to TblUserTO for the existing DAL method
                var userTO = new TblUserTO
                {
                    UserName     = model.UserName,
                    UserEmail    = model.UserEmail,
                    Password     = model.Password,
                    Phone_Number = model.Phone_Number,
                    Date_Of_Birth = model.Date_Of_Birth,
                    ProfileImage = model.ProfileImage,
                    IsActive     = model.IsActive,
                    RoleId       = model.RoleId
                };

                // Step 1: Insert user
                int userId = await dal.AddNewCustomerDAL(userTO, conn, tran);
                if (userId <= 0) { tran.Rollback(); return 0; }

                // Step 2: Role mapping
                int mappingId = await dal.AddUserRoleMapping(userId, model.RoleId ?? 0, conn, tran);
                if (mappingId <= 0) { tran.Rollback(); return 0; }

                // Step 3: Address (only if AddressLine1 is provided)
                bool hasAddress = !string.IsNullOrWhiteSpace(model.AddressLine1);
                if (hasAddress)
                {
                    int addrMappingId = await dal.AddAddressWithMappingDAL(userId, model, conn, tran);
                    if (addrMappingId <= 0) { tran.Rollback(); return 0; }
                }

                tran.Commit();
                return userId;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public async Task<int> EditUserTO(TblUserTO customerDTO)
        {
            int userid = await dal.EditUserTO(customerDTO);
            //if(userid > 0)
            //{
            //    dal.EditUserRoileTO(customerDTO.UserId,customerDTO.RoleId);
            //}
            return 1;
        }

        public async Task<string> LoginBL(TblUserLogin loginCustomer)
        {
            return await dal.LoginDAL(loginCustomer);
        }
        public async Task<List<TblRoleTO>> GetAllRoles()
        {
            return await dal.GetAllRoles();
        }


        public async Task<OutPutClass<TblUserTO>> GetAllMemberList(FilterModelTO filter)
        {
            try
            {
                return await dal.GetAllMemberList(filter);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
