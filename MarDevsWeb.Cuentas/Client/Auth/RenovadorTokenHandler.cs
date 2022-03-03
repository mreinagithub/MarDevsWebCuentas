using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace MarDevsWeb.Cuentas.Client.Auth
{

    public class RenovadorTokenHandler : DelegatingHandler
    {        
        public RenovadorTokenHandler(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            InnerHandler = new HttpClientHandler();
            EndpointsIgnorados = new List<string>();
        }

        public IServiceProvider serviceProvider { get; }

        /// <summary>
        /// Agregar acá endpoints que deben ser ignorados del intento de renovar token.
        /// Es decir que deben ser ignorados por el interceptor http.
        /// Debe comenzar con '/'
        /// </summary>
        public IList<string> EndpointsIgnorados { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {                       
            var uri = request.RequestUri;
            if (!EndpointsIgnorados.Any(i => uri.LocalPath.EndsWith(i)))
            {
                var loginService = serviceProvider.GetService(typeof(ILoginService));
                if (loginService != null && loginService is ILoginService)
                {
                    await (loginService as ILoginService).VerificarYRenovarToken(request);                    
                }                
            }
            return await base.SendAsync(request, cancellationToken);
        }


    }
}
