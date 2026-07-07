using DTO.Models.CommonModel;
using DTO.Models.Master.UserMaster;
using Microsoft.Data.SqlClient;

namespace OFMS_API.DAL.Interface
{
    public interface IuserDAL
    {
        Task<OutPutClass<TblUserResponseTO>> GetAllCustomer(FilterModelTO filter);
        Task<TblUserResponseTO> GetUserByIdUser(int idUser);
        Task<OutPutClass<TblUserTO>> GetAllMemberList(FilterModelTO filter);
        Task<int> AddNewCustomerDAL(TblUserTO customerDTO,SqlConnection conn,SqlTransaction tran);
        Task<int> EditUserTO(TblUserTO customerDTO);
        //Task<int> EditUserRoleTO(int UserId,int IdRole);
        Task<int> AddUserRoleMapping(int userid,int roleid,SqlConnection conn ,SqlTransaction tran);
        Task<string> LoginDAL(TblUserLogin loginCustomer);
        Task<List<TblRoleTO>> GetAllRoles();
    }
}
