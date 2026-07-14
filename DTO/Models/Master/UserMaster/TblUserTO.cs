using Microsoft.AspNetCore.Http;

namespace DTO.Models.Master.UserMaster
{
    public class TblUserTO
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? Password { get; set; }
        public string? Phone_Number { get; set; }
        public DateTime? Date_Of_Birth { get; set; }
        public IFormFile? ProfileImage { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? Created_At { get; set; }
        public DateTime? Updated_At { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
    }

    /// <summary>
    /// Combined DTO for creating a user + optional address in a single form submission.
    /// EntityType is determined server-side from RoleId (CUSTOMER if RoleId=6, else EMPLOYEE).
    /// Lat/Long are handled server-side; not exposed to frontend.
    /// </summary>
    public class TblUserWithAddressTO
    {
        // ─── User Fields ───────────────────────────────────────────────
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? Password { get; set; }
        public string? Phone_Number { get; set; }
        public DateTime? Date_Of_Birth { get; set; }
        public IFormFile? ProfileImage { get; set; }
        public bool? IsActive { get; set; }
        public int? RoleId { get; set; }

        // ─── Address Fields (all optional) ────────────────────────────
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? Landmark { get; set; }
        public string? Area { get; set; }
        public string? Locality { get; set; }
        public int? IdCity { get; set; }
        public int? IdState { get; set; }
        public int? IdCountry { get; set; }
        public string? Pincode { get; set; }
        public int? IdAddressType { get; set; }   // 1=Home, 2=Office, 3=Other
        public bool IsDefaultAddress { get; set; } = true;
    }

    public class TblUserResponseTO
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? Password { get; set; }
        public string? Phone_Number { get; set; }
        public DateTime? Date_Of_Birth { get; set; }
        public string? Profile_Image { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? Created_At { get; set; }
        public DateTime? Updated_At { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
    }
}
