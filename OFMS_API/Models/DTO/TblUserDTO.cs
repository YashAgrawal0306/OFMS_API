namespace OFMS_API.Models.DTO
{
    public class TblUserDTO
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? Password { get; set; }
        public string? Phone_Number { get; set; }
        public DateTime? Date_Of_Birth { get; set; }
        public string? ProfileImage { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? Created_At { get; set; }
        public DateTime? Updated_At { get; set; }
        public int RoleId { get; set; }
    }
}
