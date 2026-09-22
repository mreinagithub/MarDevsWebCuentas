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
                .HasKey(a => new { a.UsuarioID, a.TokenValidacion });                
            modelBuilder.Entity<UsuarioRefreshToken>()
               .HasKey(a => new { a.UsuarioID, a.BrowserToken });
            modelBuilder.Entity<UsuarioPreferencia>()
                .HasKey(a => new { a.UsuarioID });
                

        }

        //Seguridad        
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<FlagsSeguridad> FlagsSeguridad { get; set; }
        public DbSet<UsuarioValidacion> UsuarioValidacion { get; set; }
        public DbSet<UsuarioRefreshToken> UsuarioRefreshToken { get; set; }
        public DbSet<UsuarioPreferencia> UsuarioPreferencia { get; set; }

        //Negocio
        public DbSet<Periodo> Periodo { get; set; }
        public DbSet<FlagsCuentas> FlagsCuentas { get; set; }
        public DbSet<Rubro> Rubro { get; set; }
        public DbSet<Concepto> Concepto { get; set; }
        public DbSet<Movimiento> Movimiento { get; set; }


    }   

}
