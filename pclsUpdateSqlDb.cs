using System;
using System.ServiceProcess;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Xml.Linq;
using System.Net;
using System.Security.Permissions;

namespace LAMSoft.ISACFMgr
{
	public partial class ISACFMgr : ServiceBase
	{
		private sqlOperationList _currentSubOperation;
		// Nombre del fichero donde está el contenido de las categorías en formato XML.
		private static string indexFileName = "global_usage.xml";
		// Nombre de los fichero que contienen los dominios.
		private static string domainsFileName = "domains.xml";
		// Nombre de los fichero que contienen las urls.
		private static string urlsFileName = "urls.xml";
		// Nombre del directorio donde se crearon los ficheros XML.
		private static string xmlDestinationFolderName = "BL_XML";

		internal struct ForeignKeys
		{
			public string foreignKeyName;
			public string tableName;
		};

		internal enum sqlOperationList
		{
			preparingTransaction = 1,
			cleaningupAllData,
			updatingDatosGen,
			updatingCategories,
			updatingDomains,
			updatingUrls,
			committingTransaction,
			unknownOperation = 10
		}

		/// <summary>
		/// Current operation in execution.
		/// </summary>
		internal sqlOperationList currentSubOperation
		{
			get
			{
				// Get the currentOperation from the statusLogFile and delete the code bellow.
				if(_currentSubOperation == 0)
					_currentSubOperation = sqlOperationList.unknownOperation;
				return _currentSubOperation;
			}
			set
			{
				_currentSubOperation = value;
				clsLogProcessing.WriteToEventLog(string.Format("El estado de operaciones ha cambiado a: {0} -> {1}", operationList.updatingSqlDataBase.ToString(), _currentSubOperation.ToString()), EventLogEntryType.Information);
				// Try to free son memory every time the sub operation changes.
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
			}
		}

		internal bool updateSqlDataBase()
		{
			// Set the currentOperation
			clsSettings.serviceSettings.currentOperation = operationList.updatingSqlDataBase;

			ISACFDataContext db = new ISACFDataContext();
			try
			{
				if(clsSettings.serviceSettings.useSqlTransaction)
				{
					Decimal AvailablePhysicalMemoryMB = clsStatus.getAvailablePhysicalMemoryMB();
					if(AvailablePhysicalMemoryMB < 500.00M)
					{
						string Message = string.Format("El proceso \"{0}\" requiere que, en el momento de su ejecución, el valor de la memoria física disponible (RAM) " + 
							"(actualmente: {1} MB) sea mayor que {2} MB para llevar a cabo los procesos correspondientes en modo transacción. Por tanto, no se realizarán " +
							"las actualizaciones de la base de datos en modo de transacción en esta oportunidad. Si este mensaje aparece repetidamente en su sistema, es " +
							"posible que se requiera aumentar la memoria física total (RAM) del mismo o disminuir su carga de trabajo actual.", 
							operationList.updatingSqlDataBase, AvailablePhysicalMemoryMB, 500.00M);
						clsLogProcessing.WriteToEventLog(Message, EventLogEntryType.Warning);
					}
					else
					{
						currentSubOperation = sqlOperationList.preparingTransaction;
						db.Connection.Open();
						db.Transaction = db.Connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
					}
				}

				currentSubOperation = sqlOperationList.cleaningupAllData;
				cleanupAllData(db);

				currentSubOperation = sqlOperationList.updatingDatosGen;
				updateDatosGen(db);

				currentSubOperation = sqlOperationList.updatingCategories;
				updateCategories(db);

				currentSubOperation = sqlOperationList.updatingDomains;
				updateDomains(db);

				currentSubOperation = sqlOperationList.updatingUrls;
				updateUrls(db);

				if(db.Transaction != null)
				{
					currentSubOperation = sqlOperationList.committingTransaction;
					db.Transaction.Commit();
				}

			}
			catch(System.Exception Ex)
			{
				clsLogProcessing.WriteToEventLog(Ex.Message, System.Diagnostics.EventLogEntryType.Error);
				if(db != null)
				{
					if(db.Transaction != null)
					{
						db.Transaction.Rollback();
						db = null;
					}
				}
				return false;
			}
			finally
			{
				if(db.Connection.State == System.Data.ConnectionState.Open)
					db.Connection.Close();
				if(db != null)
					db = null;
				GC.Collect();
			}
			return true;
		}

		/// <summary>
		/// Clean up all the data for categorization and content filter.
		/// </summary>
		private static void cleanupAllData(ISACFDataContext db)
		{
			//ISACFDataContext db = new ISACFDataContext();
			object[] fakeParam = new object[0];
			if(db.Transaction == null & db.Connection.State == System.Data.ConnectionState.Closed)
				db.Connection.Open();

			db.ExecuteCommand("TRUNCATE TABLE ccfDatosGen", fakeParam);

			ForeignKeys[] foreignKeys = new ForeignKeys[4];
			foreignKeys[0].foreignKeyName = "FK_ccfCategoryName_es_ccfCategory";
			foreignKeys[0].tableName = "ccfCategoryName_es";
			foreignKeys[1].foreignKeyName = "FK_ccfDomain_ccfCategory";
			foreignKeys[1].tableName = "ccfDomain";
			foreignKeys[2].foreignKeyName = "FK_ccfUrl_ccfCategory";
			foreignKeys[2].tableName = "ccfUrl";
			foreignKeys[3].foreignKeyName = "FK_ccfIPv4_ccfCategory";
			foreignKeys[3].tableName = "ccfIPv4";

			for(int i = 0; i < foreignKeys.Length; ++i)
			{
				db.ExecuteCommand(string.Format("TRUNCATE TABLE {0}", foreignKeys[i].tableName), fakeParam);
			}

			// DROP Foreign Keys Constraints
			for(int i = 0; i < foreignKeys.Length; ++i)
			{
				string dropFKCommand = string.Format("IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[{0}]') AND parent_object_id = OBJECT_ID(N'[{1}]')) " +
					"ALTER TABLE [{1}] DROP CONSTRAINT [{0}]", foreignKeys[i].foreignKeyName, foreignKeys[i].tableName);
				db.ExecuteCommand(dropFKCommand, fakeParam);
			}
			// Now we can TRUNCATE the "ccfCategory" table.
			db.ExecuteCommand("TRUNCATE TABLE ccfCategory", fakeParam);

			// CREATE Foreign Keys Constraints
			for(int i = 0; i < foreignKeys.Length; ++i)
			{
				string dropFKCommand = string.Format("ALTER TABLE [{0}]  WITH CHECK ADD  CONSTRAINT [{1}] FOREIGN KEY([{2}]) " +
				"REFERENCES [{3}] ([ID]) " +
				"ON DELETE CASCADE " +
				"ALTER TABLE [{0}] CHECK CONSTRAINT [{1}]", foreignKeys[i].tableName, foreignKeys[i].foreignKeyName, "id_Category", "ccfCategory");
				db.ExecuteCommand(dropFKCommand, fakeParam);
			}

			fakeParam = null;

			if(db.Transaction == null & db.Connection.State == System.Data.ConnectionState.Open)
			{
				db.Connection.Close();
				db = null;
			}

			GC.Collect();
			return;
		}

		/// <summary>
		/// Update the general data: dateCreated, source.
		/// </summary>
		private static void updateDatosGen(ISACFDataContext db)
		{
			XDocument xDocumentIndex;
			ccfDatosGen dbDatosGen = new ccfDatosGen();

			FileInfo indexFile = new FileInfo(Path.Combine(clsSettings.serviceSettings.dataDirectory.FullName, string.Format("{0}{1}{2}", xmlDestinationFolderName, Path.DirectorySeparatorChar, indexFileName)));
			// Load the XML document containing the CategoryIndex.
			xDocumentIndex = XDocument.Load(indexFile.FullName);

			ccfDatosGen xmlDatosGen = new ccfDatosGen();
			xmlDatosGen = (from e in xDocumentIndex.Descendants("CategoryIndex")
										 where ((string)e.Attribute("CategoryIndex")) != string.Empty
										 select new ccfDatosGen
										 {
											 dateCreated = (DateTime)e.Attribute("DateCreated"),
											 source = (string)e.Attribute("source"),
										 }).Single();

			dbDatosGen.dateCreated = xmlDatosGen.dateCreated;
			dbDatosGen.source = xmlDatosGen.source;

			if(db.Transaction == null & db.Connection.State == System.Data.ConnectionState.Closed)
				db.Connection.Open();

			db.ccfDatosGens.InsertOnSubmit(dbDatosGen);
			db.SubmitChanges();

			if(db.Transaction == null & db.Connection.State == System.Data.ConnectionState.Open)
			{
				db.Connection.Close();
				db = null;
			}

			// Do some Cleanup.
			if(dbDatosGen != null)
				dbDatosGen = null;
			if(xDocumentIndex != null)
				xDocumentIndex = null;
			if(indexFile != null)
				indexFile = null;

			GC.Collect();
		}

		private static void updateCategories(ISACFDataContext db)
		{
			FileInfo indexFile = new FileInfo(Path.Combine(clsSettings.serviceSettings.dataDirectory.FullName, string.Format("{0}{1}{2}", xmlDestinationFolderName, Path.DirectorySeparatorChar, indexFileName)));
			XDocument xDocumentIndex;
			xDocumentIndex = XDocument.Load(indexFile.FullName);
			// Read only the Attribute name of each Category Element.
			var Categories = (from e in xDocumentIndex.Elements("CategoryIndex").Elements("Category")
												where ((string)e.Element("name")) != string.Empty
												select e).ToArray();

			if(db.Transaction == null & db.Connection.State == System.Data.ConnectionState.Closed)
				db.Connection.Open();

			var CategoriesToBlock = (from e in db.ccfCategoryToBlocks
															 select e.name).ToArray();

			ccfCategory newCat = null;
			ccfCategoryName_es newCatNameEs = null;
			foreach(XElement Category in Categories)
			{
				newCat = new ccfCategory();
				newCat.name = Category.Attribute("name").Value;
				newCat.default_type = Category.Element("default-type").Value;
				newCat.name_en = Category.Element("name-en").Value;
				newCat.desc_en = Category.Element("desc-en").Value;
				newCat.processForISARule = CategoriesToBlock.Contains(newCat.name);
				if(!string.IsNullOrEmpty(Category.Element("name-es").Value))
				{
					newCatNameEs = new ccfCategoryName_es();
					newCatNameEs.id_Category = newCat.ID;
					newCatNameEs.name_es = Category.Element("name-es").Value;
					newCatNameEs.desc_es = string.IsNullOrEmpty(Category.Element("desc-es").Value) ? DBNull.Value.ToString() : Category.Element("desc-es").Value;
					newCat.ccfCategoryName_es = newCatNameEs;
				}
				db.ccfCategories.InsertOnSubmit(newCat);
				// Do cleanup.
				if(newCatNameEs != null)
					newCatNameEs = null;
				if(newCat != null)
					newCat = null;
			}
			db.SubmitChanges();

			if(db.Transaction == null & db.Connection.State == System.Data.ConnectionState.Open)
			{
				db.Connection.Close();
				db = null;
			}

			// Do some Cleanup.
			if(indexFile != null)
				indexFile = null;
			if(xDocumentIndex != null)
				xDocumentIndex = null;
			if(Categories != null)
				Categories = null;
			if(CategoriesToBlock != null)
				CategoriesToBlock = null;

			GC.Collect();
		}

		private static void updateDomains(ISACFDataContext db)
		{
			if(db.Transaction == null & db.Connection.State == System.Data.ConnectionState.Closed)
				db.Connection.Open();

			List<ccfCategory> lstCategory = (from e in db.ccfCategories
																			 select e).ToList();

			if(db.Transaction == null & db.Connection.State == System.Data.ConnectionState.Open)
				db.Connection.Close();

			FileInfo DomainsFile = null;
			foreach(ccfCategory Category in lstCategory)
			{
				// Try to free some memory when Category changes.
				GC.Collect();
				DomainsFile = new FileInfo(Path.Combine(clsSettings.serviceSettings.dataDirectory.FullName, string.Format("{0}{1}{2}{1}{3}", xmlDestinationFolderName, Path.DirectorySeparatorChar, Category.name, domainsFileName)));
				if(!File.Exists(DomainsFile.FullName)) continue;

				bool onceAgain = true;
				int s = 0;
				const int rowsToTake = 5000;
				while(onceAgain)
				{
					List<string> domains = (from e in XDocument.Load(DomainsFile.FullName).Descendants("domain")
																	select (string)e.Value).Distinct().Skip(s).Take(rowsToTake).ToList();
					s += rowsToTake;
					onceAgain = domains.Count == rowsToTake;

					if(db == null)
						db = new ISACFDataContext();
					if(db.Transaction == null & db.Connection.State == System.Data.ConnectionState.Closed)
						db.Connection.Open();

					foreach(string domain in domains)
					{
						if(!isIPAddress(domain))
						{
							ccfDomain newDomain = new ccfDomain();
							newDomain.id_Category = Category.ID;
							newDomain.domain = domain;
							db.ccfDomains.InsertOnSubmit(newDomain);
							newDomain = null;
						}
						else
						{
							ccfIPv4 newIPv4 = new ccfIPv4();
							newIPv4.id_Category = Category.ID;
							newIPv4.IP = domain;
							db.ccfIPv4s.InsertOnSubmit(newIPv4);
							newIPv4 = null;
						}
					}
					db.SubmitChanges();

					// Do some Cleanup.
					if(db.Transaction == null & db.Connection.State == System.Data.ConnectionState.Open)
					{
						db.Connection.Close();
						db = null;
					}
					if(domains != null)
						domains = null;
					// Try to free some memory.
					GC.Collect();
				}

				// Do some Cleanup.
				if(DomainsFile != null)
					DomainsFile = null;
				// Try to free some memory.
				GC.Collect();
			}

			if(lstCategory != null)
				lstCategory = null;
			if(db != null)
			{
				if(db.Transaction == null & db.Connection.State == System.Data.ConnectionState.Open)
				{
					db.Connection.Close();
					db = null;
				}
			}
			// Try to free some memory.
			GC.Collect();
		} // End of updateDomains()

		private static void updateUrls(ISACFDataContext db)
		{
			if(db.Transaction == null & db.Connection.State == System.Data.ConnectionState.Closed)
				db.Connection.Open();

			List<ccfCategory> lstCategory = (from e in db.ccfCategories
																			 select e).ToList();

			if(db.Transaction == null & db.Connection.State == System.Data.ConnectionState.Open)
				db.Connection.Close();

			FileInfo UrlsFile = null;
			foreach(ccfCategory Category in lstCategory)
			{
				// Try to free some memory when Category changes.
				GC.Collect();
				UrlsFile = new FileInfo(Path.Combine(clsSettings.serviceSettings.dataDirectory.FullName, string.Format("{0}{1}{2}{1}{3}", xmlDestinationFolderName, Path.DirectorySeparatorChar, Category.name, urlsFileName)));
				if(!File.Exists(UrlsFile.FullName)) continue;

				bool onceAgain = true;
				int s = 0;
				const int rowsToTake = 5000;
				while(onceAgain)
				{
					List<string> urls = (from e in XDocument.Load(UrlsFile.FullName).Descendants("url")
															 select (string)e.Value).Distinct().Skip(s).Take(rowsToTake).ToList();
					s += rowsToTake;
					onceAgain = urls.Count == rowsToTake;

					if(db == null)
						db = new ISACFDataContext();
					if(db.Transaction == null & db.Connection.State == System.Data.ConnectionState.Closed)
						db.Connection.Open();

					foreach(string url in urls)
					{
						if(!isIPAddress(url))
						{
							ccfUrl newUrl = new ccfUrl();
							newUrl.id_Category = Category.ID;
							newUrl.url = url;
							db.ccfUrls.InsertOnSubmit(newUrl);
							newUrl = null;
						}
						else
						{
							ccfIPv4 newIPv4 = new ccfIPv4();
							newIPv4.id_Category = Category.ID;
							newIPv4.IP = url;
							db.ccfIPv4s.InsertOnSubmit(newIPv4);
							newIPv4 = null;
						}
					}
					db.SubmitChanges();

					// Do some Cleanup.
					if(db.Transaction == null & db.Connection.State == System.Data.ConnectionState.Open)
					{
						db.Connection.Close();
						db = null;
					}
					if(urls != null)
						urls = null;
					// Try to free some memory.
					GC.Collect();
				}

				// Do some Cleanup.
				if(UrlsFile != null)
					UrlsFile = null;
				// Try to free some memory.
				GC.Collect();
			}

			if(lstCategory != null)
				lstCategory = null;
			if(db != null)
			{
				if(db.Transaction == null & db.Connection.State == System.Data.ConnectionState.Open)
				{
					db.Connection.Close();
					db = null;
				}
			}
			// Try to free some memory.
			GC.Collect();
		} // End of updateUrls()

		private static bool isIPAddress(string value)
		{
			try
			{
				IPAddress address;
				return System.Net.IPAddress.TryParse(value, out address);
			}
			catch
			{
				return false;
			}
		}

	} // Fin de la clase.

} // Fin del namespace.
