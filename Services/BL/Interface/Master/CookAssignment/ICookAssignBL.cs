using DTO.Models.CommonModel;
using DTO.Models.Master.OrderMaster;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.BL.Interface.Master.CookAssignment
{
    public interface ICookAssignBL
    {
        Task<int> CreateCookAssignmentBL(List<CreateCookAssignmentTO> models);
        Task<int> UpdateKitchenStatusBL(UpdateKitchenStatusTO model);
        Task<List<CookAssignmentResponseTO>> GetCookAssignmentListBL(FilterModelTO filterModelTO);
    }
}
