using DTO.Models.Master.CustomerHome;
using System.Threading.Tasks;

namespace OFMS_API.BL.Interface.Master.CustomerHome
{
    public interface ICustomerHomeBL
    {
        Task<CustomerHomeDataTO> GetCustomerHomeData();
    }
}
