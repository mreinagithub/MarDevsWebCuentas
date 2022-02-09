using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Client.Helpers
{
    public interface IMostrarMensajes
    {

        Task MostrarMensajeError(string Mensaje);
        Task MostrarMensajeExitoso(string Mensaje);

    }
}
