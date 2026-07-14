using DTO.Models.Master.CustomerHome;
using OFMS_API.BL.Interface.Master.CustomerHome;
using Repository.DAL.Interface.Master.CustomerHome;
using System.Threading.Tasks;

namespace OFMS_API.BL.Imple.Master.CustomerHome
{
    public class CustomerHomeBL : ICustomerHomeBL
    {
        private readonly ICustomerHomeDAL _dal;

        public CustomerHomeBL(ICustomerHomeDAL dal)
        {
            _dal = dal;
        }

        public async Task<CustomerHomeDataTO> GetCustomerHomeData()
        {
            return await _dal.GetCustomerHomeDataDAL();
        }
    }
}
