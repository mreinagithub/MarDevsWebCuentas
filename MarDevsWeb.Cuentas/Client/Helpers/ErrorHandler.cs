using MarDevsWeb.Cuentas.Client.Servicios;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Client.Helpers
{
    public class ErrorHandler
    {
        private readonly NavigationManager navigationManager;
        private readonly ToastService toastService;

        public ErrorHandler(NavigationManager navigationManager, ToastService toastService)
        {
            this.navigationManager = navigationManager;
            this.toastService = toastService;
        }


        public int StatusCode { get; set; }
        public string Message { get; set; }

        public async Task MostrarError(HttpResponseMessage httpResponseMessage, bool usarToast = false)
        {
            MostrarError((int)httpResponseMessage.StatusCode,
                await httpResponseMessage.Content.ReadAsStringAsync(), usarToast);
        }

        public void MostrarError(int statusCode, string mensaje, bool usarToast = false)
        {
            StatusCode = statusCode;

            if (StatusCode == 401)
            {
                ManejarUsuarioNoAutorizado();
            }
            else if (StatusCode == 500)
            {
                Message = "Se ha producido un error interno. Si el problema persiste, contacte al administrador.";
                navigationManager.NavigateTo("./error");
            }
            else if (usarToast)
            {
                toastService.ShowToast(mensaje, ToastLevel.Error);
            }
            else
            {
                Message = mensaje;
                navigationManager.NavigateTo("./error");
            }
        }
        public void ManejarUsuarioNoAutorizado()
        {
            Message = "Parece que no tiene permisos para acceder a este sitio. Su sesión ha expirado";
            navigationManager.NavigateTo("./no-autoirzado");
        }
    }
}
