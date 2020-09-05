using System;
using System.IO;
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;

namespace LAMSoft.ISACFMgr
{
    internal class ServiceSettings
	{
		private static string serviceName;
		private static string downloaderUserAgent;
		private static System.Uri downloadUri;
		private static DirectoryInfo workingDirectory;
		private static DirectoryInfo dataDirectory;
		private static DirectoryInfo logsDirectory;
		private static TimeSpan checkForUpdateInterval;
		private static ISACFMgr.operationList currentOperation;
		private static DateTime downloadedFileLastModifiedDate;
		private static string downloadedFileETag;
		private static string downloadedFileName;
		private static bool? useSqlTransaction = null;
		private static bool? processISARuleElements = null;
		// Fields for struct "struISAServer"
		private static string ISAServerName = null;
		private static string ISAServerUserName = null;
		private static string ISAServerUserDomain = null;
		private static string ISAServerUserPasswd = null;

		/// <summary>
		/// Service settings structure.
		/// </summary>
		internal struct StruServiceSettings
		{
			/// <summary>
			/// The service name, obtained from the class ISACFMgr.
			/// </summary>
			public string ServiceName
			{
				get
				{
					if(string.IsNullOrEmpty(serviceName))
					{
						ISACFMgr s = new ISACFMgr();
						serviceName = s.ServiceName;
					}
					return serviceName;
				}
				//set { _serviceName = value; }
			}

			/// <summary>
			/// User-Agent string used to request the downloads as nedded.
			/// </summary>
			public string DownloaderUserAgent
			{
				get
				{
					if(string.IsNullOrEmpty(downloaderUserAgent))
						SetDownloaderUserAgent();
					return downloaderUserAgent;
				}
				//set { _downloaderUserAgent = value; }
			}

			/// <summary>
			/// The Uri to download the file containing the index and content of the categories and blacklisted sites.
			/// </summary>
			public System.Uri DownloadUri
			{
				get
				{
					if(downloadUri == null)
						SetDownloadUri();
					return downloadUri;
				}
				//set { _downloadUri = value; }
			}

			/// <summary>
			/// the "data" and "logs" parent directorie, this can be set at the app.config file.
			/// </summary>
			public DirectoryInfo WorkingDirectory
			{
				get
				{
					if(workingDirectory == null)
					{
						SetWorkingDirectory();
					}
					return workingDirectory;
				}
				//set { }
			}

			/// <summary>
			/// The "data" directory, where to donaload, decompress and convert to XML format the index and content of the categories and blacklisted sites.
			/// </summary>
			public DirectoryInfo DataDirectory
			{
				get
				{
					if(dataDirectory == null)
						SetWorkingSubDirectory(out dataDirectory, "data");
					return dataDirectory;
				}
				//set { _dataDirectory = value; }
			}

			/// <summary>
			/// Directory to process the log files, errors.xml, status.xml.
			/// </summary>
			public DirectoryInfo LogsDirectory
			{
				get
				{
					if(logsDirectory == null)
						SetWorkingSubDirectory(out logsDirectory, "logs");
					return logsDirectory;
				}
				//set { _logsDirectory = value; }
			}

			/// <summary>
			/// Interval to check for updates of the source file containing the index and content of categories and blacklisted sites.
			/// </summary>
			public TimeSpan CheckForUpdateInterval
			{
				get
				{
					if(checkForUpdateInterval.TotalSeconds == 0)
						SetCheckForUpdateInterval();
					return checkForUpdateInterval;
				}
				//set { _checkForUpdateInterval = value; }
			}

			/// <summary>
			/// Current operation in execution.
			/// </summary>
			public ISACFMgr.operationList CurrentOperation
			{
				get
				{
					// Get the currentOperation from the statusLogFile and delete the code bellow.
					if(currentOperation == 0)
						currentOperation = Status.GetCurrentOperation();

					return currentOperation;
				}
				set
				{
					currentOperation = value;
					Status.SetStatusValue(Status.Fields.CurrentOperationName, currentOperation.ToString());
					// Notify the status change in the EventLog. 
					LogProcessing.WriteToEventLog(string.Format("El estado de operaciones ha cambiado a: {0}", currentOperation), EventLogEntryType.Information);
					// Try to free some memory every time the current operation changes.
					GC.Collect();
				}
			}

			public DateTime DownloadedFileLastModifiedDate
			{
				get
				{
					// Get the downloadedFileLastModifiedDate from the statusLogFile and delete the code bellow.
					if(downloadedFileLastModifiedDate == DateTime.MinValue)
						downloadedFileLastModifiedDate = Status.GetDownloadedFileLastModifiedDate();

					return downloadedFileLastModifiedDate;
				}
				set
				{
					downloadedFileLastModifiedDate = value;
					Status.SetStatusValue(Status.Fields.DownloadedFileLastModifiedDate, downloadedFileLastModifiedDate.ToString("o"));
					// Write code to notify the status change in the EventLog.
					// clsLogProcessing.WriteToEventLog(string.Format("Se ha almacenado el valor fecha: {0} en el fichero status.xml", _downloadedFileLastModifiedDate.ToString("o")), EventLogEntryType.Information);
					// Write code to save the actual state in logFile to keep constance of the current operation.
				}
			}

			public string DownloadedFileETag
			{
				get
				{
					// Get the downloadedFileETag from the status.xml Log File.
					if(string.IsNullOrEmpty(downloadedFileETag))
						downloadedFileETag = Status.GetDownloadedFileETag();

					return downloadedFileETag;
				}
				set
				{
					downloadedFileETag = value.Replace("\"", string.Empty);
					Status.SetStatusValue(Status.Fields.DownloadedFileETag, downloadedFileETag);
				}
			}

			public string DownloadedFileName
			{
				get
				{
					// Get the downloadedFileName from the status.xml Log File.
					if(string.IsNullOrEmpty(downloadedFileName))
						downloadedFileName = Status.GetDownloadedFileName();

					return downloadedFileName;
				}
				set
				{
					downloadedFileName = value;
					Status.SetStatusValue(Status.Fields.DownloadedFileName, downloadedFileName);
				}
			}

			public bool UseSqlTransaction
			{
				get
				{
					if(useSqlTransaction == null)
					{
						SetUseSqlTransaction();
					}
					return (bool)useSqlTransaction;
				}
				//set {_useTransactionIfPossible = value;}
			}

			public bool ProcessISARuleElements
			{
				get
				{
					if(processISARuleElements == null)
					{
						processISARuleElements = GetProcessISARuleElements();
					}
					return (bool)processISARuleElements;
				}
				//set {_processISARuleElements = value;}
			}

			public StruISAServer ISAServer;

		}

		internal struct StruISAServer
		{
			public string Name
			{
				get
				{
					if(string.IsNullOrEmpty(ISAServerName))
						SetISAServerName();
					return ISAServerName;
				}
			}
			public string UserName
			{
				get
				{
					if(string.IsNullOrEmpty(ISAServerUserName))
						ISAServerUserName = Properties.Settings.Default.ISAServerUserName;
					return ISAServerUserName;
				}
			}
			public string UserDomain
			{
				get
				{
					if(string.IsNullOrEmpty(ISAServerUserDomain))
						SetISAServerUserDomain();
					return ISAServerUserDomain;
				}
			}
			public string UserPasswd
			{
				get
				{
					if(string.IsNullOrEmpty(ISAServerUserPasswd))
						SetISAServerUserPasswd();
					return ISAServerUserPasswd;
				}
			}

		}

		internal static StruServiceSettings serviceSettings;
		internal static void SetServiceSettings()
		{

			Properties.Settings.Default.Reload();

			// CONFIGURATION: UserAgent (used for any nedded requests).
			SetDownloaderUserAgent();

			// CONFIGURATION: downloadUri (Uri resource to download).
			SetDownloadUri();

			// CONFIGURATION: dataDirectory (Path to process the downloaded file(s)).
			SetWorkingDirectory();

			// CONFIGURATION: dataDirectory (Path to process the downloaded file(s)).
			SetWorkingSubDirectory(out dataDirectory, "data");

			// CONFIGURATION: logsDirectory (Path to process the logs file(s)).
			SetWorkingSubDirectory(out logsDirectory, "logs");

			// CONFIGURATION: checkForUpdateInterval
			SetCheckForUpdateInterval();

			// CONFIGURATION: useSqlTransaction
			SetUseSqlTransaction();

			// VALORES PARA CONEXIÓN CON EL SERVIDOR ISA.
			// CONFIGURATION: ISAServerName
			SetISAServerName();

			// CONFIGURATION: ISAServerUserName
			SetISAServerUserName();

			// CONFIGURATION: ISAServerUserPasswd
			SetISAServerUserPasswd();

			// CONFIGURATION: ISAServerUserDomain
			SetISAServerUserDomain();

		} // Fin de setServiceSettings()


		/// <summary>
		/// Set the User-Agent value passed when requesting Internet resources.
		/// </summary>
		private static void SetDownloaderUserAgent()
		{
			downloaderUserAgent = string.IsNullOrEmpty(Properties.Settings.Default.downloaderUserAgent) ? "ISA Categorization & Content Filter Manager/1.0" : Properties.Settings.Default.downloaderUserAgent;
		}

		/// <summary>
		/// Set the TimeSpan value for retry interval when requesting Internet resources.
		/// </summary>
		private static void SetCheckForUpdateInterval()
		{

			// Set the value of checkForUpdateInterval to the default 12 hours.
			checkForUpdateInterval = new TimeSpan(12, 00, 00);

			try
			{
				// Check if not is null or empty the value of the parameter.
				if(!string.IsNullOrEmpty(Properties.Settings.Default.checkForUpdateInterval))
				{
					string[] updateInterval;
					updateInterval = Properties.Settings.Default.checkForUpdateInterval.Split(':');

					// Check the value is in the spected format.
					if(updateInterval.Length == 3)
					{
						int[] interval = new int[3];
						// Convert the values in the correct type.
						for(int i = 0; i <= updateInterval.Length - 1; i++)
						{
							System.Int32.TryParse(updateInterval[i], out interval[i]);
						}
						checkForUpdateInterval = new TimeSpan(interval[0], interval[1], interval[2]);
					}
					else // If there are not three element in the interval 
					{
						// Write a warning in the EventLog.
						string strMsg = string.Format("El servicio \"{0}\" requiere un intervalo correcto (horas:min:seg) como valor " +
																					"del parámetro de configuración \"checkForUpdateInterval\" del fichero de " +
																					"configuración \"app.config\" que debe estar ubicado en \"{1}\". El sistema " +
																					"ha elegido funcionar con el valor \"12:00:00\" para este parámetro, lo cual " +
																					"representa un intervalo aproximado de 12 horas, con cero minutos y cero segundos.",
																					ServiceSettings.serviceSettings.ServiceName, AppDomain.CurrentDomain.BaseDirectory);
						LogProcessing.WriteToEventLog(strMsg, System.Diagnostics.EventLogEntryType.Warning);
					}
					// Validate the interval and ensere a not too short one or generate a warning.
					if(checkForUpdateInterval.TotalMinutes <= 5)
					{
						// Set the value of checkForUpdateInterval to 12 hours.
						checkForUpdateInterval = new TimeSpan(12, 00, 00);
						// Write a warning in the EventLog.
						string strMsg = string.Format("El servicio \"{0}\" requiere un intervalo correcto como valor " +
																					"del parámetro de configuración \"checkForUpdateInterval\" del fichero de " +
																					"configuración \"app.config\" que debe estar ubicado en \"{1}\". El sistema " +
																					"ha elegido tomar como valor \"12:00:00\" para este parámetro, lo cual " +
																					"representa un intervalo aproximado de 12 horas, con cero minutos y cero segundos.",
																					ServiceSettings.serviceSettings.ServiceName, AppDomain.CurrentDomain.BaseDirectory);
						LogProcessing.WriteToEventLog(strMsg, System.Diagnostics.EventLogEntryType.Warning);
					}
					else if(checkForUpdateInterval.TotalMinutes < 60)
					{
						// Write a warning in the EventLog, the interval is too short.
						string strMsg = string.Format("El servicio \"{0}\" le sugiere que aumente el intervalo que se " +
																					"establece a través de la propiedad \"checkForUpdateInterval\" del " +
																					"fichero de configuración \"app.config\" que debe estar ubicado en " +
																					"\"{1}\". Actualmente este valor está establecido a \"{2}\" minutos.",
																					ServiceSettings.serviceSettings.ServiceName, AppDomain.CurrentDomain.BaseDirectory, checkForUpdateInterval.TotalMinutes);
						LogProcessing.WriteToEventLog(strMsg, System.Diagnostics.EventLogEntryType.Warning);
					}
				}
				else
				{
					// Write a warning in the EventLog.
					string strMsg = string.Format("El servicio \"{0}\" requiere un intervalo correcto como valor " +
																				"del parámetro de configuración \"checkForUpdateInterval\" del fichero de " +
																				"configuración \"app.config\" que debe estar ubicado en \"{1}\". El sistema " +
																				"ha elegido tomar como valor \"12:00:00\" para este parámetro, lo cual " +
																				"un intervalo aproximado de 12 horas, con cero minutos y cero segundos.",
																				ServiceSettings.serviceSettings.ServiceName, AppDomain.CurrentDomain.BaseDirectory);
					LogProcessing.WriteToEventLog(strMsg, System.Diagnostics.EventLogEntryType.Warning);
				}
			}
			catch(Exception exUpdateInterval)
			{
				// Write a warning in the EventLog.
				string strMsg = string.Format("ERROR: {0} El servicio \"{1}\" requiere un valor de intervalo correcto (horas:min:seg) " +
																			"para la configuración \"checkForUpdateInterval\" en el fichero de configuración " +
																			"\"app.config\" que debe estar ubicado en \"{2}\". El sistema ha elegido tomar como " +
																			"valor \"12:00:00\" para este parámetro, lo cual un intervalo aproximado de 12 horas, " +
																			"con cero minutos y cero segundos.",
																			exUpdateInterval.Message, ServiceSettings.serviceSettings.ServiceName, AppDomain.CurrentDomain.BaseDirectory);
				LogProcessing.WriteToEventLog(strMsg, System.Diagnostics.EventLogEntryType.Warning);
			}
		}

		private static void SetDownloadUri()
		{
			// Set the default Uri.
			downloadUri = new System.Uri("http://www.shallalist.de/Downloads/shallalist.tar.gz");
			try
			{
				// Get it from app.config if exist.
				if(!string.IsNullOrEmpty(Properties.Settings.Default.downloadUri))
					downloadUri = new System.Uri(Properties.Settings.Default.downloadUri);
			}
			catch(System.Exception Ex)
			{
				// Write an Error in the EventLog.
				string strMsg = string.Format("Ha ocurrido un error inesperado al procesar el valor del elemento de configuración " +
					"\"downloadUri\" del fichero \"app.config\". Como valor para este parámetro se ha tomado \"{0}\". Los detalles " +
					"del error se muestran a continucaión: ", downloadUri.ToString(), Ex.Message);
				LogProcessing.WriteToEventLog(strMsg, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private static void SetWorkingDirectory()
		{
			workingDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

			if(!string.IsNullOrEmpty(Properties.Settings.Default.workingDirFullPath))
			{
				try
				{
					workingDirectory = new DirectoryInfo(Properties.Settings.Default.workingDirFullPath);
					if(!workingDirectory.Exists)
						workingDirectory.Create();
					return;
				}
				catch(System.Exception Ex)
				{
					string strMsg = string.Format("Ha ocurrido un error inesperado mientras se determinaba el valor del " +
						"elemento de configuración \"workingDirFullPath\". Como valor para este parámetro se ha determinado " +
						"tomar \"{0}\". Los detalles del error se muestran a continuación: ", workingDirectory.FullName, Ex.Message);
					LogProcessing.WriteToEventLog(strMsg, System.Diagnostics.EventLogEntryType.Error);
				}
			}

			if(!workingDirectory.FullName.EndsWith(Path.DirectorySeparatorChar.ToString()))
				workingDirectory = new DirectoryInfo(string.Format("{0}{1}", workingDirectory.FullName, Path.DirectorySeparatorChar.ToString()));

		}

		private static void SetWorkingSubDirectory(out DirectoryInfo DirInfo, string subDirName)
		{
			DirInfo = new DirectoryInfo(string.Format("{0}{1}", ServiceSettings.serviceSettings.WorkingDirectory, subDirName));
			if(!DirInfo.Exists)
			{
				DirInfo.Create();
			}

			// Add the final backslash if needed.
			if(!DirInfo.FullName.EndsWith(Path.DirectorySeparatorChar.ToString()))
				DirInfo = new DirectoryInfo(string.Format("{0}{1}", DirInfo.FullName, Path.DirectorySeparatorChar.ToString()));

		}

		/// <summary>
		/// Get the formated DateTime value for prefixing the file to download to the local file system.
		/// </summary>
		/// <param name="dateForPrefix">DateTime value to convert to specific string.</param>
		/// <returns>String in the form yearmonthday</returns>
		public static string GetPrefixForFileNameFromDate(DateTime dateForPrefix)
		{
			return dateForPrefix.ToString("u").Substring(0, 10).Replace("-", "");
		}

		private static bool SetUseSqlTransaction()
		{
			useSqlTransaction = Properties.Settings.Default.useSqlTransaction;

			if((bool)useSqlTransaction)
			{
				try
				{
					Decimal RAMRequired = 2000.00M; // 2GB, at least, required to do SQL Transactions.
					ComputerInfo compInfo = new ComputerInfo();
					Decimal TotalPhysicalMemoryMB = Decimal.Round(((Decimal)compInfo.TotalPhysicalMemory / 1048576), 2); // Convert to ~MB.
					if(TotalPhysicalMemoryMB >= RAMRequired) // Solo se efectuarán transacciones si el equipo cuenta con 2GB o más de RAM.
						useSqlTransaction = true;
					else
					{
						string Message = string.Format("Solo se efectuarán las actualizaciones de datos en modo de transacción en aquellos " +
						"equipos que cuenten con {0} MB de memoria RAM o más. Si no desea ver nuevamente este mensaje cambie el valor de " +
						"configuración \"useSqlTransaction\" de \"True\" a \"False\" ó haga una actualización de la memoria RAM de su equipo " +
						"incrementando esta de {1} MB (valor actual) a al menos {0} MB.", RAMRequired, TotalPhysicalMemoryMB);
						LogProcessing.WriteToEventLog(Message, EventLogEntryType.Warning);
						useSqlTransaction = false;
					}
				}
				catch(System.ComponentModel.Win32Exception ExWin32) // The application cannot obtain the memory status.
				{
					string Message = string.Format("El sistema no pudo obtener el valor total de la memoria física (RAM) de este esquipo " +
						"para decidir si es apropiado hacer los procesos de actualizaciones de datos en modo de transacción, por tanto se ha " +
						"determinado que no se utilizarán transacciones para ello. " + Environment.NewLine + "Detalles del error: {0}", ExWin32.Message);
					LogProcessing.WriteToEventLog(Message, EventLogEntryType.Warning);
					useSqlTransaction = false;
				}
				catch(System.Exception Ex)
				{
					string Message = string.Format("Ha ocurrido un error inesperado mientras el sistema intentaba obtener el valor total de " +
						"la memoria física (RAM) de este esquipo para decidir si es apropiado hacer los procesos de actualizaciones de datos " +
						"en modo de transacción, por tanto se ha determinado que no se utilizarán transacciones para ello. " + Environment.NewLine +
						"Detalles del error: {0}", Ex.Message);
					LogProcessing.WriteToEventLog(Message, EventLogEntryType.Warning);
					useSqlTransaction = false;
				}
			}

			return (bool)useSqlTransaction;
		}

		private static bool GetProcessISARuleElements()
		{
			processISARuleElements = Properties.Settings.Default.processISARuleElements;
			return (bool)processISARuleElements;
		}

		private static void SetISAServerName()
		{
			// If processISARuleElements==false then this value is not needed.
			if(!ServiceSettings.serviceSettings.ProcessISARuleElements)
				return;

			ISAServerName = Properties.Settings.Default.ISAServerName;
			if(string.IsNullOrEmpty(ISAServerName))
			{
				try
				{
					if(Process.GetProcessesByName("isastg").Length > 0) // Check if exists "Microsoft ISA Server Storage" process.
					{
						ISAServerName = Environment.MachineName;
						string Message = string.Format("No hay valor para el elemento \"ISAServerName\" del fichero de configuración " +
							"\"app.config\", el sistema ha encontrado que en este equipo está instalado el proceso \"Microsoft ISA Server Storage\" " +
							"por tanto se ha tomado como nombre del servidor ISA el siguiente: {0}", ISAServerName);
						LogProcessing.WriteToEventLog(Message, EventLogEntryType.Warning);
					}
					else
					{
						ISAServerName = string.Empty;
						string Message = string.Format("No hay valor para el elemento \"ISAServerName\" del fichero de configuración " +
							"\"app.config\", el sistema comprobó si en el equipo local existía el proceso \"Microsoft ISA Server Storage\" " +
							"pero no lo encontró, por tanto no se pudo determinar qué valor asignar a este parámetro, como consecuencia " +
							"no se procesarán los RuleElements del servidor ISA.");
						LogProcessing.WriteToEventLog(Message, EventLogEntryType.Warning);
					}
				}
				catch(System.Exception Ex)
				{
					string Message = string.Format("No hay valor para el elemento \"ISAServerName\" del fichero de configuración " +
						"\"app.config\", el sistema ha intentado, sin éxito, determinar si este equipo ejecuta el proceso \"Microsoft " +
						"ISA Server Storage\" lo cual ha provocado un error inesperado. Sin este valor el sistema no podrá procesar " +
						"los elementos de configuración del servidor ISA Server. Los detalles del error se muestran a continuación: {0}", Ex.Message);
					LogProcessing.WriteToEventLog(Message, EventLogEntryType.Error);
				}
			}
		}

		private static void SetISAServerUserName()
		{
			// If processISARuleElements==false then this value is not needed.
			if(!ServiceSettings.serviceSettings.ProcessISARuleElements)
				return;

			ISAServerUserName = Properties.Settings.Default.ISAServerUserName;
			if(string.IsNullOrEmpty(ISAServerUserName))
			{
				try
				{
					ISAServerUserName = Process.GetCurrentProcess().StartInfo.UserName;
					string Message = string.Format("No hay valor para el elemento \"ISAServerUserName\" del fichero de configuración " +
						"\"app.config\", el sistema ha encontrado que se utiliza el usuario \"{0}\" para ejecutar este servicio y ha " +
						"determinado que también se empleará para conectarse al servidir ISA.", ISAServerUserName);
					LogProcessing.WriteToEventLog(Message, EventLogEntryType.Warning);
				}
				catch(System.Exception Ex)
				{
					string Message = string.Format("No hay valor para el elemento \"ISAServerUserName\" del fichero de configuración " +
						"\"app.config\", y ha ocurrido un error inesperado mientras se intentaba determinar el usuario que se emplea para " +
						"la ejecución de este servicio. Los detalles del error se muestran a continuación: {0}", Ex.Message);
					LogProcessing.WriteToEventLog(Message, EventLogEntryType.Error);
				}
			}
		}

		private static void SetISAServerUserPasswd()
		{
			// If processISARuleElements==false then this value is not needed.
			if(!ServiceSettings.serviceSettings.ProcessISARuleElements)
				return;

			ISAServerUserPasswd = Properties.Settings.Default.ISAServerUserPasswd;
			if(string.IsNullOrEmpty(ISAServerUserPasswd))
			{
				try
				{
					string Message = string.Empty;
					if(!string.IsNullOrEmpty(ISAServerUserName) && ISAServerUserName == Process.GetCurrentProcess().StartInfo.UserName)
					{
						ISAServerUserPasswd = Process.GetCurrentProcess().StartInfo.Password.ToString();
						Message = string.Format("No hay valor para el elemento \"ISAServerUserPasswd\" del fichero de configuración " +
							"\"app.config\", el sistema ha determinado utilizar la contraseña perteneciente al usuario \"{0}\", pero puede " +
							"este procedimiento no de los resultados esperados, se le recomienda que revise los valores de configuración " +
							"para este servicio que se encuentran ubicados en el fichero antes mencionado.", ServiceSettings.serviceSettings.ISAServer.UserName);
					}
					else
					{
						ISAServerUserPasswd = null;
						Message = string.Format("No hay valor para el elemento \"ISAServerUserPasswd\" del fichero de configuración " +
							"\"app.config\", el sistema no ha podido establecer un valor para este elemento lo cual podrá traer como " +
							"consecuencia que el servicio no pueda modificar los RuleElements en el servidor ISA. Se le recomienda que " +
							"revise los valores de configuración para este servicio que se encuentran ubicados en el fichero antes mencionado.");
					}
					LogProcessing.WriteToEventLog(Message, EventLogEntryType.Warning);
				}
				catch(System.Exception Ex)
				{
					string Message = string.Format("No hay valor para el elemento \"ISAServerUserPasswd\" del fichero de configuración " +
						"\"app.config\", y ha ocurrido un error inesperado mientras se intentaba determinar la contraseña utilizada por el " +
						"usuario \"{0}\". Los detalles del error se muestran a continuación: {1}", ServiceSettings.serviceSettings.ISAServer.UserName, Ex.Message);
					LogProcessing.WriteToEventLog(Message, EventLogEntryType.Warning);
				}
			}
		}

		private static void SetISAServerUserDomain()
		{
			// If processISARuleElements==false then this value is not needed.
			if(!ServiceSettings.serviceSettings.ProcessISARuleElements)
				return;

			ISAServerUserDomain = string.IsNullOrEmpty(Properties.Settings.Default.ISAServerUserDomain) ? ServiceSettings.serviceSettings.ISAServer.Name : Properties.Settings.Default.ISAServerUserDomain;
		}

	} // Fin de la clase.

} //Fin del namespace.
