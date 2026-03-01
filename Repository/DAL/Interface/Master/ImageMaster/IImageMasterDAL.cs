using DTO.Models.CommonModel;
using DTO.Models.Master.ImageMaster;
using DTO.Models.Master.ImageMaster.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DAL.Interface.Master.ImageMaster
{
    public interface IImageMasterDAL
    {
        Task<int> AddItemMasterImage(TblImageMasterRequestTO model, string imageUrl);
        Task<List<tblImageMasterResponseTO>> GetListOfItemMasterImage(FilterModelTO filterModelTO);
        Task<tblImageMasterResponseTO> GetItemMasterImageById(int id);
        Task<List<AttachmentListTO>> GetItemMasterImageByReferenceId(int IdReference,int ImageTypeId);
        Task<int> UpdateItemMasterImage(TblImageMasterRequestTO model, string imageUrl);
        Task<int> DeleteItemMasterImage(int id);
    }
}
