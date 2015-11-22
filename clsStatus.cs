using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;

namespace LAMSoft.ISACFMgr
{
	class clsStatus
	{
		const string _statusFileName = "status.xml";

		public enum statusFields
		{
			currentOperationName,
			downloadedFileLastModifiedDate,
			downloadedFileETag,
			downloadedFileName,
		}

		private static void createStatusLogFile()
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
				File.AppendAllText(string.Format("{0}{1}", clsSettings.serviceSettings.logsDirectory, _statusFileName), sbXml.ToString());
			}
			catch (System.Exception ex)
			{
				clsLogProcessing.WriteToEventLog(string.Format("Error al crear el fichero \"{0}\". " + Environment.NewLine + "ERROR: {1}", _statusFileName, ex.Message), EventLogEntryType.Error);
			}
		}

		internal static ISACFMgr.operationList getCurrentOperation()
		{
			try
			{
				Type operations = typeof(ISACFMgr.operationList);
				Int32 i = Int32.Parse(Enum.Format(operations, Enum.Parse(operations, getStatusValue(statusFields.currentOperationName)), "d"));
				return (ISACFMgr.operationList)i;
			}
			catch (System.Exception Ex)
			{
				clsLogProcessing.WriteToEventLog(string.Format("No se pudo obtener correctamente el " +
					"valor del elemento \"currentOperationName\" del fichero \"status.xml\". Si es la " +
					"primera vez que se observa este error pase por alto este mensaje, los detalles se " +
					"muestran a continuación: " + Environment.NewLine + "ERROR: {0}", Ex.ToString()), EventLogEntryType.Error);
				return ISACFMgr.operationList.waitingForNextExecution;
			}
		}

		internal static DateTime getDownloadedFileLastModifiedDate()
		{
			try
			{
				return DateTime.Parse(getStatusValue(statusFields.downloadedFileLastModifiedDate));
			}
			catch (System.Exception Ex)
			{
				clsLogProcessing.WriteToEventLog(string.Format("No se pudo obtener correctamente el valor del " +
					"elemento \"downloadedFileLastModifiedDate\" del fichero \"status.xml\". Si es la primera " +
					"vez que se observa este error pase por alto este mensaje, los detalles se muestran a continuación: "
					+ Environment.NewLine + "ERROR: {0}", Ex.ToString()), EventLogEntryType.Error);
				return DateTime.MinValue;
			}
		}

		internal static string getDownloadedFileETag()
		{
			try
			{
				return getStatusValue(statusFields.downloadedFileETag);
			}
			catch (System.Exception Ex)
			{
				clsLogProcessing.WriteToEventLog(string.Format("No se pudo obtener correctamente el valor del " +
					"elemento \"downloadedFileETag\" del fichero \"status.xml\". Si es la primera vez que se " + 
					"observa este error pase por alto este mensaje, los detalles se muestran a continuación: "
					+ Environment.NewLine + "ERROR: {0}", Ex.ToString()), EventLogEntryType.Error);
				return string.Empty;
			}
		}

		internal static string getDownloadedFileName()
		{
			try
			{
				return getStatusValue(statusFields.downloadedFileName);
			}
			catch (System.Exception Ex)
			{
				clsLogProcessing.WriteToEventLog(string.Format("No se pudo obtener correctamente el valor del " +
					"elemento \"downloadedFileName\" del fichero \"status.xml\". Si es la primera vez que se " +
					"observa este error pase por alto este mensaje, los detalles se muestran a continuación: "
					+ Environment.NewLine + "ERROR: {0}", Ex.ToString()), EventLogEntryType.Error);
				return string.Empty;
			}
		}

		internal static void setStatusValue(statusFields fieldName, string newValue)
		{
			try
			{
				string statusFileFullPath = string.Format("{0}{1}", clsSettings.serviceSettings.logsDirectory, _statusFileName);
				XDocument xDocument = XDocument.Load(statusFileFullPath);
				xDocument.Element("serviceStatus").SetElementValue(fieldName.ToString(), newValue);
				xDocument.Save(statusFileFullPath);
			}
			catch (System.Exception Ex)
			{
				clsLogProcessing.WriteToEventLog(Ex.Message, EventLogEntryType.Error);
			}

		}

		private static string getStatusValue(statusFields fieldName)
		{
			string statusFileFullPath = string.Format("{0}{1}", clsSettings.serviceSettings.logsDirectory, _statusFileName);
			if (!File.Exists(statusFileFullPath))
			{
				createStatusLogFile();
				//clsSettings.serviceSettings.currentOperation = ISACFMgr.operationList.waitingForNextExecution;
				//return ISACFMgr.operationList.waitingForNextExecution.ToString();
			}

			switch (fieldName)
			{
				case statusFields.currentOperationName:
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
						clsLogProcessing.WriteToEventLog(string.Format("Error obteniendo el valor del elemento \"{0}\" del fichero {1}. ERROR: {2}",
																														fieldName.ToString(), statusFileFullPath, ex.Message),
																														EventLogEntryType.Error);
						// Record the error but keep the service operational.
						return ISACFMgr.operationList.waitingForNextExecution.ToString();
					}

				case statusFields.downloadedFileLastModifiedDate:
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
						clsLogProcessing.WriteToEventLog(string.Format("Error obteniendo el valor de \"{0}\" del fichero {1}. ERROR: {2}",
																														fieldName.ToString(), statusFileFullPath, ex.Message),
																														EventLogEntryType.Error);
						return null;
					}
				case statusFields.downloadedFileETag:
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
				case statusFields.downloadedFileName:
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
						clsLogProcessing.WriteToEventLog(string.Format("Error obteniendo el valor del elemento \"{0}\" del fichero {1}. ERROR: {2}",
																														fieldName.ToString(), statusFileFullPath, ex.Message),
																														EventLogEntryType.Error);
						// Record the error but keep the service operational.
						return string.Empty;
					}
				default:
					return null;
			}
		}

		internal static Decimal getAvailablePhysicalMemoryMB()
		{
			try
			{
				ComputerInfo compInfo = new ComputerInfo();
				return Decimal.Round(((Decimal)compInfo.AvailablePhysicalMemory / 1048576),2); // Convert to MB.
			}
			catch(System.Exception)
			{
				return 0.0M;
			}

		}

	} // Fin de la clase.

} // Fin del namespace.
