using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Shared.DTOs
{
    public class UserRecuperarClave
    {

        [Required(ErrorMessage = "Debe indicar un correo electrónico")]
        [EmailAddress(ErrorMessage = "El formato ingresado es inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Debe ingresar la contraseña temporal recibida.")]
        public string PasswordTemporal { get; set; }
        [Required(ErrorMessage = "Debe ingresar la contraseña nueva")]
        public string PasswordNuevo { get; set; }
        [Required(ErrorMessage = "Debe ingresar la repetición de la contraseña nueva")]
        public string PasswordNuevoRepetido { get; set; }
    }
}
