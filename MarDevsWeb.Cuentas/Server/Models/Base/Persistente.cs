using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarDevsWeb.Cuentas.Server.Models
{
	/// <summary>
	/// Case base para representar entidades de negocios que se persisten en forma independiente.
	/// Deriva de NegocioBase
	/// </summary>
	
	public abstract class Persistente<TIPOID> : IPersistente<TIPOID>
	{	
		[Browsable(false)]		
        public virtual TIPOID Id { get; set; }

        [Browsable(false), NotMapped]        
        public virtual object Yo
        {
            get { return this; }
        }               
		public virtual bool EsNuevo()
		{
            return Id == null || Id.Equals(default(TIPOID));
		}
        public virtual object ObtenerID()
        {
            return Id;
        }
        public virtual string ObtenerTipo()
		{
            return this.GetType().Name;
		}						
		public override bool Equals(object obj)
		{
			if (obj == null || this.GetType() != obj.GetType()) { return false; }
			if (this == obj) { return true; }
			if (this.EsNuevo()) { return false; }
			return (this.Id.Equals(((IPersistente<TIPOID>)obj).Id));
		}
		public override int GetHashCode()
		{
			return (this.EsNuevo()) ? 0 : this.Id.GetHashCode();
		}
		
    }
}
