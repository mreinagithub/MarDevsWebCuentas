using System;
using System.ComponentModel;

namespace MarDevsWeb.Cuentas.Server.Models
{
	/// <summary>
	/// Descripción breve de IFlagsSeguridad.
	/// </summary>
	public interface IFlagsSeguridad
	{
        int? Id { get; }

        //ModoBlanqueoPasswordEnum ModoBlanqueoPassword {get;set;}
        //string ValorPasswordFijo {get;set;}
		int PasswordLongitudMinima {get;set;}
		int PasswordLongitudMaxima {get;set;}
		int DiasVigenciaPassword {get;set;}
	}
}
