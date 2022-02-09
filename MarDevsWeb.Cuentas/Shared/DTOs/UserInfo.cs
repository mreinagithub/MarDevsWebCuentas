using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Shared.DTOs
{
    public class UserInfo
    {
        [Required(ErrorMessage = "Debe ingresar un usuario válido")]               
        public string Logon { get; set; }
        [Required(ErrorMessage = "Debe ingresar la contraseña")]
        public string Password { get; set; }
    }
    
}
