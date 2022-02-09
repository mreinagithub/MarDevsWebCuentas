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

          

        }

        //Seguridad        
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<FlagsSeguridad> FlagsSeguridad { get; set; }

       


    }   

}
