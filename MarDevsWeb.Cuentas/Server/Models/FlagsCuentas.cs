using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarDevsWeb.Cuentas.Server.Models
{
    public class FlagsCuentas
    {
        public FlagsCuentas()
        {
        }

        [Browsable(false)]
        [Column("FlagsCuentasID")]
        public int? Id { get; set; }        
        public string MailSmtp { get; set; }        
        public int MailPort { get; set; }        
        public string MailUserAuth { get; set; }        
        public string MailPassAuth { get; set; }        
        public string MailFrom { get; set; }        
        public string MailFromDisplayName { get; set; }        
        public bool HabilitarSSL { get; set; }
        

    }
}
