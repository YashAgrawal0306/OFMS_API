using OFMS_API.BL.Interface;
using OFMS_API.DAL.Interface;
using OFMS_API.Models;

namespace OFMS_API.BL.Imple
{
    public class userBL : IuserBL
    {
        private readonly IuserDAL dal;
        public userBL(IuserDAL dAL)
        {
            dal = dAL;
        }
        public List<tbluser> GetAllCust()
        {
            return dal.GetAllCustomer();
        }

        public Task<int> AddNewCustomerBL(tbluser customerDTO)
        {
            return dal.AddNewCustomerDAL(customerDTO);
        }

        public Task<string> LoginBL(tbluserlogin loginCustomer)
        {
           return dal.LoginDAL(loginCustomer);
        }
    }
}
