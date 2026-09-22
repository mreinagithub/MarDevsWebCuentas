using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarDevsWeb.Cuentas.Server.Models
{
    [Table("periodo")]
    public class Periodo : Persistente<Guid?>, IAuditable
    {
        public Periodo()
        {
        }

        [Browsable(false)]
        [Column("periodo_id")]
        public override Guid? Id { get => base.Id; set => base.Id = value; }
        [Column("fecha_desde")]
        public DateOnly FechaDesde { get; set; }
        [Column("creado_el")]
        public DateTime CreadoEl { get; set; }
        [Column("creado_por")]
        public int CreadoPor { get; set; }
    }
}
