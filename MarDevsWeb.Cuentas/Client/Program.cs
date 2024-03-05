using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MarDevsWeb.Cuentas.Client.Auth;
using MarDevsWeb.Cuentas.Client.Helpers;
using MarDevsWeb.Cuentas.Client.Repositorios;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Serilog.Core;
using Serilog;
using System.Globalization;
using MarDevsWeb.Cuentas.Client.Servicios;


namespace MarDevsWeb.Cuentas.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            ConfigureSerilog();

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");        

            ConfigureServices(builder);            


            CultureInfo culture = new CultureInfo("es-AR");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            await builder.Build().RunAsync();
        }

        private static void ConfigureSerilog()
        {
            // In a Blazor WASM Program.cs file
            var levelSwitch = new LoggingLevelSwitch();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(levelSwitch)
                .WriteTo.BrowserHttp(controlLevelSwitch: levelSwitch)
                .CreateLogger();

            Log.Information("Hola, Blazor Cli!");
        }

        private static void ConfigureServices(WebAssemblyHostBuilder builder)
        {

            IServiceCollection services = builder.Services;

            //services.AddScoped(sc =>
            //{
            //    var logService = sc.GetRequiredService<ProveedorAuthenticacionJWT>();

            //    return new RenovadorTokenHandler()
            //    {
            //        InnerHandler = new HttpClientHandler()
            //    };
            //});
            services.AddScoped(sp =>
            {
                var handler = sp.GetRequiredService<RenovadorTokenHandler>();

                return new HttpClient(handler)
                {
                    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
                };
            });

            services.AddScoped(sp => new HttpClient() { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });


            services.AddOptions();            
            services.AddScoped<IMostrarMensajes, MostrarMensajes>();
            services.AddScoped<IRepositorio, Repositorio>();
            services.AddAuthorizationCore();
            //Creamos una instancia del proveedor
            services.AddScoped<ProveedorAuthenticacionJWT>();
            //El provider le dice al servicio que funciona como authenticationstateprovider que utilice la
            //instancia creada arriba.
            services.AddScoped<AuthenticationStateProvider, ProveedorAuthenticacionJWT>(
                provider => provider.GetRequiredService<ProveedorAuthenticacionJWT>());
            services.AddScoped<ILoginService, ProveedorAuthenticacionJWT>(
                provider => provider.GetRequiredService<ProveedorAuthenticacionJWT>());                   

            //Para almacenar información del error a mostrar
            services.AddScoped<ErrorHandler>();

            services.AddScoped<ToastService>();

            services.AddScoped<RenovadorTokenHandler>();            
           
           
            services.AddScoped(sp =>
            {
                var handler = sp.GetRequiredService<RenovadorTokenHandler>();
                handler.EndpointsIgnorados.Add("/api/cuenta/RefreshToken");
                handler.EndpointsIgnorados.Add("/api/cuenta/enviar-correo-recupero");
                handler.EndpointsIgnorados.Add("/api/cuenta/recuperar-clave");
                handler.EndpointsIgnorados.Add("/api/cuenta/registrar");
                handler.EndpointsIgnorados.Add("/api/cuenta/login");
                handler.EndpointsIgnorados.Add("/api/cuenta/validacion-correo");
                handler.EndpointsIgnorados.Add("/api/cuenta/GoogleSignIn");                


                return new HttpClient(handler)
                {
                    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
                };
            });

            




        }
    }
}
