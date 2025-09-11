using Microsoft.Data.SqlClient;
using OFMS_API.Models;

namespace OFMS_API.DAL.Interface
{
    public interface IuserDAL
    {
        Task<(List<TblUser>, int count)> GetAllCustomer(int PageNo,int totalItem);
        Task<int> AddNewCustomerDAL(TblUser customerDTO);
        Task<string> LoginDAL(TblUserLogin loginCustomer);
    }
}
