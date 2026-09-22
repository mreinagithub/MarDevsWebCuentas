using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarDevsWeb.Cuentas.Server.Models.Seguridad
{
    [Table("usuario_refresh_token")]
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

        [Column("usuario_id")]
        public int UsuarioID { get; set; }
        [Column("browser_token")]
        public Guid BrowserToken { get; set; }
        [Column("refresh_token")]
        public string RefreshToken { get; set; } = null;
        [Column("refresh_token_expire_date")]
        public DateTime? RefreshTokenExpireDate { get; set; } = null;
    }
}
