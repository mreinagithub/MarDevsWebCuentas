using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Shared.DTOs
{
    public class PeriodoDTO
    {
        public Guid Id { get; set; }        
        public DateTime Desde { get; set; }
        public DateTime? Hasta { get; set; } = null;
        public int? Dias
        {
            get
            {
                if (Hasta != null)
                    return Hasta.Value.Subtract(Desde).Days;
                else
                    return null;
            }
        }
    }
}
