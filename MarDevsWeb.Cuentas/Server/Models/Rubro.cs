using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarDevsWeb.Cuentas.Server.Models
{
    public class Rubro : Persistente<Guid?>, IAuditable
    {
        [Column("RubroID")]
        public override Guid? Id { get => base.Id; set => base.Id = value; }        
        public string Descripcion { get; set; }
        public string Color { get; set; }
        public DateTime CreadoEl { get; set; }
        public int CreadoPor { get; set; }
    }
}
