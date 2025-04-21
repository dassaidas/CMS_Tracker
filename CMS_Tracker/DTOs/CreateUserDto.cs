namespace CMS_Tracker.DTOs
{
    public class CreateUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } // e.g., Consultant, Manager
    }
}
