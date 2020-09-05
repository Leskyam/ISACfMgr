using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.IO;
using System.Xml.Linq;
using System.Diagnostics;

namespace LAMSoft.ISACFMgr
{
	public partial class ISACFMgr : ServiceBase
	{
		/// <summary>
		/// Cantidad máxima de elementos a importar mediante el método importFromFile de los RuleElements.
		/// </summary>
		private const int maxAllowedRecordsImporting = 400000;

		private static FPCLib.FPC _oFPC;
		private static FPCLib.FPCArray _ISAServer;

		internal static FPCLib.FPCArray ISAServer
		{
			get
			{
				if(_ISAServer == null || _oFPC == null || _oFPC.Arrays.Count == 0)
				{
					_ISAServer = getFPCArray();
				}
				return _ISAServer;
			}
			set
			{
				if(value != null)
					throw new System.InvalidOperationException("El valor de \"ISAServer\" solo se puede establecer a null, de esta manera se desconecta del servidor.");
				if(_oFPC != value & _oFPC.Arrays.Count == 1)
				{
					try
					{
						// Disconnect from ISA server.
						_oFPC.Arrays.Disconnect(_ISAServer);
					}
					catch
					{
						; // No action is needed.
					}
				}
				// Cleanup ISA server object.
				if(_ISAServer != null)
					_ISAServer = null;
				if(_oFPC != null)
					_oFPC = null;
			}
		}

		internal enum RuleElementType
		{
			/// <summary>
			/// Computer (IP).
			/// </summary>
			Computer = 1,
			/// <summary>
			/// DomainNameSet.
			/// </summary>
			Domain,
			/// <summary>
			/// UrlSet.
			/// </summary>
			Url,
		};

		internal struct RuleElement
		{
			// Categoría (con sus respectivos datos) a la que ertenece el RuleElement.
			internal vw_ccfCategoryToBlock CategoryToBlock;
			// Tipo de RuleElement (Domain, Url ó Computer).
			internal RuleElementType ruleElementType;
			// Nombre asignado al RuleElement según la Categoría y el tipo RuleElementType al que pertenece.
			internal string ruleElementName;
			// Flag to know if the RuleElement was created on this processing execution.
			internal bool createdOnThisExecution;
			// Lista para los elementos (dominios, urls ó IPs) que existen ya registrados en el RuleElement que se está procesando.
			internal List<string> lstExistingDataInRuleElement;
			// Lista para los elementos (dominios, urls ó IPs) que existen en la base dedatos según el RuleElement que se está procesando.
			internal List<string> lstExistingDataInDB;
			// Lista para los elementos (dominios, urls ó IPs) que aparecen en el RuleElement pero ya no en la base de datos y por tanto son obsoletos.
			internal List<string> lstObsoleteDataInRuleElement;
			// Lista para los elementos (dominios, urls ó IPs) que deben ser agregados al RuleElement que se está procesando.
			internal List<string> lstNewData;
		}


		/// <summary>
		/// Conecta con el servidor ISA y devuelve el objeto FPCArray conectado.
		/// </summary>
		/// <returns></returns>
		private static FPCLib.FPCArray getFPCArray()
		{
			try
			{
				// Connect to ISA Server.
				_oFPC = new FPCLib.FPC(); // Inicializar la variable local privada para esta clase llamada "_oFPC";
				return _oFPC.Arrays.Connect(ServiceSettings.serviceSettings.ISAServer.Name,
																		ServiceSettings.serviceSettings.ISAServer.UserName,
																		ServiceSettings.serviceSettings.ISAServer.UserDomain,
																		ServiceSettings.serviceSettings.ISAServer.UserPasswd);
				//ISAServerName, ISAServerUserName, ISAServerUserDomain, ISAServerUserPasswd
			}
			catch(System.UnauthorizedAccessException ExAccessDenied) // Acceso Denegado.
			{
				string Message = string.Format("Imposible conectar con el servidor ISA \"{0}\". Revise los valores de los elementos " +
					"\"ISAServerUserName\", \"ISAServerUserPasswd\" y \"ISAServerUserDomain\" en el fichero de configuración \"app.config\"" + Environment.NewLine + 
					"Mensaje del error: {1}" + Environment.NewLine +
					"Otros detalles del error: " + Environment.NewLine +
					"Source: {2}" + Environment.NewLine +
					"TargetSite: {3}", ServiceSettings.serviceSettings.ISAServer.Name, ExAccessDenied.Message, ExAccessDenied.Source, ExAccessDenied.TargetSite);

				throw new System.UnauthorizedAccessException(Message, ExAccessDenied);
			}
			catch(System.Runtime.InteropServices.COMException ExRPC)
			{
				string Message = string.Format("Imposible conectar con el servidor ISA \"{0}\". Revise el valor del elemento \"ISAServerName\" " + 
					"en el fichero de configuración \"app.config\". " + Environment.NewLine + 
					"Mensaje del error: {1}" + Environment.NewLine +
					"Este error puede suceder debido a las siguientes causas: " + Environment.NewLine +
					"1. El nombre del servidor ISA \"{0}\" no es el correcto." + Environment.NewLine +
					"2. No hay conexión de red con el servidor ISA \"{0}\"" + Environment.NewLine +
					"3. El servicio \"Microsoft ISA Server Storage\" está detenido o no existe en el servidor ISA \"{0}\"" + Environment.NewLine +
					Environment.NewLine +
					"Otros detalles del error: " + Environment.NewLine +
					"Source: {2}" + Environment.NewLine +
					"TargetSite: {3}", ServiceSettings.serviceSettings.ISAServer.Name, ExRPC.Message, ExRPC.Source, ExRPC.TargetSite);

				throw new System.Runtime.InteropServices.COMException(Message, ExRPC);
			}
			catch(System.Exception Ex)
			{
				string Message = string.Format("Error no identificado al intentar conectar con el servidor ISA: \"{0}\". " + Environment.NewLine +
					"Descripción del error: {1} " + Environment.NewLine +
					Environment.NewLine +
					"Otros detalles del error: " + Environment.NewLine +
					"Source: {2}" + Environment.NewLine +
					"TargetSite: {3}", ServiceSettings.serviceSettings.ISAServer.Name, Ex.Message, Ex.Source, Ex.TargetSite);

				throw new System.Exception(Message, Ex);
			}
		}

		internal bool updateISARuleElements()
		{
			// Set the currentOperation
			ServiceSettings.serviceSettings.CurrentOperation = operationList.updatingISARuleElements;

			ISACFDataContext db = new ISACFDataContext();
			List<vw_ccfCategoryToBlock> CategoryToBlock = (from c in db.vw_ccfCategoryToBlocks
																										 where c.processForISARule == true
																										 orderby c.name
																										 select c).ToList();

			foreach(vw_ccfCategoryToBlock categoryToBlock in CategoryToBlock)
			{

				string statusMessage = string.Format("El estado de operaciones ha cambiado a: {0} -> Categoría: {1}" + Environment.NewLine + 
					" Elementos a procesar: " + Environment.NewLine + 
					"  Dominios (Domain Name Sets): {2}" + Environment.NewLine + 
					"  Urls (URL Sets): {3}" + Environment.NewLine + 
					"  IPs (Computer Sets): {4}", 
					ServiceSettings.serviceSettings.CurrentOperation, 
					categoryToBlock.name,
					categoryToBlock.processDomains?"Sí":"No",
					categoryToBlock.processUrls?"Sí":"No",
					categoryToBlock.processIPs?"Sí":"No");

				LogProcessing.WriteToEventLog(statusMessage, EventLogEntryType.Information);

				if(categoryToBlock.processDomains)
				{
					try
					{
						processRuleElement(categoryToBlock, RuleElementType.Domain);
					}
					catch(System.Exception Ex)
					{
						string errMessage = string.Format("Error procesando categoría \"{0}\" tipo de RuleElement \"{1}\"." + Environment.NewLine, categoryToBlock.name, RuleElementType.Domain);
						if(Ex.InnerException != null) // Ya los detalles vienen en pa propiedad Message.
						{
							// Proviene de un error tratado en otro bloque try-catch{} y tiene InnerException.
							errMessage += string.Format("Mensaje del error: {0}", Ex.Message);
						}
						else
						{
							errMessage += string.Format("Detalles del error: " + Environment.NewLine +
							"Mensaje: {0}. " + Environment.NewLine +
							"Source: {1}" + Environment.NewLine +
							"TargetSite: {2}", Ex.Message, Ex.Source, Ex.TargetSite);
						}
						LogProcessing.WriteToEventLog(errMessage, EventLogEntryType.Error);
					}
				}
				if(categoryToBlock.processUrls)
				{
					try
					{
						processRuleElement(categoryToBlock, RuleElementType.Url);
					}
					catch(System.Exception Ex)
					{
						string errMessage = string.Format("Error procesando categoría \"{0}\" tipo de RuleElement \"{1}\"." + Environment.NewLine, categoryToBlock.name, RuleElementType.Url);
						if(Ex.InnerException != null) // Ya los detalles vienen en pa propiedad Message.
						{
							// Proviene de un error tratado en otro bloque try-catch{} y tiene InnerException.
							errMessage += string.Format("Mensaje del error: {0}", Ex.Message);
						}
						else
						{
							errMessage += string.Format("Detalles del error: " + Environment.NewLine +
							"Mensaje: {0}. " + Environment.NewLine +
							"Source: {1}" + Environment.NewLine +
							"TargetSite: {2}", Ex.Message, Ex.Source, Ex.TargetSite);
						}
						LogProcessing.WriteToEventLog(errMessage, EventLogEntryType.Error);
					}
				}
				if(categoryToBlock.processIPs)
				{
					try
					{
						processRuleElement(categoryToBlock, RuleElementType.Computer);
					}
					catch(System.Exception Ex)
					{
						string errMessage = string.Format("Error procesando categoría \"{0}\" tipo de RuleElement \"{1}\"." + Environment.NewLine, categoryToBlock.name, RuleElementType.Computer);
						if(Ex.InnerException != null)
						{
							// Proviene de un error tratado en otro bloque try-catch{} y tiene InnerException.
							errMessage += string.Format("Mensaje del error: {0}", Ex.Message);
						}
						else
						{
							errMessage += string.Format("Detalles del error: " + Environment.NewLine +
							"Mensaje: {0}. " + Environment.NewLine +
							"Source: {1}" + Environment.NewLine +
							"TargetSite: {2}", Ex.Message, Ex.Source, Ex.TargetSite);
						}
						LogProcessing.WriteToEventLog(errMessage, EventLogEntryType.Error);
					}
				}

				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
			}

			ISAServer = null;
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			return true;
		}

		private static void processRuleElement(vw_ccfCategoryToBlock Category, RuleElementType ruleElementType)
		{
			// Obtener el nombre del RuleElement.
			string ruleElementName = getNameForRuleElement(Category.name, ruleElementType);
			// Lista para los elementos (dominios, urls ó IPs) que existen ya registrados en el RuleElement que se está procesando.
			List<string> lstExistingDataInRuleElement = new List<string>();
			// Lista para los elementos (dominios, urls ó IPs) que existen en la base dedatos según el RuleElement que se está procesando.
			List<string> lstExistingDataInDB = new List<string>();
			// Lista para los elementos (dominios, urls ó IPs) que aparecen en el RuleElement pero ya no en la base de datos y por tanto son obsoletos.
			List<string> lstObsoleteDataInRuleElement = new List<string>();
			// Lista para los elementos (dominios, urls ó IPs) que deben ser agregados al RuleElement que se está procesando.
			List<string> lstNewData = new List<string>();
			// Flag to know if the RuleElement is created in this execution.
			bool createdOnThisExecution = true;

			switch(ruleElementType)
			{
				case RuleElementType.Domain:				// Process the DomainNameSets
					{
						FPCLib.FPCDomainNameSet domNameSet = null;
						// Si existe el DomainNameSet a procesar establecerlo como valor de la variable domNameSet.
						foreach(FPCLib.FPCDomainNameSet d in ISAServer.RuleElements.DomainNameSets)
						{
							if(d.Name.ToLower() == ruleElementName.ToLower())
							{
								domNameSet = ISAServer.RuleElements.DomainNameSets.Item(ruleElementName);
								createdOnThisExecution = false;
								break;
							}
						}
						// Si domNameSet continúa siendo "null" es que no existía el DomainNameSet en el ISA Server, crearlo.
						if(domNameSet == null)
							domNameSet = ISAServer.RuleElements.DomainNameSets.Add(ruleElementName);
						else // Si existía el RuleElement llenar la lista con los datos de dominio que tenga contenidos en él.
						{
							for(int i = 1; i <= domNameSet.Count; ++i)
							{
								lstExistingDataInRuleElement.Add(domNameSet.Item(i));
							}
						}

						// Lista de los Dominios que vienen de la base de datos.
						ISACFDataContext db = new ISACFDataContext();
						lstExistingDataInDB = (from d in db.ccfDomains
																	 where d.id_Category == Category.ID
																	 select d.domain).Distinct().ToList();
						db = null; // Cleanup.

						// Lista de los que ya no aparecen como sitios bloqueados y sí en el DomainNameSet.
						lstObsoleteDataInRuleElement = (lstExistingDataInRuleElement.Except(lstExistingDataInDB)).ToList();
						// Eliminar del DomainNameSet y de la lista de datos existentes en este los dominios que ya son obsoletos.
						foreach(string obsoleteData in lstObsoleteDataInRuleElement)
						{
							lstExistingDataInRuleElement.Remove(obsoleteData);
							domNameSet.Remove(obsoleteData);
						}

						// Lista para los elementos (dominios, urls ó IPs) que deben ser agregados al RuleElement que se está procesando.
						lstNewData = (lstExistingDataInDB.Except(lstExistingDataInRuleElement)).ToList();

						// Si el DomainNameSet no contiene dominios y tampoco aparecen nuevos en la base de datos para 
						// él, entonces poner una descripción correspondiente porque más adelante en este mismo método 
						// será descartado y no se procesará más.
						if((lstExistingDataInRuleElement.Count == 0) & (lstNewData.Count == 0))
							domNameSet.Description = string.Format("{1}{0} Id de Categoría: {2}{0} Actualizado: {3}{0} Cantidad total: Ninguno{0} Agregados: Ninguno{0} Eliminados: {4}",
								Environment.NewLine, Category.desc_en, Category.ID, DateTime.Now.ToString(), lstObsoleteDataInRuleElement.Count == 0 ? "Ninguno" : lstObsoleteDataInRuleElement.Count.ToString());

						// Salvar el DomainNameSet ya que se han hecho todas las operaciones sobre el mismo.
						domNameSet.Save(false, false);

						// Do some cleanup.
						if(domNameSet != null)
						{
							System.Runtime.InteropServices.Marshal.ReleaseComObject(domNameSet);
							domNameSet = null;
						}

						break; // Fin del RuleElementType.Domain
					}
				case RuleElementType.Url: 				// Process the URLSets
					{
						FPCLib.FPCURLSet urlSet = null;
						// Si existe el URLSet a procesar establecerlo como valor de la variable urlSet.
						foreach(FPCLib.FPCURLSet d in ISAServer.RuleElements.URLSets)
						{
							if(d.Name.ToLower() == ruleElementName.ToLower())
							{
								urlSet = ISAServer.RuleElements.URLSets.Item(ruleElementName);
								break;
							}
						}
						// Si urlSet continúa siendo "null" es que no existía el URLSet en el ISA Server, crearlo.
						if(urlSet == null)
							urlSet = ISAServer.RuleElements.URLSets.Add(ruleElementName);
						else // Si existía el RuleElement llenar la lista con los datos de url que tenga contenidos en él.
						{
							for(int i = 1; i <= urlSet.Count; ++i)
							{
								lstExistingDataInRuleElement.Add(urlSet.Item(i));
							}
						}

						// Lista de los urls que vienen de la base de datos.
						// the same query: db.ccfUrls.Where((u) => u.id_Category == Category.ID).Select((u) => u.url).Distinct().ToList();
						ISACFDataContext db = new ISACFDataContext();
						lstExistingDataInDB = (from u in db.ccfUrls
																	 where u.id_Category == Category.ID
																	 select u.url).Distinct().ToList();
						db = null; // Cleanup.

						// Lista de los que ya no aparecen como sitios bloqueados pero todavía están en el URLSet.
						lstObsoleteDataInRuleElement = (lstExistingDataInRuleElement.Except(lstExistingDataInDB)).ToList();
						// Eliminar del DomainNameSet y de la lista de datos existentes en este los dominios que ya son obsoletos.
						foreach(string obsoleteData in lstObsoleteDataInRuleElement)
						{
							lstExistingDataInRuleElement.Remove(obsoleteData);
							urlSet.Remove(obsoleteData);
						}

						// Lista para los elementos (dominios, urls ó IPs) que deben ser agregados al RuleElement que se está procesando.
						lstNewData = (lstExistingDataInDB.Except(lstExistingDataInRuleElement)).ToList();

						// Si el URLSet no contiene urls y tampoco aparecen nuevas en la base de datos para él, 
						// entonces poner una descripción correspondiente porque más adelante en este mismo método 
						// será descartado y no se procesará más.
						if((lstExistingDataInRuleElement.Count == 0) & (lstNewData.Count == 0))
							urlSet.Description = string.Format("{1}{0} Id de Categoría: {2}{0} Actualizado: {3}{0} Cantidad total: Ninguno{0} Agregados: Ninguno{0} Eliminados: {4}",
								Environment.NewLine, Category.desc_en, Category.ID, DateTime.Now.ToString(), lstObsoleteDataInRuleElement.Count == 0 ? "Ninguno" : lstObsoleteDataInRuleElement.Count.ToString());

						// Salvar el URLSet ya que se han hecho todas las operaciones sobre el mismo.
						urlSet.Save(false, false);

						// Do some cleanup.
						if(urlSet != null)
						{
							System.Runtime.InteropServices.Marshal.ReleaseComObject(urlSet);
							urlSet = null;
						}

						break; // Fin del RuleElementType.Url
					}
				// Process the IPs (Computers)
				case RuleElementType.Computer:
					{
						FPCLib.FPCComputerSet computerSet = null;
						// Si existe el URLSet a procesar establecerlo como valor de la variable urlSet.
						foreach(FPCLib.FPCComputerSet c in ISAServer.RuleElements.ComputerSets)
						{
							if(c.Name.ToLower() == ruleElementName.ToLower())
							{
								computerSet = ISAServer.RuleElements.ComputerSets.Item(ruleElementName);
								break;
							}
						}
						// Si urlSet continúa siendo "null" es que no existía el URLSet en el ISA Server, crearlo.
						if(computerSet == null)
							computerSet = ISAServer.RuleElements.ComputerSets.Add(ruleElementName);
						else // Si existía el RuleElement llenar la lista con los datos de url que tenga contenidos en él.
						{
							for(int i = 1; i <= computerSet.Computers.Count; ++i)
							{
								lstExistingDataInRuleElement.Add(computerSet.Computers.Item(i).IPAddress);
							}
						}

						// Lista de los urls que vienen de la base de datos.
						// the same query: db.ccfUrls.Where((u) => u.id_Category == Category.ID).Select((u) => u.url).Distinct().ToList();
						ISACFDataContext db = new ISACFDataContext();
						lstExistingDataInDB = (from c in db.ccfIPv4s
																	 where c.id_Category == Category.ID
																	 select c.IP).Distinct().ToList();
						db = null; // Cleanup.

						// Lista de los que ya no aparecen como sitios bloqueados pero todavía están en el URLSet.
						lstObsoleteDataInRuleElement = (lstExistingDataInRuleElement.Except(lstExistingDataInDB)).ToList();
						// Eliminar del DomainNameSet y de la lista de datos existentes en este los dominios que ya son obsoletos.
						foreach(string obsoleteData in lstObsoleteDataInRuleElement)
						{
							lstExistingDataInRuleElement.Remove(obsoleteData);
							computerSet.Computers.Remove(obsoleteData);
						}

						// Lista para los elementos (dominios, urls ó IPs) que deben ser agregados al RuleElement que se está procesando.
						lstNewData = (lstExistingDataInDB.Except(lstExistingDataInRuleElement)).ToList();

						// Si el URLSet no contiene urls y tampoco aparecen nuevas en la base de datos para él, 
						// entonces poner una descripción correspondiente porque más adelante en este mismo método 
						// será descartado y no se procesará más.
						if((lstExistingDataInRuleElement.Count == 0) & (lstNewData.Count == 0))
							computerSet.Description = string.Format("{1}{0} Id de Categoría: {2}{0} Actualizado: {3}{0} Cantidad total: Ninguno{0} Agregados: Ninguno{0} Eliminados: {4}",
								Environment.NewLine, Category.desc_en, Category.ID, DateTime.Now.ToString(), lstObsoleteDataInRuleElement.Count == 0 ? "Ninguno" : lstObsoleteDataInRuleElement.Count.ToString());

						// Salvar el URLSet ya que se han hecho todas las operaciones sobre el mismo.
						computerSet.Save(false, false);

						// Do some cleanup.
						if(computerSet != null)
						{
							System.Runtime.InteropServices.Marshal.ReleaseComObject(computerSet);
							computerSet = null;
						}

						break; // Fin del RuleElementType.Url
					}
				default:
					throw new System.InvalidOperationException("Las operaciones solo son válidas para una de las opciones del tipo \"RuleElementType\".");
			}

			// Dejar de procesar si no hay nuevos elementos en la base de datos que deban ser agragados al RuleElement.
			if(lstNewData.Count == 0)
			{
				// No hay nada mas que hacer, estos ya se procesaron más arriba.
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				return;
			}

			RuleElement preprocessedRuleElement;
			preprocessedRuleElement.CategoryToBlock = Category;
			preprocessedRuleElement.ruleElementType = ruleElementType;
			preprocessedRuleElement.ruleElementName = ruleElementName;
			preprocessedRuleElement.createdOnThisExecution = createdOnThisExecution;
			preprocessedRuleElement.lstExistingDataInRuleElement = lstExistingDataInRuleElement;
			preprocessedRuleElement.lstExistingDataInDB = lstExistingDataInDB;
			preprocessedRuleElement.lstObsoleteDataInRuleElement = lstObsoleteDataInRuleElement;
			preprocessedRuleElement.lstNewData = lstNewData;

			switch(ruleElementType)
			{
				case RuleElementType.Domain:
				case RuleElementType.Url:
					{
						// DECIDIR QUÉ MÉTODO ES MÁS CONVENIENTE
						// Seleccionar el método apropiado (Add || ImportFromFile) para actualizar los datos del RuleElement.
						if((lstExistingDataInRuleElement.Count == 0 & lstNewData.Count > 0) || ((lstExistingDataInRuleElement.Count < maxAllowedRecordsImporting) & (lstExistingDataInRuleElement.Count + lstNewData.Count > 75000)))
						{
							//string Message = string.Format("Método \"ImportFromFile\" para: {0} con {1} dominios existentes y {2} a importar.", ruleElementName, lstExistingDataInRuleElement.Count, lstNewData.Count);
							//Debug.WriteLine(Message);
							//Console.WriteLine(Message);
							// Use the RiuleElement's ImportFromFile method.
							importFromFile(preprocessedRuleElement);
						}
						else
						{
							//string Message = string.Format("Método \"Add\" para: {0} con {1} dominios existentes y {2} a agregar.", ruleElementName, lstExistingDataInRuleElement.Count, lstNewData.Count);
							//Debug.WriteLine(Message);
							//Console.WriteLine(Message);
							// Use the RiuleElement's Add method.
							addDataToRuleElement(preprocessedRuleElement);
						}

						break;
					}
				case RuleElementType.Computer:
					{
						//string Message = string.Format("Método \"Add\" para: {0} con {1} dominios existentes y {2} a agregar.", ruleElementName, lstExistingDataInRuleElement.Count, lstNewData.Count);
						//Debug.WriteLine(Message);
						//Console.WriteLine(Message);
						// Use the RiuleElement's Add method.
						addDataToRuleElement(preprocessedRuleElement);

						break;
					}
				default:
					throw new System.InvalidOperationException("Las operaciones solo son válidas para una de las opciones del tipo \"RuleElementType\".");
			}

			// Cleanup.
			lstExistingDataInRuleElement = null;
			lstExistingDataInDB = null;
			lstObsoleteDataInRuleElement = null;
			lstNewData = null;
			// Try to free some memory.
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			return;
		}

		private static void addDataToRuleElement(RuleElement ruleElement)
		{
			// Contador para saber la cantidad real de elementos que se han agregado, para poner tope a los elementos 
			// que es considerado eficiente agregar (75000) de una vez y porque en caso de error al agregar la rutina 
			// continúa agregando el siguiente y se pierde la cantidad real de elementos que se han procesado.
			int importedDataCounter = 0;
			// Hora a la que comienza la rutina porque también hay que controlar por tiempo, para no permitir que se
			// inviertan más de 15 minutos en un RuleElement, si llega a este tiempo agregando los elementos de un 
			// RuleElement es que debe tener muchos elementos ya registrados y es muy lento procesar más, así que los 
			// que falten se irán procesando poco a poco con cada ejecución (actualización) del sistema.
			DateTime beginTime = DateTime.Now;
			// Tiempo máximo (en minutos) que se mantendrán en ejecución las operaciones Add para el RuleElement dado.
			const int timeIsOver = 10;
			// Cantidad máxima de valores (Dominios, Urls, IPs) que es "sano" importar por este médodo.
			const int maxRecordsToAdd = 75000;

			switch(ruleElement.ruleElementType)
			{
				case RuleElementType.Domain:
					{
						FPCLib.FPCDomainNameSet domNameSet = ISAServer.RuleElements.DomainNameSets.Item(ruleElement.ruleElementName);
						foreach(var dom in ruleElement.lstNewData)
						{
							try
							{
								domNameSet.Add(dom);
								// Agregar más de 75000 elementos comienza a afectar considerablemente el rendimiento, 
								// pero también hay que controlar el tiempo porque aún sin llegar a los 75000 hay RuleElements 
								// que contienen muchos registros y agregar nuevos se vuelve tremendamente lento.
								if((++importedDataCounter >= 75000) || ((DateTime.Now - beginTime).TotalMinutes >= timeIsOver))
									break;
							}
							catch
							{
								continue;
							}
						}
						domNameSet.Description = string.Format("{1}{0} Id de Categoría: {2}{0} Actualizado: {3}{0} Cantidad total: {4}{0} Agregados: {5}{0} Eliminados: {6}",
							Environment.NewLine, ruleElement.CategoryToBlock.desc_en, ruleElement.CategoryToBlock.ID, DateTime.Now.ToString(), ruleElement.lstExistingDataInRuleElement.Count + importedDataCounter,
							importedDataCounter == 0 ? "Ninguno" : importedDataCounter.ToString(), ruleElement.lstObsoleteDataInRuleElement.Count == 0 ? "Ninguno" : ruleElement.lstObsoleteDataInRuleElement.Count.ToString());
						try
						{
							domNameSet.Save(false, false);
						}
						catch(System.Exception Ex)
						{
							//Process Error Here and continue;
							string errMessage = string.Format("Error en el método \"addDataToRuleElement\" mientras se procesaba el elemento \"{0}\". Mensaje del error: {1}",
								ruleElement.ruleElementName, Ex.Message);
							LogProcessing.WriteToEventLog(errMessage, EventLogEntryType.Error);
						}

						// Do some cleanup.
						if(domNameSet != null)
						{
							System.Runtime.InteropServices.Marshal.ReleaseComObject(domNameSet);
							domNameSet = null;
						}

						break;
					}
				case RuleElementType.Url:
					{
						FPCLib.FPCURLSet urlSet = ISAServer.RuleElements.URLSets.Item(ruleElement.ruleElementName);
						foreach(var url in ruleElement.lstNewData)
						{
							try
							{
								urlSet.Add(url);
								// Agregar más de 75000 elementos comienza a afectar considerablemente el rendimiento, 
								// pero también hay que controlar el tiempo porque aún sin llegar a los 75000 hay RuleElements 
								// que contienen muchos registros y agregar nuevos se vuelve tremendamente lento.
								if((++importedDataCounter >= maxRecordsToAdd) || ((DateTime.Now - beginTime).TotalMinutes >= timeIsOver))
									break;
							}
							catch
							{
								continue;
							}
						}
						urlSet.Description = string.Format("{1}{0} Id de Categoría: {2}{0} Actualizado: {3}{0} Cantidad total: {4}{0} Agregados: {5}{0} Eliminados: {6}",
							Environment.NewLine, ruleElement.CategoryToBlock.desc_en, ruleElement.CategoryToBlock.ID, DateTime.Now.ToString(), ruleElement.lstExistingDataInRuleElement.Count + importedDataCounter,
							importedDataCounter == 0 ? "Ninguno" : importedDataCounter.ToString(), ruleElement.lstObsoleteDataInRuleElement.Count == 0 ? "Ninguno" : ruleElement.lstObsoleteDataInRuleElement.Count.ToString());
						try
						{
							urlSet.Save(false, false);
						}
						catch(System.Exception Ex)
						{
							//Process Error Here and continue;
							string errMessage = string.Format("Error en el método \"addDataToRuleElement\" mientras se procesaba el elemento \"{0}\". Mensaje del error: {1}",
								ruleElement.ruleElementName, Ex.Message);
							LogProcessing.WriteToEventLog(errMessage, EventLogEntryType.Error);
						}

						// Do some cleanup.
						if(urlSet != null)
						{
							System.Runtime.InteropServices.Marshal.ReleaseComObject(urlSet);
							urlSet = null;
						}

						break;
					}
				case RuleElementType.Computer:
					{
						FPCLib.FPCComputerSet computerSet = ISAServer.RuleElements.ComputerSets.Item(ruleElement.ruleElementName);
						foreach(var computer in ruleElement.lstNewData)
						{
							try
							{
								computerSet.Computers.Add(computer, computer);
								// Agregar más de 75000 elementos comienza a afectar considerablemente el rendimiento, 
								// pero también hay que controlar el tiempo porque aún sin llegar a los 75000 hay RuleElements 
								// que contienen muchos registros y agregar nuevos se vuelve tremendamente lento.
								if((++importedDataCounter >= maxRecordsToAdd) || ((DateTime.Now - beginTime).TotalMinutes >= timeIsOver))
									break;
							}
							catch
							{
								continue;
							}
						}
						computerSet.Description = string.Format("{1}{0} Id de Categoría: {2}{0} Actualizado: {3}{0} Cantidad total: {4}{0} Agregados: {5}{0} Eliminados: {6}",
							Environment.NewLine, ruleElement.CategoryToBlock.desc_en, ruleElement.CategoryToBlock.ID, DateTime.Now.ToString(), ruleElement.lstExistingDataInRuleElement.Count + importedDataCounter,
							importedDataCounter == 0 ? "Ninguno" : importedDataCounter.ToString(), ruleElement.lstObsoleteDataInRuleElement.Count == 0 ? "Ninguno" : ruleElement.lstObsoleteDataInRuleElement.Count.ToString());
						try
						{
							computerSet.Save(false, false);
						}
						catch(System.Exception Ex)
						{
							//Process Error Here and continue;
							string errMessage = string.Format("Error en el método \"addDataToRuleElement\" mientras se procesaba el elemento \"{0}\". Mensaje del error: {1}",
								ruleElement.ruleElementName, Ex.Message);
							LogProcessing.WriteToEventLog(errMessage, EventLogEntryType.Error);
						}

						// Do some cleanup.
						if(computerSet != null)
						{
							System.Runtime.InteropServices.Marshal.ReleaseComObject(computerSet);
							computerSet = null;
						}

						break;
					}
				default:
					{
						throw new System.NotImplementedException(string.Format("Tipo de RuleElement \"{0}\" desconocido, el método \"AddDataToRuleElement\" no implementa código para procesarlo.", ruleElement.ruleElementType.ToString()));
					}
			}

		} // Fin del método addDataToRuleElement()

		private static void importFromFile(RuleElement ruleElement)
		{
			// Este método tiene dos limitantes:
			// 1. No es aplicable cuando el DomainNameSet en cuestión no tiene elementos "domain" en lista. (Resuelto)
			// 2. Al importar se eliminan los existentes y se importan solmente los que se definan aquí.

			// Contador para saber la cantidad real de elementos que se importan.
			int importedDataCounter = 0;
			// Cantidad de registros que se van a importar.
			int recordsToImport = maxAllowedRecordsImporting - ruleElement.lstExistingDataInRuleElement.Count;
			// Camino donde se encontrará el fichero XML con el que se va a trabajar.
			string xmlFileName = Path.Combine(ServiceSettings.serviceSettings.DataDirectory.FullName, ruleElement.ruleElementName + ".xml");

			switch(ruleElement.ruleElementType)
			{
				case RuleElementType.Domain:
					{
						FPCLib.FPCDomainNameSet domNameSet = ISAServer.RuleElements.DomainNameSets.Item(ruleElement.ruleElementName);
						// Comprobar si es nuevo y crear al menos un registro antes de exportar para poder obtener 
						// los objetos XNamespace necesarios ya que si no existe ningún registro no aparecen muestras
						// para poder determinar qué XNamespace utiliza el elemento XML "DomainNameStrings".
						if(ruleElement.createdOnThisExecution || ruleElement.lstExistingDataInRuleElement.Count == 0)
						{
							domNameSet.Add("not-existing-domain.com");
							domNameSet.Description = string.Format("Descripción temporal. {0} está en preparación para importar contenido desde fichero: {1}", ruleElement.ruleElementName, xmlFileName);
							domNameSet.Save(false, false);
						}
						domNameSet.ExportToFile(xmlFileName, 0);

						XDocument xDoc = XDocument.Load(xmlFileName); // OLD CODE: AppDomain.CurrentDomain.BaseDirectory + domNameSet.Name + ".xml"
						XElement xDomainNameStrings = xDoc.Descendants(xDoc.Root.Name.Namespace + "DomainNameStrings").Single();

						foreach(string e in ruleElement.lstNewData)
						{
							XElement newXElement = new XElement(xDomainNameStrings.Elements().First().Name, e);
							foreach(XAttribute xAtt in xDomainNameStrings.Elements().First().Attributes())
							{
								newXElement.SetAttributeValue(xAtt.Name, xAtt.Value);
							}
							xDomainNameStrings.Add(newXElement);

							if(++importedDataCounter >= recordsToImport) // Evitar que se pase de maxAllowedRecordsImporting la lista de dominios a importar.
								break;
						}
						string ruleElementDescription = string.Format("{1}{0} Id de Categoría: {2}{0} Actualizado: {3}{0} Cantidad total: {4}{0} Agregados: {5}{0} Eliminados: {6}",
																				Environment.NewLine, ruleElement.CategoryToBlock.desc_en, ruleElement.CategoryToBlock.ID, DateTime.Now.ToString(),
																				ruleElement.lstExistingDataInRuleElement.Count + importedDataCounter, importedDataCounter == 0 ? "Ninguno" : importedDataCounter.ToString(),
																				ruleElement.lstObsoleteDataInRuleElement.Count == 0 ? "Ninguno" : ruleElement.lstObsoleteDataInRuleElement.Count.ToString());
						if(importedDataCounter >= recordsToImport)
						{
							// Si estamos aquí es porque llegamos al tope (maxAllowedRecordsImporting) de los elementos permitidos a importar por este método, notificarlo en la descipción del RuleElement.
							string note = string.Format(Environment.NewLine + "NOTA." + Environment.NewLine +
								"Aparentemente no se han importado todos los Dominios que aparecen en la base de datos, el límite para esta operación es de {0} elementos, " +
								"no se requiere ninguna acción por su parte, el sistema agregará los Dominios restantes en cada ejecución (actualización) que se efectúe.", maxAllowedRecordsImporting);
							ruleElementDescription += note;
						}

						XElement xDescription = xDoc.Descendants(xDoc.Root.Name.Namespace + "Description").Single();
						xDescription.SetValue(ruleElementDescription);
						xDoc.Save(xmlFileName, SaveOptions.None);
						try
						{
							domNameSet.ImportFromFile(xmlFileName, 0);
						}
						catch(Exception Ex)
						{
							//Process Error Here and continue;
							string errMessage = string.Format("Error en el método \"importFromFile\" mientras se procesaba el elemento \"{0}\". Mensaje del error: {1}",
								ruleElement.ruleElementName, Ex.Message);
							LogProcessing.WriteToEventLog(errMessage, EventLogEntryType.Error);
						}
						// Remover el dominio temporal utilizado para obtener el XNamespace correcto para los XElement DomainNameStrings.
						if(ruleElement.createdOnThisExecution || ruleElement.lstExistingDataInRuleElement.Count == 0)
						{
							try
							{
								domNameSet.Remove("not-existing-domain.com");
								domNameSet.Save(false, false);
							}
							catch
							{
								; // Nothing else to do.
							}
						}

						// Do some cleanup.
						if(domNameSet != null)
						{
							System.Runtime.InteropServices.Marshal.ReleaseComObject(domNameSet);
							domNameSet = null;
						}

						break;
					}
				case RuleElementType.Url:
					{
						FPCLib.FPCURLSet urlSet = ISAServer.RuleElements.URLSets.Item(ruleElement.ruleElementName);
						// Comprobar si es nuevo y crear al menos un registro antes de exportar para poder obtener 
						// los objetos XNamespace necesarios ya que si no existe ningún registro no aparecen muestras
						// para poder determinar qué XNamespace utiliza el elemento XML "URLStrings".
						if(ruleElement.createdOnThisExecution || ruleElement.lstExistingDataInRuleElement.Count == 0)
						{
							urlSet.Add("not-existing-url.com/");
							urlSet.Description = string.Format("Descripción temporal. {0} está en preparación para importar contenido desde fichero: {1}", ruleElement.ruleElementName, xmlFileName);
							urlSet.Save(false, false);
						}
						urlSet.ExportToFile(xmlFileName, 0);

						XDocument xDoc = XDocument.Load(xmlFileName); // OLD CODE: AppDomain.CurrentDomain.BaseDirectory + urlSet.Name + ".xml"

						XElement xDomainNameStrings = xDoc.Descendants(xDoc.Root.Name.Namespace + "URLStrings").Single();
						foreach(string e in ruleElement.lstNewData)
						{
							XElement newXElement = new XElement(xDomainNameStrings.Elements().First().Name, e);
							foreach(XAttribute xAtt in xDomainNameStrings.Elements().First().Attributes())
							{
								newXElement.SetAttributeValue(xAtt.Name, xAtt.Value);
							}
							xDomainNameStrings.Add(newXElement);

							if(++importedDataCounter >= maxAllowedRecordsImporting) // Evitar que se pase de maxAllowedRecordsImporting la lista de urls a importar.
								break;
						}
						string ruleElementDescription = string.Format("{1}{0} Id de Categoría: {2}{0} Actualizado: {3}{0} Cantidad total: {4}{0} Agregados: {5}{0} Eliminados: {6}",
																				Environment.NewLine, ruleElement.CategoryToBlock.desc_en, ruleElement.CategoryToBlock.ID, DateTime.Now.ToString(),
																				ruleElement.lstExistingDataInRuleElement.Count + importedDataCounter, importedDataCounter == 0 ? "Ninguno" : importedDataCounter.ToString(),
																				ruleElement.lstObsoleteDataInRuleElement.Count == 0 ? "Ninguno" : ruleElement.lstObsoleteDataInRuleElement.Count.ToString());
						if(importedDataCounter >= maxAllowedRecordsImporting)
						{
							// Si estamos aquí es porque llegamos al tope (maxAllowedRecordsImporting) de los elementos permitidos a importar por este método, notificarlo en la descipción del RuleElement.
							string note = string.Format(Environment.NewLine + "NOTA." + Environment.NewLine +
								"Aparentemente no se han importado todas las urls que aparecen en lista, el límite para esta operación es de {0} elementos, usted puede importar el fichero {0} " +
								"manualmente a través de la consola de administración del servidor ISA. De lo contrario el sistema agregará las urls restantes en cada ejecución (actualización) " +
								"que se efectúe.", maxAllowedRecordsImporting);
							ruleElementDescription += note;
						}

						XElement xDescription = xDoc.Descendants(xDoc.Root.Name.Namespace + "Description").Single();
						xDescription.SetValue(ruleElementDescription);
						xDoc.Save(xmlFileName, SaveOptions.None);
						
						try
						{
							urlSet.ImportFromFile(xmlFileName, 0);
						}
						catch(Exception Ex)
						{
							//Process Error Here and continue;
							string errMessage = string.Format("Error en el método \"importFromFile\" mientras se procesaba el elemento \"{0}\". Mensaje del error: {1}",
								ruleElement.ruleElementName, Ex.Message);
							LogProcessing.WriteToEventLog(errMessage, EventLogEntryType.Error);
						}
						// Remover el dominio temporal utilizado para obtener el XNamespace correcto para los XElement DomainNameStrings.
						if(ruleElement.createdOnThisExecution || ruleElement.lstExistingDataInRuleElement.Count == 0)
						{
							try
							{
								urlSet.Remove("not-existing-url.com/");
								urlSet.Save(false, false);
							}
							catch
							{
								; // Nothing else to do.
							}
						}

						// Do some cleanup.
						if(urlSet != null)
						{
							System.Runtime.InteropServices.Marshal.ReleaseComObject(urlSet);
							urlSet = null;
						}

						break;
					}
				case RuleElementType.Computer:
					{
						throw new System.NotImplementedException(string.Format("El método \"importFromFile\" no implementa código para procesar los RuleElement del tipo {0}.", RuleElementType.Computer.ToString()));
					}
				default:
					{
						throw new System.NotImplementedException(string.Format("Tipo de RuleElement \"{0}\" desconocido, el método \"importFromFile\" no implementa código para procesarlo.", ruleElement.ruleElementType.ToString()));
					}
			}

			// Eliminar el fichero creado si existe todavía.
			if(File.Exists(xmlFileName))
				File.Delete(xmlFileName);

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
		}

		/// <summary>
		/// Obtener el nombre para el RuleElement según los valores de los parámetros pasados.
		/// </summary>
		/// <param name="categotyName">Nombre de la categoría que se va a procesar.</param>
		/// <param name="type">Tipo de RuleElement (Domain ->DomainNameSet, Url -> URLSet ó Computer -> ComputerSet).</param>
		/// <returns>Nombre del RuleElement.</returns>
		private static string getNameForRuleElement(string categotyName, RuleElementType type)
		{
			// Ejemplo: ccf_Dom_porn para "DomainNameSet de la categoría 'porn'"
			//					ccf_Url_hobby_games-online "URLSet de la categoría 'hobby/games-online'"
			return string.Format("ccf_{0}_{1}", type.ToString(), categotyName.Replace('/', '_').ToUpper());
		}
		

	} // Fin de la clase.

} // Fin del namespace.
