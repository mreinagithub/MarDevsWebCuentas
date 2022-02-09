using System;
using System.Runtime.Serialization;
using System.Reflection;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Text;
using System.Security;
using System.Security.Principal;
using System.Security.Permissions;
using System.Collections.Specialized;
using System.Resources;

namespace MarDevsWeb.Cuentas.Server.Excepciones
{	
	public class ExcepcionBase : ApplicationException
	{
		#region Constructors
		/// <summary>
		/// Constructor with no params.
		/// </summary>
		public ExcepcionBase() : base()
		{
			InitializeEnvironmentInformation();
		}
		/// <summary>
		/// Constructor allowing the Message property to be set.
		/// </summary>
		/// <param name="message">String setting the message of the exception.</param>
		public ExcepcionBase(string message) : base(message) 
		{
			InitializeEnvironmentInformation();
		}
		/// <summary>
		/// Constructor allowing the Message and InnerException property to be set.
		/// </summary>
		/// <param name="message">String setting the message of the exception.</param>
		/// <param name="inner">Sets a reference to the InnerException.</param>
		public ExcepcionBase(string message,Exception inner) : base(message, inner)
		{
			InitializeEnvironmentInformation();
		}
		/// <summary>
		/// Constructor used for deserialization of the exception class.
		/// </summary>
		/// <param name="info">Represents the SerializationInfo of the exception.</param>
		/// <param name="context">Represents the context information of the exception.</param>
		protected ExcepcionBase(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			machineName = info.GetString("machineName");
			createdDateTime = info.GetDateTime("createdDateTime");
			appDomainName = info.GetString("appDomainName");
			threadIdentity = info.GetString("threadIdentity");
			windowsIdentity = info.GetString("windowsIdentity");
			additionalInformation = (NameValueCollection)info.GetValue("additionalInformation",typeof(NameValueCollection));
		}
		#endregion

		#region Declare Member Variables
		// Member variable declarations
		private string machineName; 
		private string appDomainName;
		private string threadIdentity; 
		private string windowsIdentity; 
		private DateTime createdDateTime = DateTime.Now;

		// Collection provided to store any extra information associated with the exception.
		private NameValueCollection additionalInformation = new NameValueCollection();
		
		#endregion

		/// <summary>
		/// Override the GetObjectData method to serialize custom values.
		/// </summary>
		/// <param name="info">Represents the SerializationInfo of the exception.</param>
		/// <param name="context">Represents the context information of the exception.</param>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData( SerializationInfo info, StreamingContext context ) 
		{
			info.AddValue("machineName", machineName, typeof(string));
			info.AddValue("createdDateTime", createdDateTime);
			info.AddValue("appDomainName", appDomainName, typeof(string));
			info.AddValue("threadIdentity", threadIdentity, typeof(string));
			info.AddValue("windowsIdentity", windowsIdentity, typeof(string));
			info.AddValue("additionalInformation", additionalInformation, typeof(NameValueCollection));
			base.GetObjectData(info,context);
		}

		#region Public Properties
		/// <summary>
		/// Machine name where the exception occurred.
		/// </summary>
		public string MachineName
		{
			get
			{
				return machineName;
			}
		}

		/// <summary>
		/// Date and Time the exception was created.
		/// </summary>
		public DateTime CreatedDateTime
		{
			get
			{
				return createdDateTime;
			}
		}

		/// <summary>
		/// AppDomain name where the exception occurred.
		/// </summary>
		public string AppDomainName
		{
			get
			{
				return appDomainName;
			}
		}

		/// <summary>
		/// Identity of the executing thread on which the exception was created.
		/// </summary>
		public string ThreadIdentityName
		{
			get
			{
				return threadIdentity;
			}
		}

		/// <summary>
		/// Windows identity under which the code was running.
		/// </summary>
		public string WindowsIdentityName
		{
			get
			{
				return windowsIdentity;
			}
		}

		/// <summary>
		/// Collection allowing additional information to be added to the exception.
		/// </summary>
		public NameValueCollection AdditionalInformation
		{
			get
			{
				return additionalInformation;
			}
		}

		
		/// <summary>
		/// Determina si la excepcion debe Publicarse (al Event Logger del Sistema Operativo).
		/// </summary>
		public virtual bool DebeConsiderarseError
		{
			get	{	return false;	}
		}
		#endregion

		/// <summary>
		/// Initialization function that gathers environment information safely.
		/// </summary>
		private void InitializeEnvironmentInformation()
		{									
			try
			{				
				machineName = Environment.MachineName;
			}
			catch(SecurityException)
			{
				machineName = "PERMISO_DENEGADO";
				
			}
			catch
			{
				machineName = "ERROR_ACCESO";
			}
					
			try
			{
				threadIdentity = Thread.CurrentPrincipal?.Identity.Name;
			}
			catch(SecurityException)
			{
				threadIdentity = "PERMISO_DENEGADO";
			}
			catch
			{
				threadIdentity = "ERROR_ACCESO";
			}			
			
			try
			{
				windowsIdentity = WindowsIdentity.GetCurrent().Name;
			}
			catch(SecurityException)
			{
				windowsIdentity = "PERMISO_DENEGADO";
			}
			catch
			{
				windowsIdentity = "ERROR_ACCESO";
			}
			
			try
			{					
				appDomainName = AppDomain.CurrentDomain.FriendlyName;
			}
			catch(SecurityException)
			{
				appDomainName = "PERMISO_DENEGADO";
			}
			catch
			{
				appDomainName = "ERROR_ACCESO";
			}			
		}

	}	
	

}
