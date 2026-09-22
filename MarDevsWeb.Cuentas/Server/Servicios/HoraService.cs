using MarDevsWeb.Cuentas.Server;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Data;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace SGAWeb.Server.Servicios
{
    public class HoraService
    {

        private readonly MarDevsContext _context;
        
        public HoraService(MarDevsContext context)
        {
            _context = context;            

        }

        private DateTime? _fechaYHoraActual = null;

        /// <summary>
		/// Obtiene la Fecha y Hora del Servidor de Base de Datos y retiene la diferencia con la fecha de la Maquina.
		/// </summary>
		public DateTime ObtenerHoraServidor()
        {            
            try
            {
                DateTime fechaServer;
                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = "SELECT NOW()";                    
                    if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();
                    var fechaRaw = Convert.ToDateTime(cmd.ExecuteScalar());
                    fechaServer = DateTime.SpecifyKind(fechaRaw, DateTimeKind.Utc);
                }            

                return fechaServer;
                

            }
            catch (PostgresException)
            {
                throw;
            }
            catch
            {
                throw new Exception("No fue posible sincronizar la hora con el servidor.");
            }            
        }
        /// <summary>
        /// Devuelve la fecha y hora actual UTC.
        /// </summary>
        public DateTime FechaYHoraActualUTC
        {
            get
            {
                if(_fechaYHoraActual == null)
                {
                    _fechaYHoraActual = ObtenerHoraServidor();
                }

                return _fechaYHoraActual.Value;
            }
        }
        /// <summary>
        /// Devuelve la fecha actual, sin incluir horas minutos o segundos UTC.
        /// </summary>
        public DateTime FechaActualUTC
        {
            get { return FechaYHoraActualUTC.Date; }
        }

        /// <summary>
        /// Devuelve la fecha y hora actual LOCAL.
        /// </summary>
        public DateTime FechaYHoraActualLOCAL
        {
            get
            {
                if (_fechaYHoraActual == null)
                {
                    _fechaYHoraActual = ObtenerHoraServidor();
                }

                //Converimos a hora local, porque si bien posgresql esta configurado con la zona horaria de Buenos Aires, el servidor puede estar en otra zona horaria.
                var fechaLocal = TimeZoneInfo.ConvertTimeFromUtc(
                    _fechaYHoraActual.Value,
                    TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires"));
                return DateTime.SpecifyKind(fechaLocal, DateTimeKind.Local);

            }
        }

        /// <summary>
        /// Devuelve la fecha actual, sin incluir horas minutos o segundos LOCAL.
        /// </summary>
        public DateTime FechaActualLOCAL
        {           

            get { return FechaYHoraActualLOCAL.Date; }
        }


    }
}
