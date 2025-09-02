using Microsoft.Data.SqlClient;
using OFMS_API.Models;

namespace OFMS_API.DAL.Interface
{
    public interface IuserDAL
    {
        List<tbluser> GetAllCustomer();
        Task<int> AddNewCustomerDAL(tbluser customerDTO);
        Task<string> LoginDAL(tbluserlogin loginCustomer);
    }
}
