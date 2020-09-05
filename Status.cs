using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;

namespace LAMSoft.ISACFMgr
{
    class Status
	{
		const string statusFileName = "status.xml";

		public enum Fields
		{
			CurrentOperationName,
			DownloadedFileLastModifiedDate,
			DownloadedFileETag,
			DownloadedFileName,
		}

		private static void CreateStatusLogFile()
		{
			StringBuilder sbXml = new StringBuilder();
			sbXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
			sbXml.AppendLine("<serviceStatus>");
			sbXml.AppendLine(string.Format("		<currentOperationName>{0}</currentOperationName>", ISACFMgr.operationList.waitingForNextExecution));
			sbXml.AppendLine(string.Format("		<downloadedFileName>{0}</downloadedFileName>", string.Empty));
			sbXml.AppendLine(string.Format("		<downloadedFileLastModifiedDate>{0}</downloadedFileLastModifiedDate>", DateTime.MinValue));
			sbXml.AppendLine(string.Format("		<downloadedFileETag>{0}</downloadedFileETag>", "1cd14ae-949656-495d549870240"));
			sbXml.AppendLine("</serviceStatus>");
			try
			{
				File.AppendAllText(string.Format("{0}{1}", ServiceSettings.serviceSettings.LogsDirectory, statusFileName), sbXml.ToString());
			}
			catch (System.Exception ex)
			{
				LogProcessing.WriteToEventLog(string.Format("Error al crear el fichero \"{0}\". " + Environment.NewLine + "ERROR: {1}", statusFileName, ex.Message), EventLogEntryType.Error);
			}
		}

		internal static ISACFMgr.operationList GetCurrentOperation()
		{
			try
			{
				Type operations = typeof(ISACFMgr.operationList);
				Int32 i = Int32.Parse(Enum.Format(operations, Enum.Parse(operations, GetStatusValue(Fields.CurrentOperationName)), "d"));
				return (ISACFMgr.operationList)i;
			}
			catch (System.Exception Ex)
			{
				LogProcessing.WriteToEventLog(string.Format("No se pudo obtener correctamente el " +
					"valor del elemento \"currentOperationName\" del fichero \"status.xml\". Si es la " +
					"primera vez que se observa este error pase por alto este mensaje, los detalles se " +
					"muestran a continuación: " + Environment.NewLine + "ERROR: {0}", Ex.ToString()), EventLogEntryType.Error);
				return ISACFMgr.operationList.waitingForNextExecution;
			}
		}

		internal static DateTime GetDownloadedFileLastModifiedDate()
		{
			try
			{
				return DateTime.Parse(GetStatusValue(Fields.DownloadedFileLastModifiedDate));
			}
			catch (System.Exception Ex)
			{
				LogProcessing.WriteToEventLog(string.Format("No se pudo obtener correctamente el valor del " +
					"elemento \"downloadedFileLastModifiedDate\" del fichero \"status.xml\". Si es la primera " +
					"vez que se observa este error pase por alto este mensaje, los detalles se muestran a continuación: "
					+ Environment.NewLine + "ERROR: {0}", Ex.ToString()), EventLogEntryType.Error);
				return DateTime.MinValue;
			}
		}

		internal static string GetDownloadedFileETag()
		{
			try
			{
				return GetStatusValue(Fields.DownloadedFileETag);
			}
			catch (System.Exception Ex)
			{
				LogProcessing.WriteToEventLog(string.Format("No se pudo obtener correctamente el valor del " +
					"elemento \"downloadedFileETag\" del fichero \"status.xml\". Si es la primera vez que se " + 
					"observa este error pase por alto este mensaje, los detalles se muestran a continuación: "
					+ Environment.NewLine + "ERROR: {0}", Ex.ToString()), EventLogEntryType.Error);
				return string.Empty;
			}
		}

		internal static string GetDownloadedFileName()
		{
			try
			{
				return GetStatusValue(Fields.DownloadedFileName);
			}
			catch (System.Exception Ex)
			{
				LogProcessing.WriteToEventLog(string.Format("No se pudo obtener correctamente el valor del " +
					"elemento \"downloadedFileName\" del fichero \"status.xml\". Si es la primera vez que se " +
					"observa este error pase por alto este mensaje, los detalles se muestran a continuación: "
					+ Environment.NewLine + "ERROR: {0}", Ex.ToString()), EventLogEntryType.Error);
				return string.Empty;
			}
		}

		internal static void SetStatusValue(Fields fieldName, string newValue)
		{
			try
			{
				string statusFileFullPath = string.Format("{0}{1}", ServiceSettings.serviceSettings.LogsDirectory, statusFileName);
				XDocument xDocument = XDocument.Load(statusFileFullPath);
				xDocument.Element("serviceStatus").SetElementValue(fieldName.ToString(), newValue);
				xDocument.Save(statusFileFullPath);
			}
			catch (System.Exception Ex)
			{
				LogProcessing.WriteToEventLog(Ex.Message, EventLogEntryType.Error);
			}

		}

		private static string GetStatusValue(Fields fieldName)
		{
			string statusFileFullPath = string.Format("{0}{1}", ServiceSettings.serviceSettings.LogsDirectory, statusFileName);
			if (!File.Exists(statusFileFullPath))
			{
				CreateStatusLogFile();
				//clsSettings.serviceSettings.currentOperation = ISACFMgr.operationList.waitingForNextExecution;
				//return ISACFMgr.operationList.waitingForNextExecution.ToString();
			}

			switch (fieldName)
			{
				case Fields.CurrentOperationName:
					//ISACFMgr.operationList currrentOperation;
					try
					{
						XDocument xDocument = XDocument.Load(statusFileFullPath);
						string qresult = (from c in xDocument.Descendants(fieldName.ToString())
															select c.Value).Single();
						return string.IsNullOrEmpty(qresult) ? ISACFMgr.operationList.waitingForNextExecution.ToString() : qresult;
					}
					catch (System.Exception ex)
					{
						LogProcessing.WriteToEventLog(string.Format("Error obteniendo el valor del elemento \"{0}\" del fichero {1}. ERROR: {2}",
																														fieldName.ToString(), statusFileFullPath, ex.Message),
																														EventLogEntryType.Error);
						// Record the error but keep the service operational.
						return ISACFMgr.operationList.waitingForNextExecution.ToString();
					}

				case Fields.DownloadedFileLastModifiedDate:
					//downloadedFileLastModifiedDate
					try
					{
						//clsLogProcessing.WriteToEventLog(string.Format("I'am here... Before the query. fieldName.ToString(): {0}", fieldName.ToString()), EventLogEntryType.Warning);

						XDocument xDocument = XDocument.Load(statusFileFullPath);
						string qresult = (from c in xDocument.Descendants(fieldName.ToString())
															select c.Value).Single();
						//Retirar esto.
						//clsLogProcessing.WriteToEventLog(string.Format("qresult: {0}", qresult), EventLogEntryType.Warning);

						return qresult;
					}
					catch (System.Exception ex)
					{
						LogProcessing.WriteToEventLog(string.Format("Error obteniendo el valor de \"{0}\" del fichero {1}. ERROR: {2}",
																														fieldName.ToString(), statusFileFullPath, ex.Message),
																														EventLogEntryType.Error);
						return null;
					}
				case Fields.DownloadedFileETag:
					//ISACFMgr.operationList downloadedFileETag;
					/*
					try
					{
						XDocument xDocument = XDocument.Load(statusFileFullPath);
						string qresult = (from c in xDocument.Descendants(fieldName.ToString())
															select c.Value).Single();
						return string.IsNullOrEmpty(qresult) ? string.Empty : qresult;
					}
					catch (System.Exception ex)
					{
						clsLogProcessing.WriteToEventLog(string.Format("Error obteniendo el valor del elemento \"{0}\" del fichero {1}. ERROR: {2}",
																														fieldName.ToString(), statusFileFullPath, ex.Message),
																														EventLogEntryType.Error);
						// Record the error but keep the service operational.
						return ISACFMgr.operationList.waitingForNextExecution.ToString();
					}
					*/ 
				case Fields.DownloadedFileName:
					//ISACFMgr.operationList downloadedFileName;
					try
					{
						XDocument xDocument = XDocument.Load(statusFileFullPath);
						string qresult = (from c in xDocument.Descendants(fieldName.ToString())
															select c.Value).Single();
						return string.IsNullOrEmpty(qresult) ? string.Empty : qresult;
					}
					catch (System.Exception ex)
					{
						LogProcessing.WriteToEventLog(string.Format("Error obteniendo el valor del elemento \"{0}\" del fichero {1}. ERROR: {2}",
																														fieldName.ToString(), statusFileFullPath, ex.Message),
																														EventLogEntryType.Error);
						// Record the error but keep the service operational.
						return string.Empty;
					}
				default:
					return null;
			}
		}

		internal static decimal GetAvailablePhysicalMemoryMB()
		{
			try
			{
				ComputerInfo compInfo = new ComputerInfo();
				return decimal.Round(((decimal)compInfo.AvailablePhysicalMemory / 1048576),2); // Convert to MB.
			}
			catch(Exception)
			{
				return 0.0M;
			}

		}

	} // Fin de la clase.

} // Fin del namespace.
