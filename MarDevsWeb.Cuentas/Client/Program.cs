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

namespace MarDevsWeb.Cuentas.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            ConfigureSerilog();

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            ConfigureServices(builder.Services);

            

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

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddScoped<IRepositorio, Repositorio>();            
            services.AddScoped<IMostrarMensajes, MostrarMensajes>();
            services.AddAuthorizationCore();
            //Creamos una instancia del proveedor
            services.AddScoped<ProveedorAuthenticacionJWT>();
            //El provider le dice al servicio que funciona como authenticationstateprovider que utilice la
            //instancia creada arriba.
            services.AddScoped<AuthenticationStateProvider, ProveedorAuthenticacionJWT>(
                provider => provider.GetRequiredService<ProveedorAuthenticacionJWT>());
            services.AddScoped<ILoginService, ProveedorAuthenticacionJWT>(
                provider => provider.GetRequiredService<ProveedorAuthenticacionJWT>());

            services.AddScoped<RenovadorToken>();

            //Para almacenar información del error a mostrar
            services.AddScoped<ErrorHandler>();
        }
    }
}
