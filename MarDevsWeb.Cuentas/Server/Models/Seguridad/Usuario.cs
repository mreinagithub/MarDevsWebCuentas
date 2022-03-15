using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarDevsWeb.Cuentas.Server.Models.Seguridad
{ 

    public class Usuario : Persistente<int?>
    {

        public Usuario()
            : base()
        {
        }    

      

        #region PROPIEDADES

        [Column("UsuarioID")]
        public override int? Id { get => base.Id; set => base.Id = value; }
        [Column("Email")]
        public virtual string Email { get; set; }
        [Column("EmailValidado")]
        public virtual bool EmailValidado { get; set; }        
        [Browsable(false)]
        [Column("Password")]
        public virtual string Password { get; set; }
        [Column("Nombre")]
        public virtual string Nombre { get; set; }        
        [Column("Habilitado")]
        public virtual bool Habilitado { get; set; }      
        public virtual DateTime? FechaUltimoIngreso { get; set; }
        [Column("FechaUltimoCambioPass")]
        public virtual DateTime? FechaUltimoCambioPassword { get; set; }

        public string PasswordTempRecupero { get; set; } = null;

        public string TipoAutenticacion { get; set; } = UsuarioTipoAutenticacion.LOCAL.ToString();
        public string ImagenURL { get; set; }


        #endregion

    }

    public enum UsuarioTipoAutenticacion
    {
        LOCAL,
        EXTER
    }
}
