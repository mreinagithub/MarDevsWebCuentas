using System;

namespace MarDevsWeb.Cuentas.Server.Excepciones
{
	[Serializable]
	public class ExcepcionInsertClaveDuplicada : ExcepcionBase
	{
		public ExcepcionInsertClaveDuplicada(): base()
		{		}
		public ExcepcionInsertClaveDuplicada(string pMensaje): base(pMensaje)
		{		}
		public ExcepcionInsertClaveDuplicada(string pMensaje, Exception pInner): base(pMensaje,pInner)
		{		}
	}

}
