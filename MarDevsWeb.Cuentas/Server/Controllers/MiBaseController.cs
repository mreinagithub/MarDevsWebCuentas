using MarDevsWeb.Cuentas.Server.Excepciones;
using MarDevsWeb.Cuentas.Server.Models;
using MarDevsWeb.Cuentas.Server.Models.Seguridad;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SGAWeb.Server.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Server.Controllers
{
    public class MiBaseController : ControllerBase
    {

        protected readonly MarDevsContext _context;
        protected readonly HoraService _horaService;


        public MiBaseController(MarDevsContext context, HoraService horaService)
        {
            _context = context;
            _horaService = horaService;
        }


        /// <summary>
        /// Obtiene el Identificador de usuario logueado
        /// </summary>
        public int YO
        {
            get { return Convert.ToInt32(User.Identity.Name); }
            //get
            //{
            //    var email = User.Claims.FirstOrDefault(e => e.Type.Equals(ClaimTypes.Email)).Value.ToString();
            //    int id = _context.Usuario.FirstOrDefault(u => u.Email.Equals(email)).Id.Value;
            //    return id;
            //}
        }

        #region WRAP EXCEPCIONES


        private static string STR_ERROR_CONCURRENCIA = "El objeto que intenta actualizar ha sido modificado "
                                                    + "por otro usuario." + System.Environment.NewLine
                                                    + "La operación no pudo concretarse.";

        private static string STR_ERROR_ACCESO_DATOS = "Se ha producido un error al intentar acceder a la "
                                                        + "base de datos." + System.Environment.NewLine
                                                        + "La operación no pudo concretarse.";

        private static string STR_ERROR_ELIMINAR_FK = "Se ha producido un error al intentar eliminar "
                                                        + "el elemento." + System.Environment.NewLine
                                                        + "Hay otros elementos que dependen de él "
                                                        + "y por lo tanto no puede eliminarse.";


        private static string STR_ERROR_INSERTAR_UK = "Se ha producido un error al intentar insertar "
            + "el elemento." + System.Environment.NewLine
            + "Está intentando insertar un elemento que ya existe.";

        protected Exception WrapException(Exception ex)
        {
            if (ex is DbUpdateConcurrencyException) //Chequear cuando se implemente versionado, que ante errores de concurrencia caiga acá.
                return new ExcepcionConcurrencia(STR_ERROR_CONCURRENCIA, ex);

            if ((ex.InnerException is PostgresException pgEx))
            {
                switch (pgEx.SqlState)
                {
                    case "23503": // foreign_key_violation
                        return new ExcepcionEliminacion(STR_ERROR_ELIMINAR_FK, ex);

                    case "23505": // unique_violation (cubre lo que antes eran 2627 y 2601)
                        return new ExcepcionInsertClaveDuplicada(STR_ERROR_INSERTAR_UK, ex);

                    case "P0001": // RAISE EXCEPTION genérico (equivalente a RAISERROR de SQL Server)
                        return new ExcepcionRaiserrorUsuario(pgEx.MessageText, ex);
                }
            }

            return ex;

        }

        #endregion

        #region CADA ENTIDAD FILTRADA

        protected IQueryable<Periodo> PeriodosUsuario
        {
            get
            {
                return _context.Periodo.Where(p => p.CreadoPor == YO).AsQueryable();                
            }
        }
        protected IQueryable<Concepto> ConceptosUsuario
        {
            get
            {
                return _context.Concepto.Where(c => c.CreadoPor == YO).AsQueryable();
            }
        }
        protected IQueryable<Movimiento> MovimientoUsuario
        {
            get
            {
                return _context.Movimiento.Where(g => g.CreadoPor == YO).AsQueryable();
            }
        }
        protected IQueryable<Rubro> RubrosUsuario
        {
            get
            {
                return _context.Rubro.Where(r => r.CreadoPor == YO).AsQueryable();
            }
        }
        protected UsuarioPreferencia PreferenciaUsuario
        {
            get
            {
                var flags = _context.UsuarioPreferencia.Where(r => r.UsuarioID == YO).FirstOrDefault();
                if(flags == null)
                {
                    flags = new UsuarioPreferencia();
                    flags.UsuarioID = YO;
                    flags.MostrarSaldoAcumuladoEntrePeriodos = true;
                    flags.Tema = "CLARO";

                    _context.Add(flags);
                    _context.SaveChanges();
                }
                return flags;
            }
        }


        #endregion


    }
}
