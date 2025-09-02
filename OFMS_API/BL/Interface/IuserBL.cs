using OFMS_API.Models;

namespace OFMS_API.BL.Interface
{
    public interface IuserBL
    {
        List<tbluser> GetAllCust();
        Task<int> AddNewCustomerBL(tbluser customerDTO);
        Task<string> LoginBL(tbluserlogin tbluserlogin);
    }
}
