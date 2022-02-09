using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Shared.DTOs
{
    public class UserModificarClave
    {

        public int UsuarioID { get; set; }

        [Required(ErrorMessage = "Debe ingresar la contraseña actual")]
        public string PasswordActual { get; set; }
        [Required(ErrorMessage = "Debe ingresar la contraseña nueva")]
        public string PasswordNuevo { get; set; }
        [Required(ErrorMessage = "Debe ingresar la repetición de la contraseña nueva")]
        public string PasswordNuevoRepetido { get; set; }

    }
}
