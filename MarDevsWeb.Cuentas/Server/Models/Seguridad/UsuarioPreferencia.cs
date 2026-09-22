using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarDevsWeb.Cuentas.Server.Models.Seguridad
{
    [Table("usuario_preferencia")]
    public class UsuarioPreferencia
    {

        [Column("usuario_id")]
        public int UsuarioID { get; set; }
        [Column("mostrar_saldo_acumulador_entre_periodos")]
        public bool MostrarSaldoAcumuladoEntrePeriodos { get; set; }
        [Column("tema")]
        public string Tema { get; set; }

    }
}
