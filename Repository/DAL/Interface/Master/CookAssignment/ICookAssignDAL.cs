using DTO.Models.CommonModel;
using DTO.Models.Master.OrderMaster;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.DAL.Interface.Master.CookAssignment
{
    public interface ICookAssignDAL
    {
        Task<int> CreateCookAssignmentDAL(List<CreateCookAssignmentTO> models);
        Task<int> UpdateKitchenStatusDAL(UpdateKitchenStatusTO model);
        Task<List<CookAssignmentResponseTO>> GetCookAssignmentListDAL(FilterModelTO filterModelTO);

        // Merged Item-wise Cook Assignment Methods
        Task<List<MergeableItemResponseTO>> GetMergeableCookItemsDAL();
        Task<int> AssignMergedCookItemDAL(MergedCookAssignmentRequestTO model);
        Task<List<int>> UpdateMergedKitchenStatusDAL(UpdateKitchenStatusTO model);
    }
}
