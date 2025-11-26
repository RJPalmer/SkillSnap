namespace SkillSnap.Shared.DTOs.Account
{
    public class LinkPortfolioUserResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public int PortfolioUserId { get; set; }
        public DateTime Expires { get; set; }
    }
}