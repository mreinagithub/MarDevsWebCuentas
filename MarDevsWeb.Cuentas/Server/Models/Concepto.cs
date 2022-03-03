using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarDevsWeb.Cuentas.Server.Models
{
    public class Concepto : Persistente<Guid?>, IAuditable
    {


        [Column("ConceptoID")]
        public override Guid? Id { get => base.Id; set => base.Id = value; }
        public string Tipo { get; set; }
        public string Descripcion { get; set; }
        public Guid? RubroID { get; set; }
        public Rubro Rubro { get; set; }
        public DateTime CreadoEl { get; set; }
        public int CreadoPor { get; set; }


    }
}
