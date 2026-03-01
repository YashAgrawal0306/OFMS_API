using DTO.Models.CommonModel;
using DTO.Models.Master.ImageMaster;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.BL.Interface.Master.ImageMaster
{
    public interface IImageMasterBL
    {
        Task<ResultMessage> AddItemMasterImage(TblImageMasterRequestTO model);
        Task<List<tblImageMasterResponseTO>> GetListOfItemMasterImage(FilterModelTO filterModelTO);
        Task<tblImageMasterResponseTO> GetItemMasterImageById(int id);
        Task<ResultMessage> UpdateItemMasterImage(TblImageMasterRequestTO model);
        Task<ResultMessage> DeleteItemMasterImage(int id);
    }
}
