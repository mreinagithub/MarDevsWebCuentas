using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Server.Servicios
{
    public interface IMailService
    {
        Task SendEmailAsync(string para, string asuntoSubject, string cuerpo, bool esCuerpoHtml);
    }
}
