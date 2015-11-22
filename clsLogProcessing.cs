using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace LAMSoft.ISACFMgr
{
	static class clsLogProcessing
	{

		const string _errorLogFileName = "error.xml";

		public static void WriteToEventLog(string Message, EventLogEntryType type )
		{
			ISACFMgr s = new ISACFMgr();
			try
			{
				EventLog.WriteEntry(s.ServiceName, Message, type);
			}
			catch (System.Exception)
			{
				// Process the record of the event by other media, for example save to a logFile.
				;
			}
		}

		public static void WriteToLogFile(string Message, EventLogEntryType type)
		{
			throw new System.NotImplementedException("El método \"WriteToLogFile\" de la clase \"clsLogProcessing\" no está implementado aún.");
		}

		public static void reportRecoveryAction(ISACFMgr.operationList currentOperation)
		{
			WriteToEventLog(string.Format("El servicio ha iniciado en modo de recuperación, al parecer el anterior cierre " + 
			"del sistema fue inesperado ó este servicio se encontraba procesando la ejecución de la operación: \"{0}\". " +
			"No se requiere ninguna acción por parte del usuario, el sistema se encargará de continuar las operaciones " + 
			"a partir de este punto.", currentOperation), EventLogEntryType.Warning);
		}

	}
}
