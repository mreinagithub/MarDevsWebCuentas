using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarDevsWeb.Cuentas.Server.Models
{
    [Table("concepto")]
    public class Concepto : Persistente<Guid?>, IAuditable
    {


        [Column("concepto_id")]
        public override Guid? Id { get => base.Id; set => base.Id = value; }
        [Column("tipo")]
        public string Tipo { get; set; }
        [Column("descripcion")]
        public string Descripcion { get; set; }
        [Column("rubro_id")]
        public Guid? RubroID { get; set; }        
        public Rubro Rubro { get; set; }
        [Column("creado_el")]
        public DateTime CreadoEl { get; set; }
        [Column("creado_por")]
        public int CreadoPor { get; set; }


    }
}
