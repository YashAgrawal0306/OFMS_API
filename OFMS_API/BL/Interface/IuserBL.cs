using OFMS_API.Models;

namespace OFMS_API.BL.Interface
{
    public interface IuserBL
    {
        Task<(List<TblUser>, int count)> GetAllCust(int PageNo,int totalItem);
        Task<int> AddNewCustomerBL(TblUser customerDTO);
        Task<string> LoginBL(TblUserLogin tbluserlogin);
    }
}
