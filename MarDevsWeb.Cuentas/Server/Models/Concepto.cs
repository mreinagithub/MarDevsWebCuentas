using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarDevsWeb.Cuentas.Server.Models
{
    public class Concepto : Persistente<Guid?>, IAuditable
    {


        [Column("ConceptoID")]
        public override Guid? Id { get => base.Id; set => base.Id = value; }
        public string Descripcion { get; set; }
        public DateTime CreadoEl { get; set; }
        public int CreadoPor { get; set; }


    }
}
