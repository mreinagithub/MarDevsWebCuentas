using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Shared.DTOs
{
    public class ResumenHomeDTO
    {
        public DateTime FechaDesde { get; set; }
        public bool MostrarSaldoInicial { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal Saldo { get; set; }

    }
}
