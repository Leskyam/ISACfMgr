using System;
using System.Net;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.IO;

namespace LAMSoft.ISACFMgr
{
	/// <summary>
	/// Partial class for checking and downloading the compressed source file.
	/// </summary>
	public partial class ISACFMgr : ServiceBase
	{

		/// <summary>
		/// Check if there is a file (see LastModifiedDate Header of the response) newer than the las downloaded.
		/// </summary>
		/// <param name="newUri"></param>
		/// <param name="fileNameFullPath"></param>
		/// <param name="lastModified"></param>
		/// <param name="contentLength"></param>
		/// <param name="ETag"></param>
		/// <returns></returns>
		private static bool IsThereNewFileToDownload(out string fileNameFullPath, out string ETag)
		{
			// Set the currentOperation
			ServiceSettings.serviceSettings.CurrentOperation = operationList.checkingUriLastModified;

			fileNameFullPath = string.Empty;
			ETag = string.Empty;
			DateTime lastModified = DateTime.MinValue;

			// Begining counter.
			int count = 0;
			// How many try in case of error.
			int finishAt = 10;
			// Sleep time (in minutes) for the thread in case of error.
			//TimeSpan delayByError = new TimeSpan(0, 10, 00);

			//bool result = false;
			while(count++ < finishAt)
			{
				try
				{
					// Create a request for the URL. 		
					WebRequest webRequest = WebRequest.Create(ServiceSettings.serviceSettings.DownloadUri);

					// Bypass the cache.
					System.Net.Cache.RequestCachePolicy cachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
					webRequest.CachePolicy = cachePolicy;

					// Set the User-Agent Header
					webRequest.Headers.Add(HttpRequestHeader.UserAgent.ToString(), ServiceSettings.serviceSettings.DownloaderUserAgent);

					// Get the response.
					HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();

					if(webRequest != null)
						webRequest = null; // Cleanup

					// Solve this, the real name of the file to download, maybe the uri in the configuration does't 
					// contain the real name of the file to download and we need it to save it to the file system.
					System.IO.FileInfo file = new System.IO.FileInfo(webResponse.ResponseUri.AbsolutePath);
					fileNameFullPath = string.Format("{0}{1}-{2}", ServiceSettings.serviceSettings.DataDirectory, ServiceSettings.GetPrefixForFileNameFromDate(webResponse.LastModified), file.Name);
					lastModified = webResponse.LastModified;
					ETag = webResponse.Headers[HttpResponseHeader.ETag.ToString()];
					webResponse.Close();

					bool result = webResponse.LastModified > ServiceSettings.serviceSettings.DownloadedFileLastModifiedDate;

					if(webResponse != null)
						webResponse = null; // Cleanup.

					if(!result)
						LogProcessing.WriteToEventLog(string.Format("La fecha del fichero a descargar es: {0}, " +
							"el sistema ha determinado que no es necesario descargarlo nuevamente.", lastModified), EventLogEntryType.Information);
					return result;
				}
				catch(System.Exception Ex)
				{
					LogProcessing.WriteToEventLog(string.Format("Error comprobando la dirección url \"{0}\"" +
						Environment.NewLine + "Este es el intento {1} de {2} posibilidades, a intervalos de {3} " +
						"minutos. " + Environment.NewLine + "A continuación se muestran los detalles del error: " +
						Environment.NewLine + "{4}",
						ServiceSettings.serviceSettings.DownloadUri, count, finishAt, _delayToRetryInterval.TotalMinutes, Ex.Message), EventLogEntryType.Error);
					Thread.Sleep(_delayToRetryInterval);
					continue;
				}
			}

			string s = ServiceSettings.serviceSettings.CheckForUpdateInterval.TotalMinutes > 120 ? (ServiceSettings.serviceSettings.CheckForUpdateInterval.TotalMinutes / 60).ToString() + " horas" : ServiceSettings.serviceSettings.CheckForUpdateInterval.TotalMinutes.ToString() + " minutos";
			LogProcessing.WriteToEventLog(string.Format("Finalmente no se ha podido comprobar la dirección url \"{0}\"" +
				Environment.NewLine + "El sistema iniciará operaciones nuevamente dentro de {1}. " +
				Environment.NewLine + "Revise eventos anteriores para ver los detalles de la causa que ha impedido la operación",
				ServiceSettings.serviceSettings.DownloadUri, s), EventLogEntryType.Error);
			return false;

		}

		/// <summary>
		/// Download the source file containing the list of categories and blacklisted sites.
		/// </summary>
		/// <param name="realUri"></param>
		/// <param name="finalFileName"></param>
		private static bool DownloadSourceFile(string fileNameFullPath, string originalETag)
		{
			// Set the currentOperation
			ServiceSettings.serviceSettings.CurrentOperation = operationList.downloadingSourceFile;

			// Begining counter.
			int count = 0;
			// How many try in case of error.
			int finishAt = 20;
			// Used to add range to resume the download if needed.
			long startingPoint = 0;

			//bool result = false;
			while (count++ < finishAt)
			{
				try
				{
				doResume: // If we get here then the download was interrupted without report any error.
					HttpWebRequest webRequest = null;
					HttpWebResponse webResponse = null;
					Stream strResponse = null;
					Stream strLocalFile = null;
					long contentLength;
					DateTime lastModified;

				beginAgain: // If we get here, probably the file in the server was changed in the middle of the doanloading process.
					if(File.Exists(fileNameFullPath))
						startingPoint = new FileInfo(fileNameFullPath).Length;
					else
						startingPoint = 0;

					webRequest = (HttpWebRequest)WebRequest.Create(ServiceSettings.serviceSettings.DownloadUri);
					webRequest.Headers.Add(HttpRequestHeader.UserAgent.ToString(), ServiceSettings.serviceSettings.DownloaderUserAgent);
					System.Net.Cache.RequestCachePolicy cachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
					webRequest.CachePolicy = cachePolicy;
					webRequest.AddRange(Convert.ToInt32(startingPoint));
					webRequest.Timeout = 180000; // Three minutes before the request timeout.
					webRequest.ReadWriteTimeout = 600000; // Ten minutes for stream (Read, Write) operations.

					webResponse = (HttpWebResponse)webRequest.GetResponse();

					contentLength = webResponse.ContentLength + startingPoint;
					lastModified = webResponse.LastModified;

					// Check if the file to download is the same checked by the "isThereNewFileToDownload" method.
					if(originalETag != webResponse.Headers[HttpResponseHeader.ETag.ToString()])
					{
						// Delete the old file we were downloading because it was changed in the middle of the process and there is a new one to download.
						File.Delete(fileNameFullPath);
						// Get again the correct "fileNameFullPath" and "ETag" values of the new file to download.
						IsThereNewFileToDownload(out fileNameFullPath, out originalETag);
						// Set the currentOperation to "downloadingSourceFile" again because the "isThereNewFileToDownload" method had chenged it.
						ServiceSettings.serviceSettings.CurrentOperation = operationList.downloadingSourceFile;
						//cleanup
						webResponse.Close();
						// Go to the begining of the process again.
						goto beginAgain;
					}

					strResponse = webResponse.GetResponseStream();

					if(startingPoint == 0)
					{
						strLocalFile = new FileStream(fileNameFullPath, FileMode.Create, FileAccess.Write, FileShare.None);
					}
					else
					{
						strLocalFile = new FileStream(fileNameFullPath, FileMode.Append, FileAccess.Write, FileShare.None);
					}

					// Stores the current number of bytes retrieved from the server.
					int byteSize = 0;
					// A buffer to store and write the data retrieved from server.
					byte[] downBuffer = new byte[2048];

					while((byteSize = strResponse.Read(downBuffer, 0, downBuffer.Length)) > 0)
					{
						strLocalFile.Write(downBuffer, 0, byteSize);
					}

					// Do some cleanup here.
					if(webRequest != null)
						webRequest = null;
					if(webResponse != null)
					{
						webResponse.Close();
						webResponse = null;
					}
					if(strResponse != null)
					{
						strResponse.Close();
						strResponse.Dispose();
					}
					if(strLocalFile != null)
					{
						strLocalFile.Close();
						strLocalFile.Dispose();
					}

					System.IO.FileInfo file = new System.IO.FileInfo(fileNameFullPath);
					if(file.Length == contentLength)
					{
						ServiceSettings.serviceSettings.DownloadedFileName = new FileInfo(fileNameFullPath).Name;
						ServiceSettings.serviceSettings.DownloadedFileLastModifiedDate = lastModified;
						ServiceSettings.serviceSettings.DownloadedFileETag = originalETag;
						return true;
					}
					else
					{
						goto doResume;
					}
				}
				catch(System.Exception Ex)
				{
					if(startingPoint != 0 & File.Exists(fileNameFullPath) & Ex.Message.Contains("Requested Range Not Satisfiable")) // Probably (416) Error - RequestedRangeNotSatisfiable Exception.
						File.Delete(fileNameFullPath);
					LogProcessing.WriteToEventLog(string.Format("Error descargando desde dirección url \"{0}\"" +
						Environment.NewLine + "Este es el intento {1} de {2} posibilidades, a intervalos de {3} " +
						"minutos. " + Environment.NewLine + "A continuación se muestran los detalles del error: " +
						Environment.NewLine + "{4}",
						ServiceSettings.serviceSettings.DownloadUri, count, finishAt, _delayToRetryInterval.TotalMinutes, Ex.Message), EventLogEntryType.Error);
					Thread.Sleep(_delayToRetryInterval);
					continue;
				}
			}

			string s = ServiceSettings.serviceSettings.CheckForUpdateInterval.TotalMinutes > 120 ? (ServiceSettings.serviceSettings.CheckForUpdateInterval.TotalMinutes / 60).ToString() + " horas" : ServiceSettings.serviceSettings.CheckForUpdateInterval.TotalMinutes.ToString() + " minutos";
			LogProcessing.WriteToEventLog(string.Format("Finalmente no se ha podido descargar desde dirección url \"{0}\"" +
				Environment.NewLine + "El sistema iniciará operaciones nuevamente dentro de {1}. " +
				Environment.NewLine + "Revise eventos anteriores para ver los detalles de la causa que ha impedido la operación.",
				ServiceSettings.serviceSettings.DownloadUri, s), EventLogEntryType.Error);
			return false;
		}

	}
}
