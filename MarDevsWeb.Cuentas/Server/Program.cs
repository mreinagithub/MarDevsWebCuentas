using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.Email;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

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
                CreateHostBuilder(args)
                    .UseSerilog()
                    .Build()
                    .Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminado inesperadamente");
            }
            finally
            {
                Log.CloseAndFlush();
            }            
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();                    
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
                            .Filter.ByExcluding(levent => levent.Exception is Excepciones.ExcepcionBase) //No incluir excepciones propias                            
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

                              .WriteTo.Email(
                                options: new EmailSinkOptions
                                {
                                    Host = "smtp.gmail.com",
                                    Port = 587,
                                    Credentials = new NetworkCredential("infomardevs@gmail.com", "qfpk ymgg foty otwt"),
                                    From = "infomardevs@gmail.com",
                                    To = destinatariosNotificacion.Split(',', ';').Select(x => x.Trim()).ToList(),
                                    ConnectionSecurity = MailKit.Security.SecureSocketOptions.StartTls,
                                    Subject = new MessageTemplateTextFormatter($"MarDevs Cuentas - Reporte de error"),
                                    Body = new MessageTemplateTextFormatter(
                                        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} - [{Level:u3}] - Usuario: {User} - {Message:lj}{NewLine}{Exception}"),
                                    IsBodyHtml = false
                                },
                                restrictedToMinimumLevel: LogEventLevel.Error)
                            .CreateLogger();
        }


    }
}
