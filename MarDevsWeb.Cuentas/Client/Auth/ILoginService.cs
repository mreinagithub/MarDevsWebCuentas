using MarDevsWeb.Cuentas.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Client.Auth
{
    public interface ILoginService
    {

        Task Login(UserToken userToken);
        Task Logout();
        Task VerificarYRenovarToken(HttpRequestMessage request);

        Task<string> ObtenerUltimoUsuarioLogueado();
        Task<string> ObtenerNombrePilaUsuario();
    }
}
