using SkillSnap.Shared.DTOs;

namespace SkillSnap_Client.Services
{
    public class UserContext
    {
        public PortfolioUserDto? CurrentPortfolioUser { get; private set; }

        public event Action? OnChange;

        public void SetPortfolioUser(PortfolioUserDto? user)
        {
            CurrentPortfolioUser = user;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();


    }
}