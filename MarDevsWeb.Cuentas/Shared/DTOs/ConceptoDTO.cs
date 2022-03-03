using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Shared.DTOs
{
    public class ConceptoDTO
    {
        public Guid Id { get; set; }
        public string Tipo { get; set; }
        public string Descripcion { get; set; }
        public string Rubro { get; set; }
    }
}
