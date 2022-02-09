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

        public ErrorHandler(NavigationManager navigationManager)
        {
            this.navigationManager = navigationManager;            
        }


        public int StatusCode { get; set; }
        public string Message { get; set; }

        public async Task MostrarError(HttpResponseMessage httpResponseMessage)
        {
            StatusCode = (int)httpResponseMessage.StatusCode;
            Message = await httpResponseMessage.Content.ReadAsStringAsync();            
            navigationManager.NavigateTo("./error");
        }
    }
}
