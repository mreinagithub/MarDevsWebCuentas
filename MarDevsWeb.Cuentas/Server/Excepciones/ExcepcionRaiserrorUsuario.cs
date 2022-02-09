using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Server.Excepciones
{
    public class ExcepcionRaiserrorUsuario : ExcepcionBase
    {
        public ExcepcionRaiserrorUsuario() : base()
        { }
        public ExcepcionRaiserrorUsuario(string pMensaje) : base(pMensaje)
        { }
        public ExcepcionRaiserrorUsuario(string pMensaje, Exception pInner) : base(pMensaje, pInner)
        { }

        public override bool DebeConsiderarseError
        {
            get
            {
                return true;
            }
        }

    }
}
