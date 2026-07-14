using DTO.Models.Master.CustomerHome;
using System.Threading.Tasks;

namespace Repository.DAL.Interface.Master.CustomerHome
{
    public interface ICustomerHomeDAL
    {
        Task<CustomerHomeDataTO> GetCustomerHomeDataDAL();
    }
}
