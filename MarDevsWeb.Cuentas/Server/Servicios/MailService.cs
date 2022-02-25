using MarDevsWeb.Cuentas.Server.Excepciones;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Server.Servicios
{
    public class MailService : IMailService
    {

        private readonly MarDevsContext _context;
        public MailService(MarDevsContext context)
        {
            _context = context;
        }

        public async Task SendEmailAsync(string para, string asuntoSubject, string cuerpo, bool esCuerpoHtml = false)
        {

            var flags = await _context.FlagsCuentas.FirstOrDefaultAsync();
            if (flags == null)
                throw new ExcepcionNegocios("No se lograron obtener los parámetros de aplicación");

            if (string.IsNullOrWhiteSpace(flags.MailSmtp) || flags.MailPort == 0)
                throw new ExcepcionNegocios("No se indicó el smtp y/o puerto de correo");
            if (string.IsNullOrWhiteSpace(flags.MailUserAuth) || string.IsNullOrWhiteSpace(flags.MailPassAuth))
                throw new ExcepcionNegocios("No se indicó usuario y/o clave de correo");
            if (string.IsNullOrWhiteSpace(flags.MailFrom))
                throw new ExcepcionNegocios("No se indicó el remitente del correo.");


            MailMessage message = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            message.From = new MailAddress(flags.MailFrom, flags.MailFromDisplayName  ?? flags.MailFrom);
            message.To.Add(new MailAddress(para));
            message.Subject = asuntoSubject;
            message.IsBodyHtml = esCuerpoHtml;
            message.Body = cuerpo;            
            smtp.Port = flags.MailPort;
            smtp.Host = flags.MailSmtp;
            smtp.EnableSsl = flags.HabilitarSSL;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(flags.MailUserAuth, flags.MailPassAuth);
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            await smtp.SendMailAsync(message);
        }

    }
}
