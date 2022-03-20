using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Shared.DTOs
{
    public class ListasFiltroMovimientosDTO
    {
        public List<PeriodoDTO> Periodos { get; set; }
        public List<RubroDTO> Rubros { get; set; }

    }
}
