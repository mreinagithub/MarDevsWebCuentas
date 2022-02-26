using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Shared.DTOs
{
    public class UserRefreshToken
    {

        public int UsuarioID { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public Guid BrowserToken { get; set; }
    }
}
