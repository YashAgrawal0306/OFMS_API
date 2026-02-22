using DTO.Models.CommonModel;
using OFMS_API.BL.Interface;
using OFMS_API.DAL.Interface;
using OFMS_API.Models;

namespace OFMS_API.BL.Imple
{
    public class UserBL(IuserDAL dal) : IuserBL
    {
        public async Task<OutPutClass<TblUserTO>> GetAllCust(FilterModelTO filter)
        {
            return await dal.GetAllCustomer(filter);
        }

        public async Task<int> AddNewCustomerBL(TblUserTO customerDTO)
        {
            return await dal.AddNewCustomerDAL(customerDTO);
        }

        public async Task<string> LoginBL(TblUserLogin loginCustomer)
        {
            return await dal.LoginDAL(loginCustomer);
        }

        public async Task<OutPutClass<TblUserTO>> GetAllMemberList(FilterModelTO filter)
        {
            try
            {
                return await dal.GetAllMemberList(filter);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
