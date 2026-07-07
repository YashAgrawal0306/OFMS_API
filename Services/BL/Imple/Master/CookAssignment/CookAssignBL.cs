using DTO.Models.CommonModel;
using DTO.Models.Master.OrderMaster;
using Repository.DAL.Interface.Master.CookAssignment;
using Services.BL.Interface.Master.CookAssignment;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.BL.Imple.Master.CookAssignment
{
    public class CookAssignBL : ICookAssignBL
    {
        private readonly ICookAssignDAL _dal;

        public CookAssignBL(ICookAssignDAL dal)
        {
            _dal = dal;
        }

        public async Task<int> CreateCookAssignmentBL(List<CreateCookAssignmentTO> models)
        {
            return await _dal.CreateCookAssignmentDAL(models);
        }

        public async Task<int> UpdateKitchenStatusBL(UpdateKitchenStatusTO model)
        {
            return await _dal.UpdateKitchenStatusDAL(model);
        }

        public async Task<List<CookAssignmentResponseTO>> GetCookAssignmentListBL(FilterModelTO filterModelTO)
        {
            return await _dal.GetCookAssignmentListDAL(filterModelTO);
        }
    }
}
