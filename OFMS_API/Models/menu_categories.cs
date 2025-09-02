using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;

namespace OFMS_API.Models
{
    public class menu_categories

    {
        [Key]
        public int Id { get; set; }
        public string? name { get; set; }
        public string? catImage { get; set; }
        public string? cat_description {  get; set; }
        public double? minprice { get; set; } 
        public double? maxprice { get; set; } 
        public int? totalitem { get; set; } = 0;
    }

}
