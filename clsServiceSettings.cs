using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;

namespace LAMSoft.ISACFMgr
{
	internal class clsSettings
	{
		private static string _serviceName;
		private static string _downloaderUserAgent;
		private static System.Uri _downloadUri;
		private static DirectoryInfo _workingDirectory;
		private static DirectoryInfo _dataDirectory;
		private static DirectoryInfo _logsDirectory;
		private static TimeSpan _checkForUpdateInterval;
		private static ISACFMgr.operationList _currentOperation;
		private static DateTime _downloadedFileLastModifiedDate;
		private static string _downloadedFileETag;
		private static string _downloadedFileName;
		private static bool? _useSqlTransaction = null;
		private static bool? _processISARuleElements = null;
		// Fields for struct "struISAServer"
		private static string _ISAServerName = null;
		private static string _ISAServerUserName = null;
		private static string _ISAServerUserDomain = null;
		private static string _ISAServerUserPasswd = null;

		/// <summary>
		/// Service settings structure.
		/// </summary>
		internal struct struServiceSettings
		{
			/// <summary>
			/// The service name, obtained from the class ISACFMgr.
			/// </summary>
			public string serviceName
			{
				get
				{
					if(string.IsNullOrEmpty(_serviceName))
					{
						ISACFMgr s = new ISACFMgr();
						_serviceName = s.ServiceName;
					}
					return _serviceName;
				}
				//set { _serviceName = value; }
			}

			/// <summary>
			/// User-Agent string used to request the downloads as nedded.
			/// </summary>
			public string downloaderUserAgent
			{
				get
				{
					if(string.IsNullOrEmpty(_downloaderUserAgent))
						setDownloaderUserAgent();
					return _downloaderUserAgent;
				}
				//set { _downloaderUserAgent = value; }
			}

			/// <summary>
			/// The Uri to download the file containing the index and content of the categories and blacklisted sites.
			/// </summary>
			public System.Uri downloadUri
			{
				get
				{
					if(_downloadUri == null)
						setDownloadUri();
					return _downloadUri;
				}
				//set { _downloadUri = value; }
			}

			/// <summary>
			/// the "data" and "logs" parent directorie, this can be set at the app.config file.
			/// </summary>
			public DirectoryInfo workingDirectory
			{
				get
				{
					if(_workingDirectory == null)
					{
						setWorkingDirectory();
					}
					return _workingDirectory;
				}
				//set { }
			}

			/// <summary>
			/// The "data" directory, where to donaload, decompress and convert to XML format the index and content of the categories and blacklisted sites.
			/// </summary>
			public DirectoryInfo dataDirectory
			{
				get
				{
					if(_dataDirectory == null)
						setWorkingSubDirectory(out _dataDirectory, "data");
					return _dataDirectory;
				}
				//set { _dataDirectory = value; }
			}

			/// <summary>
			/// Directory to process the log files, errors.xml, status.xml.
			/// </summary>
			public DirectoryInfo logsDirectory
			{
				get
				{
					if(_logsDirectory == null)
						setWorkingSubDirectory(out _logsDirectory, "logs");
					return _logsDirectory;
				}
				//set { _logsDirectory = value; }
			}

			/// <summary>
			/// Interval to check for updates of the source file containing the index and content of categories and blacklisted sites.
			/// </summary>
			public TimeSpan checkForUpdateInterval
			{
				get
				{
					if(_checkForUpdateInterval.TotalSeconds == 0)
						setCheckForUpdateInterval();
					return _checkForUpdateInterval;
				}
				//set { _checkForUpdateInterval = value; }
			}

			/// <summary>
			/// Current operation in execution.
			/// </summary>
			public ISACFMgr.operationList currentOperation
			{
				get
				{
					// Get the currentOperation from the statusLogFile and delete the code bellow.
					if(_currentOperation == 0)
						_currentOperation = clsStatus.getCurrentOperation();

					return _currentOperation;
				}
				set
				{
					_currentOperation = value;
					clsStatus.setStatusValue(clsStatus.statusFields.currentOperationName, _currentOperation.ToString());
					// Notify the status change in the EventLog. 
					clsLogProcessing.WriteToEventLog(string.Format("El estado de operaciones ha cambiado a: {0}", _currentOperation), EventLogEntryType.Information);
					// Try to free some memory every time the current operation changes.
					GC.Collect();
				}
			}

			public DateTime downloadedFileLastModifiedDate
			{
				get
				{
					// Get the downloadedFileLastModifiedDate from the statusLogFile and delete the code bellow.
					if(_downloadedFileLastModifiedDate == DateTime.MinValue)
						_downloadedFileLastModifiedDate = clsStatus.getDownloadedFileLastModifiedDate();

					return _downloadedFileLastModifiedDate;
				}
				set
				{
					_downloadedFileLastModifiedDate = value;
					clsStatus.setStatusValue(clsStatus.statusFields.downloadedFileLastModifiedDate, _downloadedFileLastModifiedDate.ToString("o"));
					// Write code to notify the status change in the EventLog.
					// clsLogProcessing.WriteToEventLog(string.Format("Se ha almacenado el valor fecha: {0} en el fichero status.xml", _downloadedFileLastModifiedDate.ToString("o")), EventLogEntryType.Information);
					// Write code to save the actual state in logFile to keep constance of the current operation.
				}
			}

			public string downloadedFileETag
			{
				get
				{
					// Get the downloadedFileETag from the status.xml Log File.
					if(string.IsNullOrEmpty(_downloadedFileETag))
						_downloadedFileETag = clsStatus.getDownloadedFileETag();

					return _downloadedFileETag;
				}
				set
				{
					_downloadedFileETag = value.Replace("\"", string.Empty);
					clsStatus.setStatusValue(clsStatus.statusFields.downloadedFileETag, _downloadedFileETag);
				}
			}

			public string downloadedFileName
			{
				get
				{
					// Get the downloadedFileName from the status.xml Log File.
					if(string.IsNullOrEmpty(_downloadedFileName))
						_downloadedFileName = clsStatus.getDownloadedFileName();

					return _downloadedFileName;
				}
				set
				{
					_downloadedFileName = value;
					clsStatus.setStatusValue(clsStatus.statusFields.downloadedFileName, _downloadedFileName);
				}
			}

			public bool useSqlTransaction
			{
				get
				{
					if(_useSqlTransaction == null)
					{
						setUseSqlTransaction();
					}
					return (bool)_useSqlTransaction;
				}
				//set {_useTransactionIfPossible = value;}
			}

			public bool processISARuleElements
			{
				get
				{
					if(_processISARuleElements == null)
					{
						_processISARuleElements = getProcessISARuleElements();
					}
					return (bool)_processISARuleElements;
				}
				//set {_processISARuleElements = value;}
			}

			public struISAServer ISAServer;

		}

		internal struct struISAServer
		{
			public string Name
			{
				get
				{
					if(string.IsNullOrEmpty(_ISAServerName))
						setISAServerName();
					return _ISAServerName;
				}
			}
			public string UserName
			{
				get
				{
					if(string.IsNullOrEmpty(_ISAServerUserName))
						_ISAServerUserName = Properties.Settings.Default.ISAServerUserName;
					return _ISAServerUserName;
				}
			}
			public string UserDomain
			{
				get
				{
					if(string.IsNullOrEmpty(_ISAServerUserDomain))
						setISAServerUserDomain();
					return _ISAServerUserDomain;
				}
			}
			public string UserPasswd
			{
				get
				{
					if(string.IsNullOrEmpty(_ISAServerUserPasswd))
						setISAServerUserPasswd();
					return _ISAServerUserPasswd;
				}
			}

		}

		internal static struServiceSettings serviceSettings;
		internal static void setServiceSettings()
		{

			Properties.Settings.Default.Reload();

			// CONFIGURATION: UserAgent (used for any nedded requests).
			setDownloaderUserAgent();

			// CONFIGURATION: downloadUri (Uri resource to download).
			setDownloadUri();

			// CONFIGURATION: dataDirectory (Path to process the downloaded file(s)).
			setWorkingDirectory();

			// CONFIGURATION: dataDirectory (Path to process the downloaded file(s)).
			setWorkingSubDirectory(out _dataDirectory, "data");

			// CONFIGURATION: logsDirectory (Path to process the logs file(s)).
			setWorkingSubDirectory(out _logsDirectory, "logs");

			// CONFIGURATION: checkForUpdateInterval
			setCheckForUpdateInterval();

			// CONFIGURATION: useSqlTransaction
			setUseSqlTransaction();

			// VALORES PARA CONEXIÓN CON EL SERVIDOR ISA.
			// CONFIGURATION: ISAServerName
			setISAServerName();

			// CONFIGURATION: ISAServerUserName
			setISAServerUserName();

			// CONFIGURATION: ISAServerUserPasswd
			setISAServerUserPasswd();

			// CONFIGURATION: ISAServerUserDomain
			setISAServerUserDomain();

		} // Fin de setServiceSettings()


		/// <summary>
		/// Set the User-Agent value passed when requesting Internet resources.
		/// </summary>
		private static void setDownloaderUserAgent()
		{
			_downloaderUserAgent = string.IsNullOrEmpty(Properties.Settings.Default.downloaderUserAgent) ? "ISA Categorization & Content Filter Manager/1.0" : Properties.Settings.Default.downloaderUserAgent;
		}

		/// <summary>
		/// Set the TimeSpan value for retry interval when requesting Internet resources.
		/// </summary>
		private static void setCheckForUpdateInterval()
		{

			// Set the value of checkForUpdateInterval to the default 12 hours.
			_checkForUpdateInterval = new TimeSpan(12, 00, 00);

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
						_checkForUpdateInterval = new TimeSpan(interval[0], interval[1], interval[2]);
					}
					else // If there are not three element in the interval 
					{
						// Write a warning in the EventLog.
						string strMsg = string.Format("El servicio \"{0}\" requiere un intervalo correcto (horas:min:seg) como valor " +
																					"del parámetro de configuración \"checkForUpdateInterval\" del fichero de " +
																					"configuración \"app.config\" que debe estar ubicado en \"{1}\". El sistema " +
																					"ha elegido funcionar con el valor \"12:00:00\" para este parámetro, lo cual " +
																					"representa un intervalo aproximado de 12 horas, con cero minutos y cero segundos.",
																					clsSettings.serviceSettings.serviceName, AppDomain.CurrentDomain.BaseDirectory);
						clsLogProcessing.WriteToEventLog(strMsg, System.Diagnostics.EventLogEntryType.Warning);
					}
					// Validate the interval and ensere a not too short one or generate a warning.
					if(_checkForUpdateInterval.TotalMinutes <= 5)
					{
						// Set the value of checkForUpdateInterval to 12 hours.
						_checkForUpdateInterval = new TimeSpan(12, 00, 00);
						// Write a warning in the EventLog.
						string strMsg = string.Format("El servicio \"{0}\" requiere un intervalo correcto como valor " +
																					"del parámetro de configuración \"checkForUpdateInterval\" del fichero de " +
																					"configuración \"app.config\" que debe estar ubicado en \"{1}\". El sistema " +
																					"ha elegido tomar como valor \"12:00:00\" para este parámetro, lo cual " +
																					"representa un intervalo aproximado de 12 horas, con cero minutos y cero segundos.",
																					clsSettings.serviceSettings.serviceName, AppDomain.CurrentDomain.BaseDirectory);
						clsLogProcessing.WriteToEventLog(strMsg, System.Diagnostics.EventLogEntryType.Warning);
					}
					else if(_checkForUpdateInterval.TotalMinutes < 60)
					{
						// Write a warning in the EventLog, the interval is too short.
						string strMsg = string.Format("El servicio \"{0}\" le sugiere que aumente el intervalo que se " +
																					"establece a través de la propiedad \"checkForUpdateInterval\" del " +
																					"fichero de configuración \"app.config\" que debe estar ubicado en " +
																					"\"{1}\". Actualmente este valor está establecido a \"{2}\" minutos.",
																					clsSettings.serviceSettings.serviceName, AppDomain.CurrentDomain.BaseDirectory, _checkForUpdateInterval.TotalMinutes);
						clsLogProcessing.WriteToEventLog(strMsg, System.Diagnostics.EventLogEntryType.Warning);
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
																				clsSettings.serviceSettings.serviceName, AppDomain.CurrentDomain.BaseDirectory);
					clsLogProcessing.WriteToEventLog(strMsg, System.Diagnostics.EventLogEntryType.Warning);
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
																			exUpdateInterval.Message, clsSettings.serviceSettings.serviceName, AppDomain.CurrentDomain.BaseDirectory);
				clsLogProcessing.WriteToEventLog(strMsg, System.Diagnostics.EventLogEntryType.Warning);
			}
		}

		private static void setDownloadUri()
		{
			// Set the default Uri.
			_downloadUri = new System.Uri("http://www.shallalist.de/Downloads/shallalist.tar.gz");
			try
			{
				// Get it from app.config if exist.
				if(!string.IsNullOrEmpty(Properties.Settings.Default.downloadUri))
					_downloadUri = new System.Uri(Properties.Settings.Default.downloadUri);
			}
			catch(System.Exception Ex)
			{
				// Write an Error in the EventLog.
				string strMsg = string.Format("Ha ocurrido un error inesperado al procesar el valor del elemento de configuración " +
					"\"downloadUri\" del fichero \"app.config\". Como valor para este parámetro se ha tomado \"{0}\". Los detalles " +
					"del error se muestran a continucaión: ", _downloadUri.ToString(), Ex.Message);
				clsLogProcessing.WriteToEventLog(strMsg, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private static void setWorkingDirectory()
		{
			_workingDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

			if(!string.IsNullOrEmpty(Properties.Settings.Default.workingDirFullPath))
			{
				try
				{
					_workingDirectory = new DirectoryInfo(Properties.Settings.Default.workingDirFullPath);
					if(!_workingDirectory.Exists)
						_workingDirectory.Create();
					return;
				}
				catch(System.Exception Ex)
				{
					string strMsg = string.Format("Ha ocurrido un error inesperado mientras se determinaba el valor del " +
						"elemento de configuración \"workingDirFullPath\". Como valor para este parámetro se ha determinado " +
						"tomar \"{0}\". Los detalles del error se muestran a continuación: ", _workingDirectory.FullName, Ex.Message);
					clsLogProcessing.WriteToEventLog(strMsg, System.Diagnostics.EventLogEntryType.Error);
				}
			}

			if(!_workingDirectory.FullName.EndsWith(Path.DirectorySeparatorChar.ToString()))
				_workingDirectory = new DirectoryInfo(string.Format("{0}{1}", _workingDirectory.FullName, Path.DirectorySeparatorChar.ToString()));

		}

		private static void setWorkingSubDirectory(out DirectoryInfo DirInfo, string subDirName)
		{
			DirInfo = new DirectoryInfo(string.Format("{0}{1}", clsSettings.serviceSettings.workingDirectory, subDirName));
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
		public static string getPrefixForFileNameFromDate(DateTime dateForPrefix)
		{
			return dateForPrefix.ToString("u").Substring(0, 10).Replace("-", "");
		}

		private static bool setUseSqlTransaction()
		{
			_useSqlTransaction = Properties.Settings.Default.useSqlTransaction;

			if((bool)_useSqlTransaction)
			{
				try
				{
					Decimal RAMRequired = 2000.00M; // 2GB, at least, required to do SQL Transactions.
					ComputerInfo compInfo = new ComputerInfo();
					Decimal TotalPhysicalMemoryMB = Decimal.Round(((Decimal)compInfo.TotalPhysicalMemory / 1048576), 2); // Convert to ~MB.
					if(TotalPhysicalMemoryMB >= RAMRequired) // Solo se efectuarán transacciones si el equipo cuenta con 2GB o más de RAM.
						_useSqlTransaction = true;
					else
					{
						string Message = string.Format("Solo se efectuarán las actualizaciones de datos en modo de transacción en aquellos " +
						"equipos que cuenten con {0} MB de memoria RAM o más. Si no desea ver nuevamente este mensaje cambie el valor de " +
						"configuración \"useSqlTransaction\" de \"True\" a \"False\" ó haga una actualización de la memoria RAM de su equipo " +
						"incrementando esta de {1} MB (valor actual) a al menos {0} MB.", RAMRequired, TotalPhysicalMemoryMB);
						clsLogProcessing.WriteToEventLog(Message, EventLogEntryType.Warning);
						_useSqlTransaction = false;
					}
				}
				catch(System.ComponentModel.Win32Exception ExWin32) // The application cannot obtain the memory status.
				{
					string Message = string.Format("El sistema no pudo obtener el valor total de la memoria física (RAM) de este esquipo " +
						"para decidir si es apropiado hacer los procesos de actualizaciones de datos en modo de transacción, por tanto se ha " +
						"determinado que no se utilizarán transacciones para ello. " + Environment.NewLine + "Detalles del error: {0}", ExWin32.Message);
					clsLogProcessing.WriteToEventLog(Message, EventLogEntryType.Warning);
					_useSqlTransaction = false;
				}
				catch(System.Exception Ex)
				{
					string Message = string.Format("Ha ocurrido un error inesperado mientras el sistema intentaba obtener el valor total de " +
						"la memoria física (RAM) de este esquipo para decidir si es apropiado hacer los procesos de actualizaciones de datos " +
						"en modo de transacción, por tanto se ha determinado que no se utilizarán transacciones para ello. " + Environment.NewLine +
						"Detalles del error: {0}", Ex.Message);
					clsLogProcessing.WriteToEventLog(Message, EventLogEntryType.Warning);
					_useSqlTransaction = false;
				}
			}

			return (bool)_useSqlTransaction;
		}

		private static bool getProcessISARuleElements()
		{
			_processISARuleElements = Properties.Settings.Default.processISARuleElements;
			return (bool)_processISARuleElements;
		}

		private static void setISAServerName()
		{
			// If processISARuleElements==false then this value is not needed.
			if(!clsSettings.serviceSettings.processISARuleElements)
				return;

			_ISAServerName = Properties.Settings.Default.ISAServerName;
			if(string.IsNullOrEmpty(_ISAServerName))
			{
				try
				{
					if(Process.GetProcessesByName("isastg").Length > 0) // Check if exists "Microsoft ISA Server Storage" process.
					{
						_ISAServerName = Environment.MachineName;
						string Message = string.Format("No hay valor para el elemento \"ISAServerName\" del fichero de configuración " +
							"\"app.config\", el sistema ha encontrado que en este equipo está instalado el proceso \"Microsoft ISA Server Storage\" " +
							"por tanto se ha tomado como nombre del servidor ISA el siguiente: {0}", _ISAServerName);
						clsLogProcessing.WriteToEventLog(Message, EventLogEntryType.Warning);
					}
					else
					{
						_ISAServerName = string.Empty;
						string Message = string.Format("No hay valor para el elemento \"ISAServerName\" del fichero de configuración " +
							"\"app.config\", el sistema comprobó si en el equipo local existía el proceso \"Microsoft ISA Server Storage\" " +
							"pero no lo encontró, por tanto no se pudo determinar qué valor asignar a este parámetro, como consecuencia " +
							"no se procesarán los RuleElements del servidor ISA.");
						clsLogProcessing.WriteToEventLog(Message, EventLogEntryType.Warning);
					}
				}
				catch(System.Exception Ex)
				{
					string Message = string.Format("No hay valor para el elemento \"ISAServerName\" del fichero de configuración " +
						"\"app.config\", el sistema ha intentado, sin éxito, determinar si este equipo ejecuta el proceso \"Microsoft " +
						"ISA Server Storage\" lo cual ha provocado un error inesperado. Sin este valor el sistema no podrá procesar " +
						"los elementos de configuración del servidor ISA Server. Los detalles del error se muestran a continuación: {0}", Ex.Message);
					clsLogProcessing.WriteToEventLog(Message, EventLogEntryType.Error);
				}
			}
		}

		private static void setISAServerUserName()
		{
			// If processISARuleElements==false then this value is not needed.
			if(!clsSettings.serviceSettings.processISARuleElements)
				return;

			_ISAServerUserName = Properties.Settings.Default.ISAServerUserName;
			if(string.IsNullOrEmpty(_ISAServerUserName))
			{
				try
				{
					_ISAServerUserName = Process.GetCurrentProcess().StartInfo.UserName;
					string Message = string.Format("No hay valor para el elemento \"ISAServerUserName\" del fichero de configuración " +
						"\"app.config\", el sistema ha encontrado que se utiliza el usuario \"{0}\" para ejecutar este servicio y ha " +
						"determinado que también se empleará para conectarse al servidir ISA.", _ISAServerUserName);
					clsLogProcessing.WriteToEventLog(Message, EventLogEntryType.Warning);
				}
				catch(System.Exception Ex)
				{
					string Message = string.Format("No hay valor para el elemento \"ISAServerUserName\" del fichero de configuración " +
						"\"app.config\", y ha ocurrido un error inesperado mientras se intentaba determinar el usuario que se emplea para " +
						"la ejecución de este servicio. Los detalles del error se muestran a continuación: {0}", Ex.Message);
					clsLogProcessing.WriteToEventLog(Message, EventLogEntryType.Error);
				}
			}
		}

		private static void setISAServerUserPasswd()
		{
			// If processISARuleElements==false then this value is not needed.
			if(!clsSettings.serviceSettings.processISARuleElements)
				return;

			_ISAServerUserPasswd = Properties.Settings.Default.ISAServerUserPasswd;
			if(string.IsNullOrEmpty(_ISAServerUserPasswd))
			{
				try
				{
					string Message = string.Empty;
					if(!string.IsNullOrEmpty(_ISAServerUserName) && _ISAServerUserName == Process.GetCurrentProcess().StartInfo.UserName)
					{
						_ISAServerUserPasswd = Process.GetCurrentProcess().StartInfo.Password.ToString();
						Message = string.Format("No hay valor para el elemento \"ISAServerUserPasswd\" del fichero de configuración " +
							"\"app.config\", el sistema ha determinado utilizar la contraseña perteneciente al usuario \"{0}\", pero puede " +
							"este procedimiento no de los resultados esperados, se le recomienda que revise los valores de configuración " +
							"para este servicio que se encuentran ubicados en el fichero antes mencionado.", clsSettings.serviceSettings.ISAServer.UserName);
					}
					else
					{
						_ISAServerUserPasswd = null;
						Message = string.Format("No hay valor para el elemento \"ISAServerUserPasswd\" del fichero de configuración " +
							"\"app.config\", el sistema no ha podido establecer un valor para este elemento lo cual podrá traer como " +
							"consecuencia que el servicio no pueda modificar los RuleElements en el servidor ISA. Se le recomienda que " +
							"revise los valores de configuración para este servicio que se encuentran ubicados en el fichero antes mencionado.");
					}
					clsLogProcessing.WriteToEventLog(Message, EventLogEntryType.Warning);
				}
				catch(System.Exception Ex)
				{
					string Message = string.Format("No hay valor para el elemento \"ISAServerUserPasswd\" del fichero de configuración " +
						"\"app.config\", y ha ocurrido un error inesperado mientras se intentaba determinar la contraseña utilizada por el " +
						"usuario \"{0}\". Los detalles del error se muestran a continuación: {1}", clsSettings.serviceSettings.ISAServer.UserName, Ex.Message);
					clsLogProcessing.WriteToEventLog(Message, EventLogEntryType.Warning);
				}
			}
		}

		private static void setISAServerUserDomain()
		{
			// If processISARuleElements==false then this value is not needed.
			if(!clsSettings.serviceSettings.processISARuleElements)
				return;

			_ISAServerUserDomain = string.IsNullOrEmpty(Properties.Settings.Default.ISAServerUserDomain) ? clsSettings.serviceSettings.ISAServer.Name : Properties.Settings.Default.ISAServerUserDomain;
		}

	} // Fin de la clase.

} //Fin del namespace.
