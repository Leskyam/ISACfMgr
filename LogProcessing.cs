using System.Diagnostics;

namespace LAMSoft.ISACFMgr
{
    static class LogProcessing
	{
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

		public static void ReportRecoveryAction(ISACFMgr.operationList currentOperation)
		{
			WriteToEventLog(string.Format("El servicio ha iniciado en modo de recuperación, al parecer el anterior cierre " + 
			"del sistema fue inesperado ó este servicio se encontraba procesando la ejecución de la operación: \"{0}\". " +
			"No se requiere ninguna acción por parte del usuario, el sistema se encargará de continuar las operaciones " + 
			"a partir de este punto.", currentOperation), EventLogEntryType.Warning);
		}

	}
}
