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
        public async Task<(List<TblUser>, int count)> GetAllCust(int PageNo,int totalItem)
        {
            return await dal.GetAllCustomer(PageNo,totalItem);
        }

        public async Task<int> AddNewCustomerBL(TblUser customerDTO)
        {
            return await dal.AddNewCustomerDAL(customerDTO);
        }

        public async Task<string> LoginBL(TblUserLogin loginCustomer)
        {
           return await dal.LoginDAL(loginCustomer);
        }
    }
}
