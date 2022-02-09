using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Client.Helpers
{
    public class MostrarMensajes : IMostrarMensajes
    {
        private readonly IJSRuntime jSRuntime;

        public MostrarMensajes(IJSRuntime jSRuntime)
        {
            this.jSRuntime = jSRuntime;
        }

        public async Task MostrarMensajeError(string Mensaje)
        {
            await MostrarMensaje("Error", Mensaje);
        }
        public async Task MostrarMensajeExitoso(string Mensaje)
        {
            await MostrarMensaje("Exitoso", Mensaje);
        }

        private async ValueTask MostrarMensaje(string titulo, string mensaje)
        {
            await jSRuntime.InvokeVoidAsync("MostrarMensaje", titulo, mensaje);
        }

        
    }
}
