using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarDevsWeb.Cuentas.Server.Models
{
    public class Gasto : Persistente<Guid?>, IAuditable
    {


        [Column("GastoID")]
        public override Guid? Id { get => base.Id; set => base.Id = value; }
        public DateTime Fecha { get; set; }
        public Guid ConceptoID { get; set; }
        public Concepto Concepto { get; set; }
        public decimal Importe { get; set; }
        public string Observaciones { get; set; }
        public DateTime CreadoEl { get; set; }
        public int CreadoPor { get; set; }


    }
}
