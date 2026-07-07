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
