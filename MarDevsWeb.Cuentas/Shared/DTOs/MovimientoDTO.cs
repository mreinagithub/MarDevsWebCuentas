using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Shared.DTOs
{
    public class HeaderMovimientoDTO
    {

        public decimal SaldoInicial { get; set; }
        public List<MovimientoDTO> Movimientos { get; set; }
    }

    public class MovimientoDTO
    {
        public Guid Id { get; set; }
        public string Tipo { get; set; }
        public string Concepto { get; set; }
        public decimal Importe { get; set; }
        public DateTime Fecha { get; set; }
        public string Observaciones { get; set; }

    }
}
