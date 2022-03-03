using Microsoft.EntityFrameworkCore;
using MarDevsWeb.Cuentas.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarDevsWeb.Cuentas.Server.Models.Seguridad;

namespace MarDevsWeb.Cuentas.Server
{
    public class MarDevsContext : DbContext
    {
        public MarDevsContext(DbContextOptions<MarDevsContext> options)
       : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<UsuarioValidacion>()
                .HasKey("UsuarioID", "TokenValidacion");
            modelBuilder.Entity<UsuarioRefreshToken>()
               .HasKey("UsuarioID", "BrowserToken");


        }

        //Seguridad        
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<FlagsSeguridad> FlagsSeguridad { get; set; }
        public DbSet<UsuarioValidacion> UsuarioValidacion { get; set; }
        public DbSet<UsuarioRefreshToken> UsuarioRefreshToken { get; set; }

        //Negocio
        public DbSet<Periodo> Periodo { get; set; }
        public DbSet<FlagsCuentas> FlagsCuentas { get; set; }
        public DbSet<Rubro> Rubro { get; set; }
        public DbSet<Concepto> Concepto { get; set; }
        public DbSet<Movimiento> Movimiento { get; set; }


    }   

}
