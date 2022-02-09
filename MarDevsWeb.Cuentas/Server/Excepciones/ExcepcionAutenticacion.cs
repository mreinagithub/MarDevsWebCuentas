using System;

namespace MarDevsWeb.Cuentas.Server.Excepciones
{
	[Serializable]
	public class ExcepcionAutenticacion : ExcepcionBase
	{
		public ExcepcionAutenticacion(): base()
		{		}
		public ExcepcionAutenticacion(string pMensaje): base(pMensaje)
		{		}
		public ExcepcionAutenticacion(string pMensaje, Exception pInner): base(pMensaje,pInner)
		{		}
	}
}
