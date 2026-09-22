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
        public DateOnly Desde { get; set; }
        public DateOnly? Hasta { get; set; } = null;
        public int? Dias
        {
            get
            {
                if (Hasta.HasValue)
                    return Hasta.Value.DayNumber - Desde.DayNumber;

                return null;
            }
        }
    }
}
