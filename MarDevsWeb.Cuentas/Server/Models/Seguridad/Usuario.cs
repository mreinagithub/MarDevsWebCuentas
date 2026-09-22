using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarDevsWeb.Cuentas.Server.Models.Seguridad
{

    [Table("usuario")]
    public class Usuario : Persistente<int?>
    {

        public Usuario()
            : base()
        {
        }    

      

        #region PROPIEDADES

        [Column("usuario_id")]
        public override int? Id { get => base.Id; set => base.Id = value; }
        [Column("email")]
        public virtual string Email { get; set; }
        [Column("email_validado")]
        public virtual bool EmailValidado { get; set; }        
        [Browsable(false)]
        [Column("password")]
        public virtual string Password { get; set; }
        [Column("nombre")]
        public virtual string Nombre { get; set; }        
        [Column("habilitado")]
        public virtual bool Habilitado { get; set; }
        [Column("fecha_ultimo_ingreso", TypeName = "timestamp without time zone")]
        public virtual DateTime? FechaUltimoIngreso { get; set; }
        [Column("fecha_ultimo_cambio_pass", TypeName = "timestamp without time zone")]
        public virtual DateTime? FechaUltimoCambioPassword { get; set; }
        [Column("password_temp_recupero")]
        public string PasswordTempRecupero { get; set; } = null;
        [Column("tipo_autenticacion")]
        public string TipoAutenticacion { get; set; } = UsuarioTipoAutenticacion.LOCAL.ToString();
        [Column("imagen_url")]
        public string ImagenURL { get; set; }


        #endregion

    }

    public enum UsuarioTipoAutenticacion
    {
        LOCAL,
        EXTER
    }
}
