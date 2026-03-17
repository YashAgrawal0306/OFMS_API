using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Models.Master.ItemMaster
{
    public class TblItemMasterTO
    {
        public int IdItemMaster { get; set; }
        public int IdCategory { get; set; }
        public int IdSubCategory { get; set; }
        public int IdGroupMaster { get; set; }
        public string? ItemName { get; set; }
        public string? ItemDescription { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class ViewTblSubCategoryWithItemsTO
    {
        // SubCategory Info
        public int IdSubCategory { get; set; }
        public int IdGroupMaster { get; set; }
        public int? ParentId { get; set; }
        public string? SubCategoryName { get; set; }
        public string? SubCategoryDescription { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public string? UpdatedByName { get; set; }

        // Item Summary
        public int? TotalItemCount { get; set; }
        public int? TotalActiveItem { get; set; }
        public int? TotalInActiveItem { get; set; }

        // Item List
        public List<ItemList>? ItemList { get; set; }
    }
    public class ItemList
    {
        public int IdItemMaster { get; set; }
        public string? ItemName { get; set; }
        public string? ItemDescription { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
        public bool IsActive { get; set; }
    }
}
