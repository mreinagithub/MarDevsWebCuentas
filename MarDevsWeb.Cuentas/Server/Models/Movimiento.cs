using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarDevsWeb.Cuentas.Server.Models
{
    [Table("movimiento")]
    public class Movimiento : Persistente<Guid?>, IAuditable
    {


        [Column("movimiento_id")]
        public override Guid? Id { get => base.Id; set => base.Id = value; }
        [Column("fecha")]
        public DateOnly Fecha { get; set; }
        [Column("tipo")]
        public string Tipo { get; set; }
        [Column("concepto_id")]
        public Guid ConceptoID { get; set; }        
        public Concepto Concepto { get; set; }
        [Column("importe", TypeName = "numeric(18,2)")]
        public decimal Importe { get; set; }
        [Column("observaciones")]
        public string Observaciones { get; set; }
        [Column("creado_el")]
        public DateTime CreadoEl { get; set; }
        [Column("creado_por")]
        public int CreadoPor { get; set; }


    }
}
