namespace CMS_Tracker.DTOs
{
    public class AuthResponse
    {

        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
