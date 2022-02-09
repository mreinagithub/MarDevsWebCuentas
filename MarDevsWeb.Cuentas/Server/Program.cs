using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Email;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                ConfigurarLogger();
                Log.Information("Hola, Blazor Server!!");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminado inesperadamente");
            }
            finally
            {
                Log.CloseAndFlush();
            }
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                    .UseSerilog();
                });

        public static void ConfigurarLogger()
        {
            //Destino del archivo
            var path = Path.Combine(Environment.GetEnvironmentVariable("PROGRAMDATA"), "MarDevsCuentas") + "\\ErrLog.txt";

            //Destinatarios del correo            
            IConfigurationBuilder configBuilderForMain = new ConfigurationBuilder();
            configBuilderForMain.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfiguration configForMain = configBuilderForMain.Build();
            var destinatariosNotificacion = configForMain.GetSection("DestinatariosNotificacion").Value;
            if (String.IsNullOrWhiteSpace(destinatariosNotificacion)) destinatariosNotificacion = "martinreina84@hotmail.com";


            Log.Logger = new LoggerConfiguration()
                            .Enrich.FromLogContext()
                            .MinimumLevel.Debug()
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                            .Filter.ByExcluding(levent => levent.Exception is Excepciones.ExcepcionNegocios) //No incluir excepciones propias                            
                            .WriteTo.Console()
                            .WriteTo.File(path,
                                    restrictedToMinimumLevel: LogEventLevel.Error, //Poner en ERROR luego de las pruebas // Minimum Log level
                                    rollingInterval: RollingInterval.Day, // This will append time period to the filename like Example20180316.txt
                                    retainedFileCountLimit: null, //Sin limite para la cantidad de archivos rolling
                                    fileSizeLimitBytes: null,
                                     outputTemplate: "-----------------------------------------------------------------------------------------------------------------------" + Environment.NewLine
                                                  + "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} - [{Level:u3}] - Usuario: {User} - {Message:lj}{NewLine}{Exception}",  // Set custom file format                                    
                                    shared: true // Shared between multi-process shared log files
                                    )
                            .WriteTo.Email(new EmailConnectionInfo
                            {
                                MailServer = "smtp.gmail.com",
                                Port = 587,
                                NetworkCredentials = new NetworkCredential("fullcarmultimarcauai@gmail.com", "Fc070621!"),
                                FromEmail = "fullcarmultimarcauai@gmail.com",
                                ToEmail = destinatariosNotificacion,
                                EnableSsl = true,
                                EmailSubject = "MarDevs Gestión Web - Reporte de error"

                            },
                                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} - [{Level:u3}] - Usuario: {User} - {Message:lj}{NewLine}{Exception}",
                                    restrictedToMinimumLevel: LogEventLevel.Error//LogEventLevel.Error // Minimum Log level
                                    )
                            .CreateLogger();
        }


    }
}
