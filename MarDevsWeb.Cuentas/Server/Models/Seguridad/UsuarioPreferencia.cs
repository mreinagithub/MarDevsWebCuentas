using System.ComponentModel.DataAnnotations;

namespace MarDevsWeb.Cuentas.Server.Models.Seguridad
{
    public class UsuarioPreferencia
    {
                
        public int UsuarioID { get; set; }
        public bool MostrarSaldoAcumuladoEntrePeriodos { get; set; }        
        public string Tema { get; set; }

    }
}
