using System;

namespace MarDevsWeb.Cuentas.Server.Models.Seguridad
{
    public class UsuarioRefreshToken
    {
        public UsuarioRefreshToken()
        {

        }
        public UsuarioRefreshToken(int usuarioId, Guid browserToken)
        {
            UsuarioID = usuarioId;
            BrowserToken = browserToken;
        }

        public int UsuarioID { get; set; }
        public Guid BrowserToken { get; set; }
        public string RefreshToken { get; set; } = null;
        public DateTime? RefreshTokenExpireDate { get; set; } = null;
    }
}
