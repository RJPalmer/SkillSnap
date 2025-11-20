namespace SkillSnap_Shared.DTOs.Account
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
    }
}