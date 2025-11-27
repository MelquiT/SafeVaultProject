//using Blazored.LocalStorage;
using Blazored.SessionStorage;
using System.Net.Http.Headers;

namespace BlazorClient.Services
{
    // Clase para inyectar el token en cada petición HTTP
    public class TokenHandler : DelegatingHandler
    {
        //private readonly ILocalStorageService _localStorage;
        private readonly ISessionStorageService _sessionStorage;

        //public TokenHandler(ILocalStorageService localStorage)
        public TokenHandler(ISessionStorageService sessionStorage)
        {
            //_localStorage = localStorage;
            _sessionStorage = sessionStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 1. Leer el token guardado en el navegador
            var token = await _sessionStorage.GetItemAsStringAsync("authToken", cancellationToken);

            if (!string.IsNullOrEmpty(token))
            {
                // 2. Adjuntar el token al encabezado de la petición
                // Formato: Authorization: Bearer <tu_token>
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
