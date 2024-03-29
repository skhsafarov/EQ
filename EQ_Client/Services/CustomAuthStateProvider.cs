using Blazored.LocalStorage;
using Blazored.SessionStorage;
using EQ_Shared;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace EQ_Client.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly HttpClient _httpClient;

        public CustomAuthStateProvider(ILocalStorageService localStorageService, HttpClient httpClient)
        {
            _localStorageService = localStorageService;
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var accessToken = await _localStorageService.GetItemAsync<string>("AccessToken");
            ClaimsIdentity identity;
            _httpClient.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrEmpty(accessToken))
            {
                identity = new ClaimsIdentity(ParseClaimsFromJwt(accessToken), "jwt");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
            else
            {
                identity = new ClaimsIdentity();
            }
            var user = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(user);

            return await Task.FromResult<AuthenticationState>(state);
        }

        public async Task MarkUserAsAuthenticated(string accessToken)
        {
            ClaimsIdentity identity;
            _httpClient.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    identity = new ClaimsIdentity(ParseClaimsFromJwt(accessToken), "jwt");
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
                catch (Exception e)
                {
                    await _localStorageService.RemoveItemAsync("AccessToken");
                    identity = new ClaimsIdentity();
                    throw new Exception(e.Message);
                }

                var user = new ClaimsPrincipal(identity);
                var state = new AuthenticationState(user);

                NotifyAuthenticationStateChanged(Task.FromResult(state));
            }
        }

        public Task MarkUserAsLoggedOut()
        {
            _localStorageService.RemoveItemAsync("accessToken");

            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));

            return Task.CompletedTask;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            var claims = keyValuePairs!.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()!));

            return claims;
        }
    }
}