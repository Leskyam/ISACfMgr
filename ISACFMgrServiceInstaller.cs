using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace LAMSoft.ISACFMgr
{
	[RunInstaller(true)]
	public partial class ISACFMgrServiceInstaller : Installer
	{
		public ISACFMgrServiceInstaller()
		{
			ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller();
			ServiceInstaller serviceInstaller = new ServiceInstaller();

			//# Service Account Information
			serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
			serviceProcessInstaller.Username = null;
			serviceProcessInstaller.Password = null;

			//# This must be identical to the WindowsService.ServiceBase name
			//# set in the constructor of WindowsService.cs
			serviceInstaller.ServiceName = "ISACFMgr";
			//# Service Information
			serviceInstaller.DisplayName = "ISA Categorization & Content Filter Manager";
			serviceInstaller.Description = "Obtiene, descargando desde una dirección URL y procesa, " +
																		 "convirtiendo a formato XML y luego actualizando la base " + 
																		 "de datos de ISA Server, la lista de sitios y dominios " + 
																		 "categorizados según el contenido descargado.";
			//# Startup type of the service.
			serviceInstaller.StartType = ServiceStartMode.Automatic;

			this.Installers.Add(serviceProcessInstaller);
			this.Installers.Add(serviceInstaller);
		}

	}

}
