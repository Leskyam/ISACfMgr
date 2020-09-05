using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Collections.Generic;

namespace LAMSoft.ISACFMgr
{
	public partial class ISACFMgr : ServiceBase
	{
		/// <summary>
		/// Constructor for this class.
		/// </summary>
		public ISACFMgr()
		{
			//this.AutoLog = true;
			this.ServiceName = "ISACFMgr";
			InitializeComponent();
		}

		/// <summary>
		/// Enum for the operations to execute in the diferent moments of the life cycle of the service.
		/// </summary>
		internal enum operationList
		{
			settingInitialConfigValues = 1,
			checkingUriLastModified,
			downloadingSourceFile,
			decompressingSourceFile,
			generatingXmlFormatedIndex,
			generatingXmlFormatedContent,
			updatingSqlDataBase,
			updatingISARuleElements,
			waitingForNextExecution = 100,
		}

		/// <summary>
		/// To manipulate the index in Xml format.
		/// </summary>
		private struct Categoria
		{
			public string name;
			public string defaualt_type;
			public string desc_en;
			public string desc_es;
			public string name_en;
			public string name_es;
		}

		/// <summary>
		/// The flag to know the service status.
		/// </summary>
		private bool serviceStarted = false;

		/// <summary>
		/// The thread to do all the work.
		/// </summary>
		private Thread workerThread;

		/// <summary>
		/// Delay interval used to retry when requesting resources from Internet.
		/// </summary>
		private static TimeSpan _delayToRetryInterval = new TimeSpan(0, 5, 0);

		/// <summary>
		/// Start the service, set the initial values for service settings variables.
		/// </summary>
		/// <param name="args"></param>
		protected override void OnStart(string[] args)
		{
			/*
				Set initial values and get the values indicating the last state 
				of the service before stop the last time.
			*/
			try
			{
				// Get and Set the initial configuration parameters from the configuration file.
				ServiceSettings.SetServiceSettings();
			}
			catch(Exception Ex)
			{
				LogProcessing.WriteToEventLog(string.Format("Error durante la ejecución del procedimiento que " +
				"establece los parámetros de configuración inicial en el evento \"OnStart\" del servicio {0}." +
				"Los detalles de este error se muestran a continuación: " + Environment.NewLine + "{1}", this.ServiceName, Ex.ToString()), EventLogEntryType.Error);
			}

			// Create the worker thread. This will invoque the "begingProcessing" function.
			// when we start it.
			// Since we are using a separate thread, the service's main thread will
			// return quickly, telling Windows that the service has started. 
			try
			{
				ThreadStart ts = new ThreadStart(begingProcessing);
				workerThread = new Thread(ts);

				// Set the flag to indicate the workerThread is active.
				serviceStarted = true;

				// Start the thread.
				workerThread.Start();
			}
			catch(System.Exception Ex)
			{
				LogProcessing.WriteToEventLog(string.Format("Error durante la ejecución del procedimiento que " +
				"establece el hilo (thread) para este servicio, sucedió en el evento \"OnStart\" del servicio {0}." +
				"Los detalles de este error se muestran a continuación: " + Environment.NewLine + "{1}", this.ServiceName, Ex.ToString()), EventLogEntryType.Error);
			}
		}

		/// <summary>
		/// Stop the service, when the current operation is not waitingForNextExecution, the thread is delayed for 30 seconds.
		/// </summary>
		protected override void OnStop()
		{
			serviceStarted = false;
			// If the status of operations is not idle, give 30 seconds to stop the thread.

			if(ServiceSettings.serviceSettings.CurrentOperation != operationList.waitingForNextExecution)
				workerThread.Join(new TimeSpan(0, 0, 30));
			else
				workerThread.Join(new TimeSpan(0, 0, 0));
		}

		/// <summary>
		/// This function will do all the work, when it finish its tasks, 
		/// it will be suspended for some time. It will continue to repeat
		/// this until the service is stopped.
		/// </summary>
		private void begingProcessing()
		{
			try
			{
				// Notify the loaded settings values for this service.
				string strMessage = "VALORES DE CONFIGURACIÓN ENCONTRADOS" + Environment.NewLine;
				strMessage += Environment.NewLine;
				// Localización y descarga de datos primarios
				strMessage += "-Localización y descarga de datos primarios" + Environment.NewLine;
				strMessage += "  Sitio origen del fichero a descargar: " + ServiceSettings.serviceSettings.DownloadUri + Environment.NewLine;
				strMessage += "  User-Agent para solicitudes Internet: " + ServiceSettings.serviceSettings.DownloaderUserAgent + Environment.NewLine;
				strMessage += Environment.NewLine;
				// Rutas temporales para procesamiento
				strMessage += "-Rutas temporales para procesamiento" + Environment.NewLine;
				strMessage += "  Directorio base para procesamiento: " + ServiceSettings.serviceSettings.WorkingDirectory + Environment.NewLine;
				strMessage += "  Subdirectorio para almacenar datos: " + ServiceSettings.serviceSettings.DataDirectory + Environment.NewLine;
				strMessage += "  Subdirectorio para almacenar logs: " + ServiceSettings.serviceSettings.LogsDirectory + Environment.NewLine;
				strMessage += Environment.NewLine;
				// Estado
				strMessage += "-Estado" + Environment.NewLine;
				strMessage += "  Siguiente operación a procesar: " + ServiceSettings.serviceSettings.CurrentOperation + Environment.NewLine;
				strMessage += "  Fecha de la última actualización: " + (ServiceSettings.serviceSettings.DownloadedFileLastModifiedDate != DateTime.MinValue ? ServiceSettings.serviceSettings.DownloadedFileLastModifiedDate.ToString() : "Nunca") + Environment.NewLine;
				strMessage += "  Horas entre chequeos para actualización: " + ServiceSettings.serviceSettings.CheckForUpdateInterval.TotalHours + Environment.NewLine;
				strMessage += Environment.NewLine;
				// Servidor SQL
				strMessage += "-Servidor SQL" + Environment.NewLine;
				strMessage += "  Utilizar transacciones: " + (ServiceSettings.serviceSettings.UseSqlTransaction ? "Sí" : "No").ToString() + Environment.NewLine;
				strMessage += Environment.NewLine;
				//Servidor ISA
				strMessage += "-Servidor ISA" + Environment.NewLine;
				strMessage += "  Procesar RuleElements: " + (ServiceSettings.serviceSettings.ProcessISARuleElements ? "Sí" : "No").ToString() + Environment.NewLine;
				if(ServiceSettings.serviceSettings.ProcessISARuleElements)
				{
					strMessage += "  Nombre del servidor: " + (string.IsNullOrEmpty(ServiceSettings.serviceSettings.ISAServer.Name) ? "No configurado" : ServiceSettings.serviceSettings.ISAServer.Name).ToString() + Environment.NewLine;
					strMessage += "  Nombre de usuario: " + (string.IsNullOrEmpty(ServiceSettings.serviceSettings.ISAServer.UserName) ? "No configurado" : ServiceSettings.serviceSettings.ISAServer.UserName).ToString() + Environment.NewLine;
					strMessage += "  Contraseña de usuario: " + (string.IsNullOrEmpty(ServiceSettings.serviceSettings.ISAServer.UserPasswd) ? "No configurada" : "*****").ToString() + Environment.NewLine;
					strMessage += "  Dominio de usuario (opcional): " + ServiceSettings.serviceSettings.ISAServer.UserDomain + Environment.NewLine;
				}
				LogProcessing.WriteToEventLog(strMessage, EventLogEntryType.Information);
			}
			catch(System.Exception Ex)
			{
				string ErrMessage = string.Format("Error obteniendo configuraciones. El servicio no efectuará procesamiento alguno " +
					"aunque permanezca en ejecución. Se recomienda revisar los valores de los elementos de configuración del fichero " +
					"\"app.config\". Los detalles de este error se muestran a continuación: {0}", Ex.ToString());
				LogProcessing.WriteToEventLog(ErrMessage, EventLogEntryType.Error);
				serviceStarted = false;
			}

			// Start an endless loop; loop will abort only when "serviceStarted" flag = false;
			while(serviceStarted)
			{
				// It's very important to catch errors here, otherwise we will never know what happens if somthing was wrong.
				try
				{
					switch(ServiceSettings.serviceSettings.CurrentOperation)
					{
						case operationList.decompressingSourceFile:
							{
								// Notify the restoring action.
								LogProcessing.ReportRecoveryAction(operationList.decompressingSourceFile);

								DecompressSourceFile();

								GenerateXmlFormatedIndex();

								GenerateXmlFormatedContent();

								if(UpdateSqlDataBase() && ServiceSettings.serviceSettings.ProcessISARuleElements)
								{
										updateISARuleElements();
								}

								break;
							}
						case operationList.generatingXmlFormatedIndex:
							{
								// Notify the restoring action.
								LogProcessing.ReportRecoveryAction(operationList.generatingXmlFormatedIndex);

								GenerateXmlFormatedIndex();

								GenerateXmlFormatedContent();

								if(UpdateSqlDataBase() && ServiceSettings.serviceSettings.ProcessISARuleElements)
								{
									updateISARuleElements();
								}

								break;
							}
						case operationList.generatingXmlFormatedContent:
							{
								// Notify the restoring action.
								LogProcessing.ReportRecoveryAction(operationList.generatingXmlFormatedContent);

								GenerateXmlFormatedContent();

								if(UpdateSqlDataBase() && ServiceSettings.serviceSettings.ProcessISARuleElements)
								{
									updateISARuleElements();
								}

								break;
							}
						case operationList.updatingSqlDataBase:
							{
								// Notify the restoring action.
								LogProcessing.ReportRecoveryAction(operationList.updatingSqlDataBase);

								if(UpdateSqlDataBase() && ServiceSettings.serviceSettings.ProcessISARuleElements)
								{
									updateISARuleElements();
								}

								break;
							}
						case operationList.updatingISARuleElements:
							{
								// Notify the restoring action.
								LogProcessing.ReportRecoveryAction(operationList.updatingISARuleElements);

								if(ServiceSettings.serviceSettings.ProcessISARuleElements)
									updateISARuleElements();

								break;
							}
						default: // Execute all operations in any case diferent the above.
							{
								string fileNameFullPath; // The path and name of the file to save in the local file system.
								string ETag; //ETag Header in the response.

								if(IsThereNewFileToDownload(out fileNameFullPath, out ETag))
								{
									if(DownloadSourceFile(fileNameFullPath, ETag))
									{

										if(DecompressSourceFile())
										{
											GenerateXmlFormatedIndex();
											GenerateXmlFormatedContent();
											if(UpdateSqlDataBase() && ServiceSettings.serviceSettings.ProcessISARuleElements)
											{
												updateISARuleElements();
											}
										}
									}
								}

								break;
							}
					}

					// Delete Downloaded files "...data\*-fileName.tar.gz" , BL folder "...data\BL".
					cleanTrash();			
					
					// When all operations are finished set the currentOperation to waitingForNextExecution.
					ServiceSettings.serviceSettings.CurrentOperation = operationList.waitingForNextExecution;
					// Return the operation to waiting for new execution.

				}
				catch(System.Exception Ex)
				{
					string errMessage = string.Format("Ha ocurrido un error en el método \"beginProcessing\". Los detalles de este error se muestran a continuación: {0}", Ex.ToString());
					LogProcessing.WriteToEventLog(errMessage, EventLogEntryType.Error);
					// When error, set the currentOperation to waitingForNextExecution to try next time.
					ServiceSettings.serviceSettings.CurrentOperation = operationList.waitingForNextExecution;
				}

				// Put the thread to sleep for a while.
				if(serviceStarted)
				{
					//Thread.Sleep(new TimeSpan(0, 0, 10));
					Thread.Sleep(ServiceSettings.serviceSettings.CheckForUpdateInterval);
				}
			}

			// It's time to end the thread.
			Thread.CurrentThread.Abort();

		} // Fin del método: beginProcessing()

		#region " UTILITIES "

		private void cleanTrash()
		{
			// Delete downloadedFiles.
			string filter = string.Format("*{0}", ServiceSettings.serviceSettings.DownloadedFileName.Substring(ServiceSettings.serviceSettings.DownloadedFileName.IndexOf('-')));
			string[] downloadedFiles = System.IO.Directory.GetFiles(ServiceSettings.serviceSettings.DataDirectory.FullName, filter, System.IO.SearchOption.TopDirectoryOnly);

			foreach(string f in downloadedFiles)
			{
				if(System.IO.File.Exists(f))
					System.IO.File.Delete(f);
			}

			// Delete BL folder and subfolders.
			string BLFolderPath = System.IO.Path.Combine(ServiceSettings.serviceSettings.DataDirectory.FullName, "BL");

			if(System.IO.Directory.Exists(BLFolderPath))
				System.IO.Directory.Delete(BLFolderPath, true);

			// Try to free som Memory.
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
		}

		#endregion " END UTILITIES "


	} // Fin de la clase.

} // Fin del namespace.
