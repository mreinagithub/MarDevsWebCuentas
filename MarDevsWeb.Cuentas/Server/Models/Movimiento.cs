using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarDevsWeb.Cuentas.Server.Models
{
    public class Movimiento : Persistente<Guid?>, IAuditable
    {


        [Column("MovimientoID")]
        public override Guid? Id { get => base.Id; set => base.Id = value; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; }
        public Guid ConceptoID { get; set; }
        public Concepto Concepto { get; set; }
        public decimal Importe { get; set; }
        public string Observaciones { get; set; }
        public DateTime CreadoEl { get; set; }
        public int CreadoPor { get; set; }


    }
}
