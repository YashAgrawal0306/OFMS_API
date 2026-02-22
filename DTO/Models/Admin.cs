namespace OFMS_API.Models
{
    public class Admin
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public string AdminType { get; set; } = null!;  

        public string? ProfileImage { get; set; }

        public string? Status { get; set; } 

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

}
