using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace BlazorClient.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        //private readonly ILocalStorageService _localStorage;
        private readonly ISessionStorageService _sessionStorage;
        private readonly HttpClient _http;

        // Estado por defecto (Usuario No Autenticado)
        private readonly AuthenticationState _anonymous =
            new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        //public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient http)
        public CustomAuthStateProvider(ISessionStorageService sessionStorage, HttpClient http)
        {
            //_localStorage = localStorage;
            _sessionStorage = sessionStorage;
            _http = http;
        }

        // Método principal: Determina el estado de autenticación al cargar la app.
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // 1. Intentar obtener el token del LocalStorage
            //var token = await _localStorage.GetItemAsStringAsync("authToken");
            var token = await _sessionStorage.GetItemAsStringAsync("authToken");

            if (string.IsNullOrEmpty(token))
            {
                return _anonymous;
            }

            // 2. Si hay token, configurar el HttpClient para futuras peticiones
            // Esto es importante si el HttpClient no usa el TokenHandler por alguna razón,
            // aunque ya lo configuramos en Program.cs.
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // 3. Crear ClaimsPrincipal a partir del token (descodificación)
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(
                ParseClaimsFromJwt(token), "jwtAuth")));
        }

        // Método llamado desde Login.razor para notificar un login exitoso
        public async Task MarkUserAsAuthenticated(string token)
        {
            // 1. Almacenar el token en el LocalStorage (ya lo hace Login.razor, pero lo repetimos por si acaso)
            //await _localStorage.SetItemAsync("authToken", token);
            await _sessionStorage.SetItemAsync("authToken", token);

            // 2. Crear el nuevo estado
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(
                ParseClaimsFromJwt(token), "jwtAuth"));

            // 3. Notificar a la aplicación (a los componentes) que el estado cambió
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }

        // Método para cerrar sesión
        public async Task MarkUserAsLoggedOut()
        {
            await _sessionStorage.RemoveItemAsync("authToken");
            _http.DefaultRequestHeaders.Authorization = null; // Limpiar encabezado

            // Notificar que el estado volvió a ser anónimo
            var anonymousState = Task.FromResult(_anonymous);
            NotifyAuthenticationStateChanged(anonymousState);
        }

        // Método utilitario para descodificar el token JWT
        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            // El token JWT tiene 3 partes: Header.Payload.Signature
            var payload = jwt.Split('.')[1];

            // Rellenar el payload con "=" si es necesario (Base64 Padding)
            payload = payload.Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            // Deserializar el JSON del payload para obtener los claims
            var jsonBytes = Convert.FromBase64String(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs != null)
            {
                foreach (var kvp in keyValuePairs)
                {
                    // Manejar claims de rol (que pueden venir como un array si hay múltiples)
                    if (kvp.Key == ClaimTypes.Role && kvp.Value is JsonElement element && element.ValueKind == JsonValueKind.Array)
                    {
                        claims.AddRange(element.EnumerateArray().Select(role => new Claim(ClaimTypes.Role, role.ToString())));
                    }
                    else
                    {
                        claims.Add(new Claim(kvp.Key, kvp.Value.ToString()!));
                    }
                }
            }
            return claims;
        }
    }
}
