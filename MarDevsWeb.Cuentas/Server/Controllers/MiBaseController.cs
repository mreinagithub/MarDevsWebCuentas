using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MarDevsWeb.Cuentas.Server.Excepciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Server.Controllers
{
    public class MiBaseController : ControllerBase
    {

        protected readonly MarDevsContext _context;

        public MiBaseController(MarDevsContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Obtiene el Identificador de usuario logueado
        /// </summary>
        public int YO
        {
            get { return Convert.ToInt32(User.Identity.Name); }
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
            if (ex is DbUpdateConcurrencyException) //Chequear cuando se implemente versinado, que ante errores de concurrencia caiga acá.
                return new ExcepcionConcurrencia(STR_ERROR_CONCURRENCIA, ex);
            if ((ex.InnerException is SqlException) && ((ex.InnerException as SqlException).Number == 547))//violacion de foreign key
                return new ExcepcionEliminacion(STR_ERROR_ELIMINAR_FK, ex);
            else if ((ex.InnerException is SqlException) && ((ex.InnerException as SqlException).Number == 2627))//violacion de unique al insertar
                return new ExcepcionInsertClaveDuplicada(STR_ERROR_INSERTAR_UK, ex);
            else if ((ex.InnerException is SqlException) && ((ex.InnerException as SqlException).Number == 2601))//violacion de unique al insertar
                return new ExcepcionInsertClaveDuplicada(STR_ERROR_INSERTAR_UK, ex);
            else if ((ex.InnerException is SqlException) && ((ex.InnerException as SqlException).Number == 50000))//Raiserror
                return new ExcepcionRaiserrorUsuario(ex.InnerException.Message, ex);
            else if ((ex.InnerException is SqlException))//Otro error SQL que siempre viene en el inner
                return new ExcepcionRaiserrorUsuario(ex.InnerException.Message, ex);
            //DADO QUE SE PRODUJO UNA EXCEPCION, DEBEMOS RESETEAR LOS ID'S            
            //return new ExcepcionTecnica(STR_ERROR_ACCESO_DATOS, ex);
            return ex;

        }

        #endregion
    }
}
