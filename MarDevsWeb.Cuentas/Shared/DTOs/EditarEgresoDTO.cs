using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Shared.DTOs
{
    public class EditarEgresoDTO
    {
        public Guid? EgresoID { get; set; } = null;        
        public IEnumerable<ConceptoDisponibleDTO> ConceptosDisponibles { get; set; }
        [Display(Name = "Concepto")]
        [Required(ErrorMessage = "Debe indicar el concepoto del egreso")]
        public Guid? ConceptoID { get; set; }
        [Display(Name = "Importe")]
        [Required(ErrorMessage = "Debe indicar el importe")]
        [Range(0.1, double.MaxValue, ErrorMessage = "El valor indicado es inválido")]
        public decimal? Importe { get; set; } = null;
        [Required(ErrorMessage ="Campo fecha obligatorio")]        
        public DateOnly Fecha { get; set; }
        public string Observaciones { get; set; }
    }

    public class ConceptoDisponibleDTO
    {
        public Guid ConceptoID { get; set; }
        public string Descripcion { get; set; }
    }
}
