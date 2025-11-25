using System.Net.Http.Json;
using SkillSnap.Shared.DTOs; // adjust to your DTO namespace

namespace SkillSnap_Client.Services
{
    public class UserService
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly HttpClient _http;

        /// <summary>
        /// 
        /// </summary>
        private readonly UserContext _userContext;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="http"></param>
        /// <param name="userContext"></param>
        public UserService(HttpClient http, UserContext userContext)
        {
            _http = http;
            _userContext = userContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<PortfolioUserDto?> GetCurrentUserAsync()
        {
            try
            {
                var response = await _http.GetAsync("api/portfolioUser/me");

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"GetCurrentUserAsync returned {response.StatusCode}. Body: {body}");
                    return null;
                }

                try
                {
                    return await response.Content.ReadFromJsonAsync<PortfolioUserDto>();
                }
                catch (System.Text.Json.JsonException jex)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error deserializing current user: {jex.Message}. Body: {body}");
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error loading current user: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<PortfolioUserDto?> LoadCurrentUserAsync()
        {
            try
            {
                var response = await _http.GetAsync("api/portfolioUser/me");

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"LoadCurrentUserAsync returned {response.StatusCode}. Body: {body}");
                    _userContext.SetPortfolioUser(null);
                    return null;
                }

                try
                {
                    var result = await response.Content.ReadFromJsonAsync<PortfolioUserDto>();
                    _userContext.SetPortfolioUser(result);
                    return result;
                }
                catch (System.Text.Json.JsonException jex)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error deserializing current user: {jex.Message}. Body: {body}");
                    _userContext.SetPortfolioUser(null);
                    return null;
                }
            }
            catch
            {
                _userContext.SetPortfolioUser(null);
                return null;
            }
        }
    }
}