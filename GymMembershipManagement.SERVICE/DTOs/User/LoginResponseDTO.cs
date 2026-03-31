namespace GymMembershipManagement.SERVICE.DTOs.User
{
    public class LoginResponseDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime RegistrationDate { get; set; }
        public List<string> Roles { get; set; } = new();
        public string Token { get; set; } = null!;
    }
}
