using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Tar;
using System.Xml;
using System.Xml.Linq;

namespace LAMSoft.ISACFMgr
{
	/// <summary>
	/// Partial class for source file content decompressing and transformation to XML format (pclsTransformSource).
	/// </summary>
	public partial class ISACFMgr : ServiceBase
	{
		/// <summary>
		/// Decompress the source file containing the categories and the content of the blacklisted sites.
		/// </summary>
		/// <returns>True if decompressing process success efectively, otherwise false.</returns>
		private bool decompressSourceFile()
		{
			// Set the currentOperation to "decompressingSourceFile"
			clsSettings.serviceSettings.currentOperation = operationList.decompressingSourceFile;

			bool result = false;

			// Form the FileInfo variable with the file to decompress.
			FileInfo fileInfo = new FileInfo(string.Format("{0}{1}", clsSettings.serviceSettings.dataDirectory, clsSettings.serviceSettings.downloadedFileName));
			if(!fileInfo.Exists)
			{
				clsSettings.serviceSettings.currentOperation = operationList.waitingForNextExecution;
				clsLogProcessing.WriteToEventLog(string.Format("El estado de operaciones ha cambiado a \"{0}\", porque durante la " +
					"operación \"{1}\" no se encontró el recurso (fichero) \"{2}\" como se esperaba, para ejecutar la descompresión " +
					"del mismo. Si esta es la primera vez que observa este error no se requiere intervención alguna por parte del usuario. " +
					"El sistema se encargará de procesar todas las operaciones desde el principio para obtener el fichero correspondiente.",
					operationList.waitingForNextExecution, operationList.decompressingSourceFile, fileInfo.FullName), EventLogEntryType.Error);
				return false;
			}

			Stream inStream = File.OpenRead(fileInfo.FullName);
			// Extract
			try
			{
				// Extract TAR, GZipped File.
				TarArchive tarGZippedArchive = TarArchive.CreateInputTarArchive(new GZipStream(inStream, CompressionMode.Decompress));
				tarGZippedArchive.ExtractContents(clsSettings.serviceSettings.dataDirectory.FullName);
				// Do some cleanup.
				tarGZippedArchive.Close();
				if(tarGZippedArchive != null)
					tarGZippedArchive = null;
				
				result = true;
			}
			catch(System.Exception)
			{
				// Sometimes the file is opened weith decompressing software (sach as WinRar) and from that momento it's not a GZipped file anymore.
				// Extract TAR File
				/* THIS DOES NOT WORK.
				TarArchive tarArchive = TarArchive.CreateInputTarArchive(inStream);
				tarArchive.ExtractContents(clsSettings.serviceSettings.dataDirectory.FullName);
				tarArchive.Close();
				*/
				using(FileStream fsIn = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
				{

					// The TarInputStream reads a UNIX tar archive as an InputStream.
					TarInputStream tarIn = new TarInputStream(fsIn);

					TarEntry tarEntry;

					while((tarEntry = tarIn.GetNextEntry()) != null)
					{
						if(tarEntry.IsDirectory)
						{
							continue;
						}
						// Converts the unix forward slashes in the filenames to windows backslashes.
						string name = tarEntry.Name.Replace('/', Path.DirectorySeparatorChar);

						// Apply further name transformations here as necessary
						string outName = Path.Combine(clsSettings.serviceSettings.dataDirectory.FullName, name);

						string directoryName = Path.GetDirectoryName(outName);
						Directory.CreateDirectory(directoryName);

						FileStream outStr = new FileStream(outName, FileMode.Create);
						tarIn.CopyEntryContents(outStr);
						// Do some cleanup.
						outStr.Close();
						if(outStr != null)
							outStr = null;
					}

					// Do some cleanup.
					if(tarIn != null)
					{
						tarIn.Close();
						tarIn = null;
					}
					if(fsIn != null)
						fsIn.Close();

				}

				result = true;

			}
			finally
			{
				if(inStream != null)
				{
					inStream.Close();
					inStream = null;
				}
			}

			return result;

		}

		/// <summary>
		/// Create the file "global_usage.xml" from the original "global_usage" coming from the original file downloaded.
		/// </summary>
		private static void generateXmlFormatedIndex()
		{
			// Set the currentOperation
			clsSettings.serviceSettings.currentOperation = operationList.generatingXmlFormatedIndex;

			// Nombre del fichero donde está el contenido a convertir en XML.
			string indexFileName = "global_usage";
			// Nombre del directorio donde está el contenido a convertir en XML.
			string sourceFolderName = "BL";
			// Nombre del directorio donde crear los ficheros XML.
			string xmlDestinationFolderName = "BL_XML";

			// Read the text file: 'global_usage'
			StreamReader streamOriginalIndexContent = new StreamReader(string.Format("{0}{1}{2}{3}", clsSettings.serviceSettings.dataDirectory,
																								sourceFolderName, System.IO.Path.DirectorySeparatorChar, indexFileName));

			// Variable for the generated XML.
			System.Text.StringBuilder sbXmlResult = new StringBuilder();

			// XML file Header.
			sbXmlResult.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
			// xmlns=\"urn:CategoryIndex-schema\"
			sbXmlResult.AppendLine(string.Format("<CategoryIndex DateCreated=\"{0}\" source=\"{1}\">", clsSettings.serviceSettings.downloadedFileLastModifiedDate.ToString("o"), clsSettings.serviceSettings.downloadUri));

			while(!streamOriginalIndexContent.EndOfStream)
			{
				string textLine = streamOriginalIndexContent.ReadLine();
				// Si es una línea de comentario.
				if(textLine.StartsWith("#"))
					continue;

				// Si es una línea en blanco.
				if(string.IsNullOrEmpty(textLine))
					continue;

				Categoria Category = new Categoria();
				while(!string.IsNullOrEmpty(textLine))
				{
					// Sólo tomar los campos que me interesan.
					// NAME:
					if(textLine.StartsWith("NAME:", true, System.Globalization.CultureInfo.CurrentCulture))
					{
						Category.name = textLine.Replace("NAME:", string.Empty);
						textLine = streamOriginalIndexContent.ReadLine();
						continue;
					}
					// DEFAULT_TYPE:
					if(textLine.StartsWith("DEFAULT_TYPE:", true, System.Globalization.CultureInfo.CurrentCulture))
					{
						Category.defaualt_type = textLine.Replace("DEFAULT_TYPE:", string.Empty);
						textLine = streamOriginalIndexContent.ReadLine();
						continue;
					}
					// DESC EN:
					if(textLine.StartsWith("DESC EN:", true, System.Globalization.CultureInfo.CurrentCulture))
					{
						Category.desc_en = textLine.Replace("DESC EN:", string.Empty);
						textLine = streamOriginalIndexContent.ReadLine();
						continue;
					}
					// DESC ES:
					if(textLine.StartsWith("DESC ES:", true, System.Globalization.CultureInfo.CurrentCulture))
					{
						Category.desc_es = textLine.Replace("DESC ES:", string.Empty);
						textLine = streamOriginalIndexContent.ReadLine();
						continue;
					}
					// NAME EN:
					if(textLine.StartsWith("NAME EN:", true, System.Globalization.CultureInfo.CurrentCulture))
					{
						Category.name_en = textLine.Replace("NAME EN:", string.Empty);
						textLine = streamOriginalIndexContent.ReadLine();
						continue;
					}
					// NAME ES:
					if(textLine.StartsWith("NAME ES:", true, System.Globalization.CultureInfo.CurrentCulture))
					{
						Category.name_es = textLine.Replace("NAME ES:", string.Empty);
						textLine = streamOriginalIndexContent.ReadLine();
						continue;
					}

					textLine = streamOriginalIndexContent.ReadLine();
				}

				//Procesar el contenido de la variable cat
				sbXmlResult.AppendLine(string.Format(" <Category name=\"{0}\">", string.IsNullOrEmpty(Category.name) ? string.Empty : Category.name.Trim()));
				//DEFAULT_TYPE:
				sbXmlResult.AppendLine(string.Format("  <default-type>{0}</default-type>", string.IsNullOrEmpty(Category.defaualt_type) ? string.Empty : Category.defaualt_type.Trim()));
				//DESC EN:
				sbXmlResult.AppendLine(string.Format("  <desc-en>{0}</desc-en>", string.IsNullOrEmpty(Category.desc_en) ? string.Empty : Category.desc_en.Trim()));
				//DESC ES:
				sbXmlResult.AppendLine(string.Format("  <desc-es>{0}</desc-es>", string.IsNullOrEmpty(Category.desc_es) ? string.Empty : Category.desc_es.Trim()));
				//NAME EN:
				sbXmlResult.AppendLine(string.Format("  <name-en>{0}</name-en>", string.IsNullOrEmpty(Category.name_en) ? string.Empty : Category.name_en.Trim()));
				//NAME ES:
				sbXmlResult.AppendLine(string.Format("  <name-es>{0}</name-es>", string.IsNullOrEmpty(Category.name_es) ? string.Empty : Category.name_es.Trim()));
				// Cerrar la categoría actual.
				sbXmlResult.AppendLine(" </Category>");

			}
			
			// Do some cleanup.
			if(streamOriginalIndexContent != null)
			{
				streamOriginalIndexContent.Close();
				streamOriginalIndexContent = null;
			}
			
			// End of que categories's catalog.
			sbXmlResult.Append("</CategoryIndex>");

			// Make sure exist the BL_XML folder.
			if(!Directory.Exists(string.Format("{0}{1}", clsSettings.serviceSettings.dataDirectory, xmlDestinationFolderName)))
			{
				clsSettings.serviceSettings.dataDirectory.CreateSubdirectory(xmlDestinationFolderName);
			}

			// Full path of the xml file to create.
			string xmlIndexFilePath = string.Format("{0}{1}{2}{3}{4}", clsSettings.serviceSettings.dataDirectory,
																xmlDestinationFolderName, Path.DirectorySeparatorChar, indexFileName, ".xml");

			// Delete the xml file if this exists.
			if(File.Exists(xmlIndexFilePath))
				File.Delete(xmlIndexFilePath);

			// Create the xml file with the new content.
			File.AppendAllText(xmlIndexFilePath, sbXmlResult.ToString());

			// Validate the xml generated file.
			// Generate the schema for the Xml file.
			string xsdGlobalUsageFileFullPath = string.Format("{0}{1}{2}", clsSettings.serviceSettings.dataDirectory, indexFileName, ".xsd");
			generateXsdGlobalUsage(xsdGlobalUsageFileFullPath);

			// Create the XmlSchemaSet class.
			System.Xml.Schema.XmlSchemaSet schemaSet = new System.Xml.Schema.XmlSchemaSet();

			// Add the schema to the collection. "urn:CategoryIndex-schema"
			schemaSet.Add(null, xsdGlobalUsageFileFullPath);

			validateXmlFile(xmlIndexFilePath, schemaSet);

			// Do some cleanup.
			if(schemaSet != null)
				schemaSet = null;

		} // Fin de generateXmlFomatedIndex()

		/// <summary>
		/// Generates the .xsd file to validate the global_usage.xml file
		/// </summary>
		/// <param name="xsdFileFullPath"></param>
		private static void generateXsdGlobalUsage(string xsdFileFullPath)
		{
			StringBuilder sbXSDContent = new StringBuilder();
			sbXSDContent.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
			sbXSDContent.AppendLine("<xs:schema attributeFormDefault=\"unqualified\" elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">"); //targetNamespace=\"urn:CategoryIndex-schema\" 
			sbXSDContent.AppendLine("	<xs:element name=\"CategoryIndex\">");
			sbXSDContent.AppendLine("		<xs:complexType>");
			sbXSDContent.AppendLine("			<xs:sequence>");
			sbXSDContent.AppendLine("				<xs:element maxOccurs=\"unbounded\" name=\"Category\">");
			sbXSDContent.AppendLine("					<xs:complexType>");
			sbXSDContent.AppendLine("						<xs:sequence>");
			sbXSDContent.AppendLine("							<xs:element name=\"default-type\" type=\"xs:string\" />");
			sbXSDContent.AppendLine("							<xs:element name=\"desc-en\" type=\"xs:string\" />");
			sbXSDContent.AppendLine("							<xs:element name=\"desc-es\" type=\"xs:string\" />");
			sbXSDContent.AppendLine("							<xs:element name=\"name-en\" type=\"xs:string\" />");
			sbXSDContent.AppendLine("							<xs:element name=\"name-es\" type=\"xs:string\" />");
			sbXSDContent.AppendLine("						</xs:sequence>");
			sbXSDContent.AppendLine("						<xs:attribute name=\"name\" type=\"xs:string\" use=\"required\" />");
			sbXSDContent.AppendLine("					</xs:complexType>");
			sbXSDContent.AppendLine("				</xs:element>");
			sbXSDContent.AppendLine("			</xs:sequence>");
			sbXSDContent.AppendLine("			<xs:attribute name=\"DateCreated\" type=\"xs:dateTime\" use=\"required\" />");
			sbXSDContent.AppendLine("			<xs:attribute name=\"source\" type=\"xs:string\" use=\"required\" />");
			sbXSDContent.AppendLine("		</xs:complexType>");
			sbXSDContent.AppendLine("	</xs:element>");
			sbXSDContent.AppendLine("</xs:schema>");

			if(File.Exists(xsdFileFullPath))
			{
				File.Delete(xsdFileFullPath);
			}
			System.IO.File.AppendAllText(xsdFileFullPath, sbXSDContent.ToString());

		}

		/// <summary>
		/// Generate the content files domains.xml and urls.xml for each category.
		/// </summary>
		private static void generateXmlFormatedContent()
		{
			// Set the currentOperation
			clsSettings.serviceSettings.currentOperation = operationList.generatingXmlFormatedContent;

			// Nombre del fichero donde está el contenido a convertir en XML.
			string indexFileName = "global_usage";
			// Nombre del directorio donde está el contenido a convertir en XML.
			string sourceFolderName = "BL";
			// Nombre del directorio donde crear los ficheros XML.
			string xmlDestinationFolderName = "BL_XML";

			// Full path to the index xml file created.
			string xmlIndexFilePath = string.Format("{0}{1}{2}{3}{4}", clsSettings.serviceSettings.dataDirectory,
																xmlDestinationFolderName, System.IO.Path.DirectorySeparatorChar, indexFileName, ".xml");

			// Begin the content files processing.
			// Create the content files (domain.xml & url.xml files) for each category found in the categoryIndex file (xmlIndexFilePath)

			// Load the XML document containing the CategoryIndex.
			XDocument xDocumentIndex = XDocument.Load(xmlIndexFilePath);
			// Read only the Attribute name of each Category Element.
			IEnumerable<XAttribute> CategoryNames = from e in xDocumentIndex.Descendants("Category")
																							where ((string)e.Attribute("name")) != string.Empty
																							select e.Attribute("name");

			// Generate the schema files to validate the domains.xml and urls.xml files, respectively.
			System.Xml.Schema.XmlSchemaSet schemaSet = new System.Xml.Schema.XmlSchemaSet();

			// Generate domains.xsd
			string xsdFileFullPath = string.Format("{0}{1}", clsSettings.serviceSettings.dataDirectory, "domains.xsd");
			generateXsdDomainsFile(xsdFileFullPath);
			// Add the schema to the collection.
			schemaSet.Add(null, xsdFileFullPath);

			// Generate urls.xsd
			xsdFileFullPath = string.Format("{0}{1}", clsSettings.serviceSettings.dataDirectory, "urls.xsd");
			generateXsdUrlsFile(xsdFileFullPath);
			// Add the schema to the collection.
			schemaSet.Add(null, xsdFileFullPath);

			// Iterate on each CategoryName.
			foreach(XAttribute name in CategoryNames)
			{
				string CategoryFolderName = name.Value.Replace('/', Path.DirectorySeparatorChar);
				string workingDir = string.Format("{0}{1}{2}{3}{4}", clsSettings.serviceSettings.dataDirectory,
																											sourceFolderName, Path.DirectorySeparatorChar,
																											CategoryFolderName, Path.DirectorySeparatorChar);
				if(Directory.Exists(workingDir))
				{
					string destXmlFolderFullPath = string.Format("{0}{1}{2}{3}{4}", clsSettings.serviceSettings.dataDirectory, xmlDestinationFolderName,
																											Path.DirectorySeparatorChar, CategoryFolderName, Path.DirectorySeparatorChar);

					// Check if exist the domains file and process if so.
					if(File.Exists(string.Format("{0}{1}", workingDir, "domains")))
					{
						StringBuilder sb = new StringBuilder();
						sb = processBLFile(string.Format("{0}{1}", workingDir, "domains"), name.Value);
						if(sb.Length != 0)
						{
							writeXmlBLFile(sb, destXmlFolderFullPath, "domains.xml");
							// Validate according to generate xsd file domains.xsd
							validateXmlFile(string.Format("{0}{1}", destXmlFolderFullPath, "domains.xml"), schemaSet);
						}
						// Do some Cleanup.
						if(sb != null)
							sb = null;
					}

					// Check if exist the urls file and process if so.
					if(File.Exists(string.Format("{0}{1}", workingDir, "urls")))
					{
						StringBuilder sb = new StringBuilder();
						sb = processBLFile(string.Format("{0}{1}", workingDir, "urls"), name.Value);
						if(sb.Length != 0)
						{
							writeXmlBLFile(sb, destXmlFolderFullPath, "urls.xml");
							// Validate according to generate xsd file urls.xsd
							validateXmlFile(string.Format("{0}{1}", destXmlFolderFullPath, "urls.xml"), schemaSet);
						}
						// Do some Cleanup.
						if(sb != null)
							sb = null;
					}
				}
			}

			// Do some Cleanup.
			if(xDocumentIndex != null)
				xDocumentIndex = null;
			if(schemaSet != null)
				schemaSet = null;
			if(CategoryNames != null)
				CategoryNames = null;

			return;

		}

		/// <summary>
		/// Create a file .xsd for xml validation of each domains.xml file generate for each category.
		/// </summary>
		/// <param name="xsdFileFullPath"></param>
		private static void generateXsdDomainsFile(string xsdFileFullPath)
		{
			StringBuilder sbXSDContent = new StringBuilder();
			sbXSDContent.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
			sbXSDContent.AppendLine("<xs:schema attributeFormDefault=\"unqualified\" elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">"); //targetNamespace=\"urn:CategoryIndexDomains-schema\" 
			sbXSDContent.AppendLine("	<xs:element name=\"domains\">");
			sbXSDContent.AppendLine("		<xs:complexType>");
			sbXSDContent.AppendLine("			<xs:sequence>");
			sbXSDContent.AppendLine("				<xs:element maxOccurs=\"unbounded\" name=\"domain\" type=\"xs:string\" />");
			sbXSDContent.AppendLine("			</xs:sequence>");
			sbXSDContent.AppendLine("			<xs:attribute name=\"category\" type=\"xs:string\" use=\"required\" />");
			sbXSDContent.AppendLine("		</xs:complexType>");
			sbXSDContent.AppendLine("	</xs:element>");
			sbXSDContent.AppendLine("</xs:schema>");

			if(File.Exists(xsdFileFullPath))
			{
				File.Delete(xsdFileFullPath);
			}
			System.IO.File.AppendAllText(xsdFileFullPath, sbXSDContent.ToString());
			// Do cleanup.
			if(sbXSDContent != null)
				sbXSDContent = null;
		}

		/// <summary>
		/// Create a file .xsd for xml validation of each urls.xml file generate for each category.
		/// </summary>
		/// <param name="xsdFileFullPath"></param>
		private static void generateXsdUrlsFile(string xsdFileFullPath)
		{
			StringBuilder sbXSDContent = new StringBuilder();
			sbXSDContent.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
			sbXSDContent.AppendLine("<xs:schema attributeFormDefault=\"unqualified\" elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">"); //targetNamespace=\"urn:CategoryIndexUrls-schema\" 
			sbXSDContent.AppendLine("	<xs:element name=\"urls\">");
			sbXSDContent.AppendLine("		<xs:complexType>");
			sbXSDContent.AppendLine("			<xs:sequence>");
			sbXSDContent.AppendLine("				<xs:element maxOccurs=\"unbounded\" name=\"url\" type=\"xs:anyURI\" />");
			sbXSDContent.AppendLine("			</xs:sequence>");
			sbXSDContent.AppendLine("			<xs:attribute name=\"category\" type=\"xs:string\" use=\"required\" />");
			sbXSDContent.AppendLine("		</xs:complexType>");
			sbXSDContent.AppendLine("	</xs:element>");
			sbXSDContent.AppendLine("</xs:schema>");

			if(File.Exists(xsdFileFullPath))
			{
				File.Delete(xsdFileFullPath);
			}
			System.IO.File.AppendAllText(xsdFileFullPath, sbXSDContent.ToString());
			// Do cleanup.
			if(sbXSDContent != null)
				sbXSDContent = null;
		}

		/// <summary>
		/// Process the original file "domains" or "urls" to generate its content in xml format.
		/// </summary>
		/// <param name="fileFullPath">Path to the original "domains" or "urls" file.</param>
		/// <param name="categoryName">The name of the category to process.</param>
		/// <returns>Returns the content converted to xml for a file domains.xml or urls.xml according to the parameters passed.</returns>
		private static StringBuilder processBLFile(string fileFullPath, string categoryName)
		{
			StringBuilder sbXmlHeader = new StringBuilder();
			StringBuilder sbXmlContent = new StringBuilder();
			StringBuilder sbXmlFooter = new StringBuilder();
			StringBuilder sbXmlAll = new StringBuilder();
			StreamReader srFileContent = new StreamReader(fileFullPath);
			string rootName = fileFullPath.EndsWith("domains") ? "domains" : "urls";
			string childName = fileFullPath.EndsWith("domains") ? "domain" : "url";

			string lineContent = string.Empty;
			sbXmlHeader.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
			sbXmlHeader.AppendLine(string.Format("<{0} category=\"{1}\">", rootName, categoryName));
			while(!srFileContent.EndOfStream)
			{
				lineContent = srFileContent.ReadLine();
				// If not content aviable continue to the next one.
				if(string.IsNullOrEmpty(lineContent)) continue;
				// Delete from the "?" character, the this is ignored appears in an ISA Server URLSet or DomainNameSet.
				if(lineContent.IndexOf('?') > 0)
					lineContent = lineContent.Remove(lineContent.IndexOf('?'));
				string xElement = string.Format("<{0}><![CDATA[{1}]]></{0}>", childName, lineContent.ToLower());
				// Parse the content before add to into xml file.
				try
				{
					XElement.Parse(xElement, LoadOptions.PreserveWhitespace);
				}
				catch(System.Exception)
				{
					//System.Diagnostics.Debug.WriteLine(ex.Message);
					continue;
				}
				sbXmlContent.AppendLine(xElement);
			}
			// Cleanup
			srFileContent.Close();

			sbXmlFooter.AppendLine(string.Format("</{0}>", rootName));

			if(sbXmlContent.Length != 0)
				sbXmlAll.Append(string.Format("{0}{1}{2}", sbXmlHeader, sbXmlContent, sbXmlFooter));

			return sbXmlAll;

		}

		/// <summary>
		/// Validate the xml file "domains.xml" or "urls.xml" according to the schema create for it.
		/// </summary>
		/// <param name="xmlFileFullPath">Path to the file to validate.</param>
		/// <param name="schemaSet">SchemaSet contanining the definitions to validate the file.</param>
		private static void validateXmlFile(string xmlFileFullPath, System.Xml.Schema.XmlSchemaSet schemaSet)
		{
			// Generate the schema file.
			XmlReaderSettings settings = new XmlReaderSettings();

			settings.ValidationType = ValidationType.Schema;
			settings.Schemas = schemaSet;
			settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessInlineSchema;
			settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;
			settings.ValidationEventHandler += new System.Xml.Schema.ValidationEventHandler(xmlValidationCallBack);

			// Create the XmlReader object.
			XmlReader reader = XmlReader.Create(xmlFileFullPath, settings);

			// Parse the file. 
			while(reader.Read())
				;

			// Do some Cleanup.
			if(reader != null)
			{
				reader.Close();
				reader = null;
			}
			if(settings != null)
				settings = null;
		
		}

		/// <summary>
		/// Manage any warnings or errors during the validation of the xml formated files (global_usage, domains.xml or urls.xml).
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private static void xmlValidationCallBack(object sender, System.Xml.Schema.ValidationEventArgs args)
		{
			if(args.Severity == System.Xml.Schema.XmlSeverityType.Warning)
				clsLogProcessing.WriteToEventLog(string.Format("Advertencia: discordancia en el esquema mientras se " +
					"genera el fichero {0}. No se validó el elemento en cuestión. A continuación se muestra un mensaje " +
					"de error que le puede servir:" + Environment.NewLine + "", "global_usage.xml", args.Message), EventLogEntryType.Warning);
			else
				clsLogProcessing.WriteToEventLog(string.Format("Error al comprobar el esquema (schema) XML del fichero de indice " +
					"\"global_usage.xml\" o de alguno de los de contenido (\"domains.xml\" ó \"urls.xml\'). " + Environment.NewLine +
					"La descripción de este error se muestra a continuación: {0}", args.Message), EventLogEntryType.Error);
		}

		/// <summary>
		/// Writes the xml file to the actual file system.
		/// </summary>
		/// <param name="content">Content xml formated to write to the file.</param>
		/// <param name="destinationFolderFullPath">The full path in the file system to create the xml file.</param>
		/// <param name="destinationFileName">The name of the file to create.</param>
		private static void writeXmlBLFile(StringBuilder content, string destinationFolderFullPath, string destinationFileName)
		{
			if(!Directory.Exists(destinationFolderFullPath))
			{
				Directory.CreateDirectory(destinationFolderFullPath);
			}
			// Delete the file xml if this exists.
			if(File.Exists(string.Format("{0}{1}{2}", destinationFolderFullPath, System.IO.Path.DirectorySeparatorChar, destinationFileName)))
				File.Delete(string.Format("{0}{1}{2}", destinationFolderFullPath, System.IO.Path.DirectorySeparatorChar, destinationFileName));
			System.IO.File.AppendAllText(string.Format("{0}{1}{2}", destinationFolderFullPath, System.IO.Path.DirectorySeparatorChar, destinationFileName),
																	 content.ToString());
			return;
		}


	}
}
