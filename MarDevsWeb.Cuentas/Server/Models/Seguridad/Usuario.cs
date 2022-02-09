using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarDevsWeb.Cuentas.Server.Models.Seguridad
{
    public enum UsuarioModoToString
    {
        Logon = 1,
        ApeNom = 2,
        NomApe = 3
    }

    public class Usuario : Persistente<int?>, IAuditable
    {

        public Usuario()
            : base()
        {
        }

        private static UsuarioModoToString _modoToString = UsuarioModoToString.Logon;
        public static UsuarioModoToString ModoToString
        {
            get { return _modoToString; }
            set { _modoToString = value; }
        }

        private bool m_UsarVigenciaPasswordDefault = true;

        #region PROPIEDADES

        [Column("UsuarioID")]
        public override int? Id { get => base.Id; set => base.Id = value; }
        [Column("UsuarioLogon")]
        public virtual string Logon { get; set; }
        [Browsable(false)]
        [Column("UsuarioPass")]
        public virtual string Password { get; set; }
        [Column("UsuarioNombre")]
        public virtual string Nombre { get; set; }
        [Column("UsuarioApellido")]
        public virtual string Apellido { get; set; }
        [NotMapped]
        public virtual string NombreCompleto
        {
            get { return Apellido + " " + Nombre; }
        }
        [Column("UsuarioHabilitado")]
        public virtual bool Habilitado { get; set; }      
        public virtual DateTime? FechaUltimoIngreso { get; set; }
        [Column("FechaUltimoCambioPass")]
        public virtual DateTime? FechaUltimoCambioPassword { get; set; }
        [Column("UsarVigenciaPassDefault")]
        public virtual bool UsarVigenciaPasswordDefault
        {
            get { return m_UsarVigenciaPasswordDefault; }
            set
            {
                m_UsarVigenciaPasswordDefault = value;
                if (this.m_UsarVigenciaPasswordDefault)
                    this.DiasVigenciaPassword = 0;
            }
        }
        [Column("DiasVigenciaPass")]
        public virtual int DiasVigenciaPassword { get; set; }
        public virtual DateTime CreadoEl { get; set; }
        public virtual int CreadoPor { get; set; }     

        #endregion

        public override string ToString()
        {
            switch (Usuario.ModoToString)
            {
                case UsuarioModoToString.Logon:
                    return Logon;
                case UsuarioModoToString.ApeNom:
                    return String.Format("{0} {1}", Apellido, Nombre);
                case UsuarioModoToString.NomApe:
                    return String.Format("{0} {1}", Nombre, Apellido);
                default:
                    return Logon;
            }
        }

    }
}
