namespace OFMS_API.Models
{
    public class TblUserTO
    {
        public int UserId { get; set; }             
        public string? UserName { get; set; }       
        public string? UserEmail { get; set; }      
        public string? Password { get; set; }        
        public string? PhoneNumber { get; set; }    
        public DateTime? DateOfBirth { get; set; }  
        public string? ProfileImage { get; set; }   
        public bool? IsActive { get; set; }         
        public DateTime? CreatedAt { get; set; }   
        public DateTime? UpdatedAt { get; set; }
        public int RoleId { get; set; }
    }
}
