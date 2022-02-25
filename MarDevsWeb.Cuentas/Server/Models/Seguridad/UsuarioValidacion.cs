using System;

namespace MarDevsWeb.Cuentas.Server.Models.Seguridad
{
    public class UsuarioValidacion
    {

        public int UsuarioID { get; set; }
        public string TokenValidacion { get; set; }
        public DateTime FechaExpiracion { get; set; }

    }
}
