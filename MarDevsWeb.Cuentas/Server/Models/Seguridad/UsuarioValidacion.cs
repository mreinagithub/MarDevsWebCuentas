using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarDevsWeb.Cuentas.Server.Models.Seguridad
{
    [Table("usuario_validacion")]
    public class UsuarioValidacion
    {

        [Column("usuario_id")]
        public int UsuarioID { get; set; }
        [Column("token_validacion")]
        public string TokenValidacion { get; set; }
        [Column("fecha_expiracion")]
        public DateTime FechaExpiracion { get; set; }

    }
}
