using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace arna.HRMS.ClientServices.Auth
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedLocalStorage _protectedLocalStorage;
        private readonly HttpClient _httpClient; 
        private const string TokenKey = "auth_token"; 

        public CustomAuthStateProvider(
            ProtectedLocalStorage protectedLocalStorage,
            HttpClient httpClient)
        {
            _protectedLocalStorage = protectedLocalStorage;
            _httpClient = httpClient;
        }

        public async Task SetTokenAsync(string token)
        {
            await _protectedLocalStorage.SetAsync(TokenKey, token);
            ApplyToken(token);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task ClearTokenAsync()
        {
            await _protectedLocalStorage.DeleteAsync(TokenKey);
            _httpClient.DefaultRequestHeaders.Authorization = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var storedToken = await _protectedLocalStorage.GetAsync<string>("auth_token");

            if (!storedToken.Success || string.IsNullOrWhiteSpace(storedToken.Value))
            {
                return new AuthenticationState(
                    new ClaimsPrincipal(new ClaimsIdentity())
                );
            }

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(storedToken.Value);

            if (jwt.ValidTo < DateTime.UtcNow)
            {
                await ClearTokenAsync();

                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            ApplyToken(storedToken.Value);

            var identity = new ClaimsIdentity(jwt.Claims, "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        private void ApplyToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
