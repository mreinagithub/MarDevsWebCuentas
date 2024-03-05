using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Shared.DTOs
{
    public class EditarPeriodoDTO
    {
        public Guid? PeriodoId { get; set; } = null;
        [Required(ErrorMessage = "Campo fecha desde obligatorio")]
        public DateTime? FechaDesde { get; set; } = null;
    }
}
