using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarDevsWeb.Cuentas.Server.Models
{
    /// <summary>
    /// Clase que implementa los flags del modelo de seguridad de Daruma.Comun
    /// se puede heredar de esta clase en cualquier sistema que utilice seguridad
    /// para agregar Flags propios
    /// </summary>    
    /// 
    [Table("flags_seguridad")]
    public class FlagsSeguridad: IFlagsSeguridad
	{
        public FlagsSeguridad()
		{
		}

		[Browsable(false)]
        [Column("flags_seguridad_id")]
        public int? Id { get; set;}

        #region Seguridad

        [Column("password_longitud_minima")]
        [DisplayName("Longítud Mínima Password"), Range(1, 50, ErrorMessage = "La longítud mínima debe ser 1"), Required(ErrorMessage = "El valor es requerido")]
        public int PasswordLongitudMinima { get; set; }
        [Column("password_longitud_maxima")]
        [DisplayName("Longítud Máxima Password"), Range(1, 50, ErrorMessage = "La longítud mínima debe ser 1"), Required(ErrorMessage = "El valor es requerido")]
        public int PasswordLongitudMaxima { get; set; }
        [Column("dias_vigencia_password")]
        [DisplayName("Días Vigencia Password default (0 - No vence)"), Range(0, int.MaxValue, ErrorMessage = "El valor no puede ser inferior a cero."), Required(ErrorMessage = "El valor es requerido")]
        public int DiasVigenciaPassword { get; set; }

		#endregion
        
       
    }
}
