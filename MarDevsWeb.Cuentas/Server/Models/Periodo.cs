using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarDevsWeb.Cuentas.Server.Models
{
    public class Periodo : Persistente<Guid?>, IAuditable
    {
        public Periodo()
        {
        }

        [Browsable(false)]
        [Column("PeriodoID")]
        public override Guid? Id { get => base.Id; set => base.Id = value; }
        public DateTime FechaDesde { get; set; }
        public DateTime CreadoEl { get; set; }
        public int CreadoPor { get; set; }
    }
}
