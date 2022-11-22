using MarDevsWeb.Cuentas.Server.Servicios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MarDevsWeb.Cuentas.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllersWithViews();
            services.AddRazorPages();

            string connString = Configuration.GetConnectionString("MarDevsContext");
            connString = connString.Replace("[USUARIO]", "mardevs");
            connString = connString.Replace("[PASSWORD]", "mDev@1686");

            services.AddDbContext<MarDevsContext>(options =>
             options.UseSqlServer(connString));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)                                
             .AddJwtBearer(opt =>             
             opt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
             {
                 ValidateIssuer = false,
                 ValidateAudience = false,
                 ValidateLifetime = true,
                 ValidateIssuerSigningKey = true,
                 IssuerSigningKey = new SymmetricSecurityKey(
                     Encoding.UTF8.GetBytes(Configuration["jwt:key"])),
                 ClockSkew = TimeSpan.Zero
             })
              .AddCookie()
                .AddGoogle(googleOpt =>
                {
                    googleOpt.ClientId = Configuration["Authentication:Google:ClientId"];
                    googleOpt.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                    googleOpt.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    googleOpt.ClaimActions.MapJsonKey("urn:google:picture", "picture","url");

                });

            services.AddScoped<IMailService, MailService>();

            services.AddHttpContextAccessor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //env.EnvironmentName = "Production";

            //app.UseDeveloperExceptionPage();
            //app.UseWebAssemblyDebugging();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();          

            //Harcodear información cultural a la  es-AR
            var defaultCulture = new CultureInfo("es-AR");
            app.UseRequestLocalization(opt =>
            {
                opt.DefaultRequestCulture = new RequestCulture(defaultCulture);
                opt.SupportedCultures = new List<CultureInfo> { defaultCulture };
                opt.SupportedUICultures = new List<CultureInfo> { defaultCulture };
            });

            //Middleware para agregar el usuario logueado al logger de errores
            app.Use(async (httpContext, next) =>
            {
                var userName = httpContext.User.Identity.IsAuthenticated ? httpContext.User.Identity.Name : "desconocido";
                Serilog.Context.LogContext.PushProperty("User", !String.IsNullOrWhiteSpace(userName) ? userName : "desconocido");
                await next.Invoke();
            });


            //app.UseSerilogIngestion();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            //app.UsePathBase("/Cuentas");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
