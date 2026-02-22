using DTO.Models.CommonModel;
using OFMS_API.Models;

namespace OFMS_API.BL.Interface
{
    public interface IuserBL
    {
        Task<OutPutClass<TblUserTO>> GetAllCust(FilterModelTO filter);
        Task<int> AddNewCustomerBL(TblUserTO customerDTO);
        Task<string> LoginBL(TblUserLogin tbluserlogin);
        Task<OutPutClass<TblUserTO>> GetAllMemberList(FilterModelTO filter);
    }
}
