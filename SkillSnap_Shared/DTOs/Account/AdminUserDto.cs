namespace SkillSnap_Shared.DTOs.Account
{
    /// <summary>
    /// Represents a user record displayed in the Admin Dashboard.
    /// Includes identity details, roles, and last login timestamp.
    /// </summary>
    public class AdminUserDto
    {
        public string Id { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Optional: the user's most recent login timestamp.
        /// Null if the user has never logged in.
        /// </summary>
        public DateTime? LastLogin { get; set; }

        /// <summary>
        /// List of role names assigned to the user (e.g., Admin, User).
        /// </summary>
        public List<string> Roles { get; set; } = new();
    }
}
