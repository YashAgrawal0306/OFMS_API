using DTO.Models.CommonModel;
using OFMS_API.Models;

namespace OFMS_API.DAL.Interface
{
    public interface IuserDAL
    {
        Task<OutPutClass<TblUserTO>> GetAllCustomer(FilterModelTO filter);
        Task<OutPutClass<TblUserTO>> GetAllMemberList(FilterModelTO filter);
        Task<int> AddNewCustomerDAL(TblUserTO customerDTO);
        Task<string> LoginDAL(TblUserLogin loginCustomer);
    }
}
