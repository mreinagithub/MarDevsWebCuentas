using MarDevsWeb.Cuentas.Client.Helpers;
using MarDevsWeb.Cuentas.Client.Repositorios;
using MarDevsWeb.Cuentas.Shared.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Client.Auth
{
    public class ProveedorAuthenticacionJWT : AuthenticationStateProvider, ILoginService
    {

        public static readonly string TOKENKEY = "jwt_token";
        public static readonly string EXPIRATIONTOKENKEY = "expiracion_token";
        public static readonly string REFRESH_TOKEN = "refresh_token";
        public static readonly string BROWSER_IDENTIFIER = "browser_id";
        public static readonly string ULTIMO_USUARIO_LOGUEADO = "ultimo_usr_logueado";
        private readonly IJSRuntime js;
        private readonly HttpClient httpClient;
        private readonly IRepositorio repositorio;

        private AuthenticationState Anonimo =>
            new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));


        public ProveedorAuthenticacionJWT(IJSRuntime js,
            HttpClient httpClient,
            IRepositorio repositorio)
        {
            this.js = js;
            this.httpClient = httpClient;
            this.repositorio = repositorio;
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await js.GetFromLocalStorage(TOKENKEY);
            if (string.IsNullOrEmpty(token))
            {
                return Anonimo;
            }
            var timeExpirationString = await js.GetFromLocalStorage(EXPIRATIONTOKENKEY);
            if (!DateTime.TryParse(timeExpirationString, out DateTime tiempoExpiracion))
            {
                return Anonimo;
            }
            if (DebeRenovarToken(tiempoExpiracion))
            {
                token = await RefreshToken(token);
                if (string.IsNullOrEmpty(token))
                {
                    return Anonimo;
                }
            }
            return ConstruirAuthenticationState(token);
        }

        private async Task<string> RefreshToken(string token)
        {            
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var claims = ParseClaimsFromJwt(token);
            if (claims == null || claims.Count() == 0 || !claims.Any(c => c.Type == "unique_name"))
                return null;
            var userId = Convert.ToInt32(claims.FirstOrDefault(c => c.Type == "unique_name").Value);

            var refreshToken = await js.GetFromLocalStorage(REFRESH_TOKEN);
            var browserToken = await js.GetFromLocalStorage(BROWSER_IDENTIFIER);

            if (string.IsNullOrWhiteSpace(browserToken))
                return null;

            var userRefreshDT = new UserRefreshToken
            {
                UsuarioID = userId,
                Token = token,
                RefreshToken = refreshToken,
                BrowserToken = Guid.Parse(browserToken)

            };

            Console.WriteLine("Renovando token...");

            var nuevoTokenResponse = await repositorio.Post<UserRefreshToken,UserToken>("api/cuenta/RefreshToken",userRefreshDT);

            if (nuevoTokenResponse.Error)
                return "";            
            
            var nuevoToken = nuevoTokenResponse.Response;

            await js.SetInLocalStorage(TOKENKEY, nuevoToken.Token);
            await js.SetInLocalStorage(EXPIRATIONTOKENKEY, nuevoToken.Expiration.ToString());
            await js.SetInLocalStorage(REFRESH_TOKEN, nuevoToken.RefreshToken);
            await js.SetInLocalStorage(BROWSER_IDENTIFIER, nuevoToken.BrowserToken.ToString());

            return nuevoToken.Token;            
        }
        private bool DebeRenovarToken(DateTime tiempoExpiracion)
        {            
            return tiempoExpiracion.Subtract(DateTime.UtcNow) < TimeSpan.FromMinutes(5);
        }      
        private AuthenticationState ConstruirAuthenticationState(string token)
        {
            httpClient.DefaultRequestHeaders.Authorization =
               new AuthenticationHeaderValue("bearer", token);            

            return new AuthenticationState(
                new ClaimsPrincipal(
                    new ClaimsIdentity(ParseClaimsFromJwt(token),"jwt")));
        }
        //Obtener una lista de claims desde el token.
        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles);

            if(roles != null)
            {
                if(roles.ToString().Trim().StartsWith("["))
                {
                    var parseRoles = JsonSerializer.Deserialize<string[]>(roles.ToString());
                    foreach(var parsedRole in parseRoles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                    }
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));
                }

                keyValuePairs.Remove(ClaimTypes.Role);
            }

            claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())));

            return claims;
        }        
        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch(base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;                    
            }
            return Convert.FromBase64String(base64);
        }
        private async Task Limpiar()
        {
            await js.RemoveFromLocalStorage(TOKENKEY);
            await js.RemoveFromLocalStorage(EXPIRATIONTOKENKEY);
            await js.RemoveFromLocalStorage(REFRESH_TOKEN);
            await js.RemoveFromLocalStorage(BROWSER_IDENTIFIER);
            httpClient.DefaultRequestHeaders.Authorization = null;

        }


        public async Task Login(UserToken userToken)
        {
            await js.SetInLocalStorage(TOKENKEY, userToken.Token);
            await js.SetInLocalStorage(EXPIRATIONTOKENKEY, userToken.Expiration.ToString());
            await js.SetInLocalStorage(ULTIMO_USUARIO_LOGUEADO, userToken.Email);
            await js.SetInLocalStorage(REFRESH_TOKEN, userToken.RefreshToken);
            await js.SetInLocalStorage(BROWSER_IDENTIFIER, userToken.BrowserToken.ToString());

            var authState = ConstruirAuthenticationState(userToken.Token);
            NotifyAuthenticationStateChanged(Task.FromResult(authState));

            
        }
        public async Task Logout()
        {
            var token = await js.GetFromLocalStorage(TOKENKEY);

            int userId = 0;
            var claims = ParseClaimsFromJwt(token);
            if (claims != null || claims.Count() > 0 || claims.Any(c => c.Type == "unique_name"))
                userId = Convert.ToInt32(claims.FirstOrDefault(c => c.Type == "unique_name").Value);            

            var refreshToken = await js.GetFromLocalStorage(REFRESH_TOKEN);
            var browserToken = await js.GetFromLocalStorage(BROWSER_IDENTIFIER);

            var userRefreshDT = new UserRefreshToken
            {
                UsuarioID = userId,
                Token = token,
                RefreshToken = refreshToken,
                BrowserToken = Guid.Parse(browserToken)

            };
            await repositorio.Post("api/cuenta/revocar-token", userRefreshDT);

            await Limpiar();
            NotifyAuthenticationStateChanged(Task.FromResult(Anonimo));
        }
        public async Task VerificarYRenovarToken(HttpRequestMessage request)
        {
                        
            //Console.WriteLine("Verificando token...");
            var token = await js.GetFromLocalStorage(TOKENKEY);
            var timeExpirationString = await js.GetFromLocalStorage(EXPIRATIONTOKENKEY);
            if (string.IsNullOrEmpty(token))
            {
                NotifyAuthenticationStateChanged(Task.FromResult(Anonimo));
            }
            else if (!DateTime.TryParse(timeExpirationString, out DateTime tiempoExpiracion))
            {
                NotifyAuthenticationStateChanged(Task.FromResult(Anonimo));
            }
            else
            {
                if (DebeRenovarToken(tiempoExpiracion))
                {
                    var nuevoToken = await RefreshToken(token);
                    if (string.IsNullOrEmpty(nuevoToken))
                        NotifyAuthenticationStateChanged(Task.FromResult(Anonimo));
                    else
                    {
                        var authState = ConstruirAuthenticationState(nuevoToken);                        
                        NotifyAuthenticationStateChanged(Task.FromResult(authState));  
                        //Actualizamos el request vigente
                        if(request != null)
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", nuevoToken);
                        }
                        
                    }
                }
            }            
        }
        public async Task<string> ObtenerUltimoUsuarioLogueado()
        {
            return await js.GetFromLocalStorage(ULTIMO_USUARIO_LOGUEADO);
        }
        public async Task<string> ObtenerNombrePilaUsuario()
        {
            var token = await js.GetFromLocalStorage(TOKENKEY);
            var claims = ParseClaimsFromJwt(token);
            if (claims == null || claims.Count() == 0 || !claims.Any(c => c.Type == "NombrePila"))
                return "Anónimo";
            else
                return claims.FirstOrDefault(c => c.Type == "NombrePila").Value;
        }        
    }
}
