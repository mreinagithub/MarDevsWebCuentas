using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Shared.DTOs
{
    public class UserRegistro
    {                
        [Display(Name = "Email (se utiliza en caso de requierir recuperar la clave)")]
        [Required(ErrorMessage = "Debe indicar un correo electrónico")]
        [EmailAddress(ErrorMessage ="El formato ingresado es inválido")]
        public string Email { get; set; }
        [Display(Name = "Nombre (su nombre de pila)")]
        [MinLength(2, ErrorMessage = "Su nombre no puede tener menos de 2 caractéres.")]
        [Required(ErrorMessage = "Debe indicar un nombre de pila")]
        public string Nombre { get; set; } 
        [Display(Name ="Contraseña")]
        [Required(ErrorMessage = "Debe ingresar la contraseña")]
        public string PasswordNuevo { get; set; }
        [Display(Name = "Repetir Contraseña")]
        [Required(ErrorMessage = "Debe ingresar la repetición de la contraseña")]
        public string PasswordNuevoRepetido { get; set; }

    }
}
