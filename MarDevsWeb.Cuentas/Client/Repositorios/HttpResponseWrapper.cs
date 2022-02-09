using Microsoft.AspNetCore.Components;
using MarDevsWeb.Cuentas.Client.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Client.Repositorios
{
    public class HttpResponseWrapper<T>
    {        

        public HttpResponseWrapper(T response, bool error, HttpResponseMessage httpResponseMessage)
        {
            Error = error;            
            Response = response;
            HttpResponseMessage = httpResponseMessage;
        }

        public bool Error { get; set; }
        public T Response { get; set; }
        public HttpResponseMessage HttpResponseMessage { get; set; }

        public async Task<string> GetBody()
        {
            return await HttpResponseMessage.Content.ReadAsStringAsync();
        }
        public HttpStatusCode GetStatusCode()
        {
            return HttpResponseMessage.StatusCode;
        }
    }
}
