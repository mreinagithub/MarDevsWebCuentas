using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarDevsWeb.Cuentas.Server.Models
{
    [Table("rubro")]
    public class Rubro : Persistente<Guid?>, IAuditable
    {
        [Column("rubro_id")]
        public override Guid? Id { get => base.Id; set => base.Id = value; }
        [Column("descripcion")]
        public string Descripcion { get; set; }
        [Column("color")]
        public string Color { get; set; }
        [Column("creado_el")]
        public DateTime CreadoEl { get; set; }
        [Column("creado_por")]
        public int CreadoPor { get; set; }
    }
}
