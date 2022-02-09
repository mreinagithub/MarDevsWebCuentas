using System;

namespace MarDevsWeb.Cuentas.Server.Excepciones
{
	[Serializable]
	public class ExcepcionEliminacion : ExcepcionBase
	{
		public ExcepcionEliminacion(): base()
		{		}
		public ExcepcionEliminacion(string pMensaje): base(pMensaje)
		{		}
		public ExcepcionEliminacion(string pMensaje, Exception pInner): base(pMensaje,pInner)
		{		}
	}

}
