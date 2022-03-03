using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Shared.DTOs
{
    public class EditarConceptoDTO
    {
        public Guid? ConceptoId { get; set; } = null;
        [Required(ErrorMessage = "Debe indicar el tipo de concepto")]
        [RegularExpression("Egreso|Ingreso",ErrorMessage = "Valor del campo inválido")]
        public string TipoConcepto { get; set; }
        [Required(ErrorMessage = "Campo descripción obligatorio")]
        [MaxLength(100, ErrorMessage = "La descripción debe tener como máximo 100 caractéres")]
        public string Descripcion { get; set; } = null;
        public IEnumerable<RubrosDisponiblesDTO> RubrosDisponibles { get; set; }
        public Guid? RubroID { get; set; } = null;


    }

    public class RubrosDisponiblesDTO
    {
        public Guid RubroID { get; set; }
        public string Descripcion { get; set; }
    }
}
