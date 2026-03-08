using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Models.Master.ItemMaster
{
    public class TblCategoryMasterTO
    {
        public int IdCategory { get; set; }
        public int IdGroupMaster { get; set; }
        public int? ParentId { get; set; }
        public string? CategoryName { get; set; }
        public string? CatDescription { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedByName {  get; set; }
        public string? UpdatedByName {  get; set; }
        public int? UpdatedBy { get; set; }
        public string? GroupName { get; set; }
    }

    public class ViewTblCategoryMasterTO
    {
        public int IdCategory { get; set; }
        public int IdGroupMaster { get; set; }
        public int? ParentId { get; set; }
        public string? CategoryName { get; set; }
        public string? CatDescription { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedByName { get; set; }
        public int? UpdatedBy { get; set; }
        public List<SubCategoryList>? SubCategoryList{ get; set; }
    }

    public class CategoryWithSubCategoryListTO
    {
        public int IdCategory { get; set; }
        public int IdGroupMaster { get; set; }
        public int? ParentId { get; set; }
        public string? CategoryName { get; set; }
        public string? CatDescription { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public int? totalSubCount { get; set; }
        public string? GroupName { get; set; }
        public List<SubCategoryList>? SubCategoryList { get; set; }
    }
    public class SubCategoryList
    {
        public int IdSubCategory { get; set; }
        public string? SubCategoryName { get; set; }
        public string? SubCategoryDescription { get; set; }
    }
}
