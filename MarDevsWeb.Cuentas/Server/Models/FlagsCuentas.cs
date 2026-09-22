using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarDevsWeb.Cuentas.Server.Models
{
    [Table("flags_cuentas")]
    public class FlagsCuentas
    {
        public FlagsCuentas()
        {
        }

        [Browsable(false)]
        [Column("flags_cuentas_id")]
        public int? Id { get; set; }
        [Column("mail_smtp")]
        public string MailSmtp { get; set; }
        [Column("mail_port")]
        public int MailPort { get; set; }
        [Column("mail_user_auth")]
        public string MailUserAuth { get; set; }
        [Column("mail_pass_auth")]
        public string MailPassAuth { get; set; }
        [Column("mail_from")]
        public string MailFrom { get; set; }
        [Column("mail_from_display_name")]
        public string MailFromDisplayName { get; set; }
        [Column("habilitar_ssl")]
        public bool HabilitarSSL { get; set; }
        

    }
}
