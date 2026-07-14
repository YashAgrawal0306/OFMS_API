using DTO.Models.CommonModel;
using DTO.Models.Master.UserMaster;

namespace OFMS_API.BL.Interface
{
    public interface IuserBL
    {
        Task<OutPutClass<TblUserResponseTO>> GetAllCust(FilterModelTO filter);
        Task<TblUserResponseTO> GetUserByIdUser(int idUser);
        Task<int> AddNewCustomerBL(TblUserTO customerDTO);
        Task<int> AddNewUserWithAddressBL(TblUserWithAddressTO model);
        Task<int> EditUserTO(TblUserTO customerDTO);
        Task<string> LoginBL(TblUserLogin tbluserlogin);
        Task<OutPutClass<TblUserTO>> GetAllMemberList(FilterModelTO filter);
        Task<List<TblRoleTO>> GetAllRoles();
    }
}
