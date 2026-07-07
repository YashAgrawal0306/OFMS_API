using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Models.Master.AddressMaster
{
    public class tblAddressTO
    {
        public int IdAddress { get; set; }

        public string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }

        public string? Landmark { get; set; }
        public string? Area { get; set; }
        public string? Locality { get; set; }

        public int IdCity { get; set; }
        public int IdState { get; set; }
        public int IdCountry { get; set; }

        public string Pincode { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class tblAddressResponseTO
    {
        public int IdAddressMapping { get; set; }
        public int IdAddress { get; set; }
        public string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? Landmark { get; set; }
        public string? Area { get; set; }
        public string? Locality { get; set; }
        public int IdCity { get; set; }
        public string CityName { get; set; }
        public string CitryCode { get; set; }
        public int IdState { get; set; }
        public string StateName { get; set; }
        public string StateCode { get; set; }
        public int IdCountry { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public string Pincode { get; set; } 
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; } 
        public bool IsActive { get; set; } 
        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; } 
        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
