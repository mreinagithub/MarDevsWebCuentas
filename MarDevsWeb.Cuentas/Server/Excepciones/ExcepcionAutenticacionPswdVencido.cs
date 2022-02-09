using System;

namespace MarDevsWeb.Cuentas.Server.Excepciones
{
	[Serializable]
	public class ExcepcionAutenticacionPswdVencido : ExcepcionBase
	{
		public ExcepcionAutenticacionPswdVencido(): base()
		{
        }
		public ExcepcionAutenticacionPswdVencido(string pMensaje): base(pMensaje)
		{
        }
		public ExcepcionAutenticacionPswdVencido(string pMensaje, Exception pInner)
			: base(pMensaje, pInner)
		{
        }
	}
}
