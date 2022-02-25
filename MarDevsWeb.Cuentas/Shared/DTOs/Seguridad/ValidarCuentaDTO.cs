using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Shared.DTOs
{
    public class ValidarCuentaDTO
    {
        [Required]
        public int UsuarioID { get; set; }
        [Required]
        public string Token { get; set; }


    }
}
