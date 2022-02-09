
namespace MarDevsWeb.Cuentas.Server.Models
{
	public interface IPersistente
	{
		object Yo { get;}
		
        object ObtenerID();
		bool EsNuevo();		
		string ObtenerTipo();

	}

	public interface IPersistente<TIPOID> : IPersistente
	{
		TIPOID Id { get;}
	}
}

