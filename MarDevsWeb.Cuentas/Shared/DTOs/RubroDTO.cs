using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Shared.DTOs
{
    public class RubroDTO
    {
        public Guid Id { get; set; }        
        public string Descripcion { get; set; }
        public string Color { get; set; } = "#000000";
        public int QConceptos { get; set; } = 0;
    }
}
