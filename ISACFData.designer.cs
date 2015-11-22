﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LAMSoft.ISACFMgr
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="ISA_Server_Logs")]
	public partial class ISACFDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InsertccfDatosGen(ccfDatosGen instance);
    partial void UpdateccfDatosGen(ccfDatosGen instance);
    partial void DeleteccfDatosGen(ccfDatosGen instance);
    partial void InsertccfCategory(ccfCategory instance);
    partial void UpdateccfCategory(ccfCategory instance);
    partial void DeleteccfCategory(ccfCategory instance);
    partial void InsertccfCategoryName_es(ccfCategoryName_es instance);
    partial void UpdateccfCategoryName_es(ccfCategoryName_es instance);
    partial void DeleteccfCategoryName_es(ccfCategoryName_es instance);
    partial void InsertccfDomain(ccfDomain instance);
    partial void UpdateccfDomain(ccfDomain instance);
    partial void DeleteccfDomain(ccfDomain instance);
    partial void InsertccfUrl(ccfUrl instance);
    partial void UpdateccfUrl(ccfUrl instance);
    partial void DeleteccfUrl(ccfUrl instance);
    partial void InsertccfIPv4(ccfIPv4 instance);
    partial void UpdateccfIPv4(ccfIPv4 instance);
    partial void DeleteccfIPv4(ccfIPv4 instance);
    partial void InsertccfCategoryToBlock(ccfCategoryToBlock instance);
    partial void UpdateccfCategoryToBlock(ccfCategoryToBlock instance);
    partial void DeleteccfCategoryToBlock(ccfCategoryToBlock instance);
    #endregion
		
		public ISACFDataContext() : 
				base(global::LAMSoft.ISACFMgr.Properties.Settings.Default.ISA_Server_LogsConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public ISACFDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public ISACFDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public ISACFDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public ISACFDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<ccfDatosGen> ccfDatosGens
		{
			get
			{
				return this.GetTable<ccfDatosGen>();
			}
		}
		
		public System.Data.Linq.Table<ccfCategory> ccfCategories
		{
			get
			{
				return this.GetTable<ccfCategory>();
			}
		}
		
		public System.Data.Linq.Table<ccfCategoryName_es> ccfCategoryName_es
		{
			get
			{
				return this.GetTable<ccfCategoryName_es>();
			}
		}
		
		public System.Data.Linq.Table<ccfDomain> ccfDomains
		{
			get
			{
				return this.GetTable<ccfDomain>();
			}
		}
		
		public System.Data.Linq.Table<ccfUrl> ccfUrls
		{
			get
			{
				return this.GetTable<ccfUrl>();
			}
		}
		
		public System.Data.Linq.Table<ccfIPv4> ccfIPv4s
		{
			get
			{
				return this.GetTable<ccfIPv4>();
			}
		}
		
		public System.Data.Linq.Table<ccfCategoryToBlock> ccfCategoryToBlocks
		{
			get
			{
				return this.GetTable<ccfCategoryToBlock>();
			}
		}
		
		public System.Data.Linq.Table<vw_ccfCategoryToBlock> vw_ccfCategoryToBlocks
		{
			get
			{
				return this.GetTable<vw_ccfCategoryToBlock>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.ccfDatosGen")]
	public partial class ccfDatosGen : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _ID;
		
		private System.DateTime _dateCreated;
		
		private string _source;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIDChanging(int value);
    partial void OnIDChanged();
    partial void OndateCreatedChanging(System.DateTime value);
    partial void OndateCreatedChanged();
    partial void OnsourceChanging(string value);
    partial void OnsourceChanged();
    #endregion
		
		public ccfDatosGen()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ID", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int ID
		{
			get
			{
				return this._ID;
			}
			set
			{
				if ((this._ID != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._ID = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_dateCreated", DbType="DateTime NOT NULL")]
		public System.DateTime dateCreated
		{
			get
			{
				return this._dateCreated;
			}
			set
			{
				if ((this._dateCreated != value))
				{
					this.OndateCreatedChanging(value);
					this.SendPropertyChanging();
					this._dateCreated = value;
					this.SendPropertyChanged("dateCreated");
					this.OndateCreatedChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_source", DbType="VarChar(512) NOT NULL", CanBeNull=false)]
		public string source
		{
			get
			{
				return this._source;
			}
			set
			{
				if ((this._source != value))
				{
					this.OnsourceChanging(value);
					this.SendPropertyChanging();
					this._source = value;
					this.SendPropertyChanged("source");
					this.OnsourceChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.ccfCategory")]
	public partial class ccfCategory : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _ID;
		
		private string _default_type;
		
		private string _name;
		
		private string _name_en;
		
		private string _desc_en;
		
		private bool _processForISARule;
		
		private EntityRef<ccfCategoryName_es> _ccfCategoryName_es;
		
		private EntitySet<ccfDomain> _ccfDomains;
		
		private EntitySet<ccfUrl> _ccfUrls;
		
		private EntitySet<ccfIPv4> _ccfIPv4s;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIDChanging(int value);
    partial void OnIDChanged();
    partial void Ondefault_typeChanging(string value);
    partial void Ondefault_typeChanged();
    partial void OnnameChanging(string value);
    partial void OnnameChanged();
    partial void Onname_enChanging(string value);
    partial void Onname_enChanged();
    partial void Ondesc_enChanging(string value);
    partial void Ondesc_enChanged();
    partial void OnprocessForISARuleChanging(bool value);
    partial void OnprocessForISARuleChanged();
    #endregion
		
		public ccfCategory()
		{
			this._ccfCategoryName_es = default(EntityRef<ccfCategoryName_es>);
			this._ccfDomains = new EntitySet<ccfDomain>(new Action<ccfDomain>(this.attach_ccfDomains), new Action<ccfDomain>(this.detach_ccfDomains));
			this._ccfUrls = new EntitySet<ccfUrl>(new Action<ccfUrl>(this.attach_ccfUrls), new Action<ccfUrl>(this.detach_ccfUrls));
			this._ccfIPv4s = new EntitySet<ccfIPv4>(new Action<ccfIPv4>(this.attach_ccfIPv4s), new Action<ccfIPv4>(this.detach_ccfIPv4s));
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ID", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int ID
		{
			get
			{
				return this._ID;
			}
			set
			{
				if ((this._ID != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._ID = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_default_type", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string default_type
		{
			get
			{
				return this._default_type;
			}
			set
			{
				if ((this._default_type != value))
				{
					this.Ondefault_typeChanging(value);
					this.SendPropertyChanging();
					this._default_type = value;
					this.SendPropertyChanged("default_type");
					this.Ondefault_typeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_name", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string name
		{
			get
			{
				return this._name;
			}
			set
			{
				if ((this._name != value))
				{
					this.OnnameChanging(value);
					this.SendPropertyChanging();
					this._name = value;
					this.SendPropertyChanged("name");
					this.OnnameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_name_en", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string name_en
		{
			get
			{
				return this._name_en;
			}
			set
			{
				if ((this._name_en != value))
				{
					this.Onname_enChanging(value);
					this.SendPropertyChanging();
					this._name_en = value;
					this.SendPropertyChanged("name_en");
					this.Onname_enChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_desc_en", DbType="VarChar(2048) NOT NULL", CanBeNull=false)]
		public string desc_en
		{
			get
			{
				return this._desc_en;
			}
			set
			{
				if ((this._desc_en != value))
				{
					this.Ondesc_enChanging(value);
					this.SendPropertyChanging();
					this._desc_en = value;
					this.SendPropertyChanged("desc_en");
					this.Ondesc_enChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_processForISARule", DbType="Bit NOT NULL")]
		public bool processForISARule
		{
			get
			{
				return this._processForISARule;
			}
			set
			{
				if ((this._processForISARule != value))
				{
					this.OnprocessForISARuleChanging(value);
					this.SendPropertyChanging();
					this._processForISARule = value;
					this.SendPropertyChanged("processForISARule");
					this.OnprocessForISARuleChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="ccfCategory_ccfCategoryName_es", Storage="_ccfCategoryName_es", ThisKey="ID", OtherKey="id_Category", IsUnique=true, IsForeignKey=false)]
		public ccfCategoryName_es ccfCategoryName_es
		{
			get
			{
				return this._ccfCategoryName_es.Entity;
			}
			set
			{
				ccfCategoryName_es previousValue = this._ccfCategoryName_es.Entity;
				if (((previousValue != value) 
							|| (this._ccfCategoryName_es.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._ccfCategoryName_es.Entity = null;
						previousValue.ccfCategory = null;
					}
					this._ccfCategoryName_es.Entity = value;
					if ((value != null))
					{
						value.ccfCategory = this;
					}
					this.SendPropertyChanged("ccfCategoryName_es");
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="ccfCategory_ccfDomain", Storage="_ccfDomains", ThisKey="ID", OtherKey="id_Category")]
		public EntitySet<ccfDomain> ccfDomains
		{
			get
			{
				return this._ccfDomains;
			}
			set
			{
				this._ccfDomains.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="ccfCategory_ccfUrl", Storage="_ccfUrls", ThisKey="ID", OtherKey="id_Category")]
		public EntitySet<ccfUrl> ccfUrls
		{
			get
			{
				return this._ccfUrls;
			}
			set
			{
				this._ccfUrls.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="ccfCategory_ccfIPv4", Storage="_ccfIPv4s", ThisKey="ID", OtherKey="id_Category")]
		public EntitySet<ccfIPv4> ccfIPv4s
		{
			get
			{
				return this._ccfIPv4s;
			}
			set
			{
				this._ccfIPv4s.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_ccfDomains(ccfDomain entity)
		{
			this.SendPropertyChanging();
			entity.ccfCategory = this;
		}
		
		private void detach_ccfDomains(ccfDomain entity)
		{
			this.SendPropertyChanging();
			entity.ccfCategory = null;
		}
		
		private void attach_ccfUrls(ccfUrl entity)
		{
			this.SendPropertyChanging();
			entity.ccfCategory = this;
		}
		
		private void detach_ccfUrls(ccfUrl entity)
		{
			this.SendPropertyChanging();
			entity.ccfCategory = null;
		}
		
		private void attach_ccfIPv4s(ccfIPv4 entity)
		{
			this.SendPropertyChanging();
			entity.ccfCategory = this;
		}
		
		private void detach_ccfIPv4s(ccfIPv4 entity)
		{
			this.SendPropertyChanging();
			entity.ccfCategory = null;
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.ccfCategoryName_es")]
	public partial class ccfCategoryName_es : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _id_Category;
		
		private string _name_es;
		
		private string _desc_es;
		
		private EntityRef<ccfCategory> _ccfCategory;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void Onid_CategoryChanging(int value);
    partial void Onid_CategoryChanged();
    partial void Onname_esChanging(string value);
    partial void Onname_esChanged();
    partial void Ondesc_esChanging(string value);
    partial void Ondesc_esChanged();
    #endregion
		
		public ccfCategoryName_es()
		{
			this._ccfCategory = default(EntityRef<ccfCategory>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_id_Category", DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int id_Category
		{
			get
			{
				return this._id_Category;
			}
			set
			{
				if ((this._id_Category != value))
				{
					if (this._ccfCategory.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.Onid_CategoryChanging(value);
					this.SendPropertyChanging();
					this._id_Category = value;
					this.SendPropertyChanged("id_Category");
					this.Onid_CategoryChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_name_es", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string name_es
		{
			get
			{
				return this._name_es;
			}
			set
			{
				if ((this._name_es != value))
				{
					this.Onname_esChanging(value);
					this.SendPropertyChanging();
					this._name_es = value;
					this.SendPropertyChanged("name_es");
					this.Onname_esChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_desc_es", DbType="NVarChar(2048)")]
		public string desc_es
		{
			get
			{
				return this._desc_es;
			}
			set
			{
				if ((this._desc_es != value))
				{
					this.Ondesc_esChanging(value);
					this.SendPropertyChanging();
					this._desc_es = value;
					this.SendPropertyChanged("desc_es");
					this.Ondesc_esChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="ccfCategory_ccfCategoryName_es", Storage="_ccfCategory", ThisKey="id_Category", OtherKey="ID", IsForeignKey=true, DeleteOnNull=true, DeleteRule="CASCADE")]
		public ccfCategory ccfCategory
		{
			get
			{
				return this._ccfCategory.Entity;
			}
			set
			{
				ccfCategory previousValue = this._ccfCategory.Entity;
				if (((previousValue != value) 
							|| (this._ccfCategory.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._ccfCategory.Entity = null;
						previousValue.ccfCategoryName_es = null;
					}
					this._ccfCategory.Entity = value;
					if ((value != null))
					{
						value.ccfCategoryName_es = this;
						this._id_Category = value.ID;
					}
					else
					{
						this._id_Category = default(int);
					}
					this.SendPropertyChanged("ccfCategory");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.ccfDomain")]
	public partial class ccfDomain : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _ID;
		
		private int _id_Category;
		
		private string _domain;
		
		private EntityRef<ccfCategory> _ccfCategory;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIDChanging(int value);
    partial void OnIDChanged();
    partial void Onid_CategoryChanging(int value);
    partial void Onid_CategoryChanged();
    partial void OndomainChanging(string value);
    partial void OndomainChanged();
    #endregion
		
		public ccfDomain()
		{
			this._ccfCategory = default(EntityRef<ccfCategory>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ID", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int ID
		{
			get
			{
				return this._ID;
			}
			set
			{
				if ((this._ID != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._ID = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_id_Category", DbType="Int NOT NULL")]
		public int id_Category
		{
			get
			{
				return this._id_Category;
			}
			set
			{
				if ((this._id_Category != value))
				{
					if (this._ccfCategory.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.Onid_CategoryChanging(value);
					this.SendPropertyChanging();
					this._id_Category = value;
					this.SendPropertyChanged("id_Category");
					this.Onid_CategoryChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_domain", DbType="VarChar(2048) NOT NULL", CanBeNull=false)]
		public string domain
		{
			get
			{
				return this._domain;
			}
			set
			{
				if ((this._domain != value))
				{
					this.OndomainChanging(value);
					this.SendPropertyChanging();
					this._domain = value;
					this.SendPropertyChanged("domain");
					this.OndomainChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="ccfCategory_ccfDomain", Storage="_ccfCategory", ThisKey="id_Category", OtherKey="ID", IsForeignKey=true, DeleteOnNull=true, DeleteRule="CASCADE")]
		public ccfCategory ccfCategory
		{
			get
			{
				return this._ccfCategory.Entity;
			}
			set
			{
				ccfCategory previousValue = this._ccfCategory.Entity;
				if (((previousValue != value) 
							|| (this._ccfCategory.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._ccfCategory.Entity = null;
						previousValue.ccfDomains.Remove(this);
					}
					this._ccfCategory.Entity = value;
					if ((value != null))
					{
						value.ccfDomains.Add(this);
						this._id_Category = value.ID;
					}
					else
					{
						this._id_Category = default(int);
					}
					this.SendPropertyChanged("ccfCategory");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.ccfUrl")]
	public partial class ccfUrl : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _ID;
		
		private int _id_Category;
		
		private string _url;
		
		private EntityRef<ccfCategory> _ccfCategory;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIDChanging(int value);
    partial void OnIDChanged();
    partial void Onid_CategoryChanging(int value);
    partial void Onid_CategoryChanged();
    partial void OnurlChanging(string value);
    partial void OnurlChanged();
    #endregion
		
		public ccfUrl()
		{
			this._ccfCategory = default(EntityRef<ccfCategory>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ID", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int ID
		{
			get
			{
				return this._ID;
			}
			set
			{
				if ((this._ID != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._ID = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_id_Category", DbType="Int NOT NULL")]
		public int id_Category
		{
			get
			{
				return this._id_Category;
			}
			set
			{
				if ((this._id_Category != value))
				{
					if (this._ccfCategory.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.Onid_CategoryChanging(value);
					this.SendPropertyChanging();
					this._id_Category = value;
					this.SendPropertyChanged("id_Category");
					this.Onid_CategoryChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_url", DbType="VarChar(2048) NOT NULL", CanBeNull=false)]
		public string url
		{
			get
			{
				return this._url;
			}
			set
			{
				if ((this._url != value))
				{
					this.OnurlChanging(value);
					this.SendPropertyChanging();
					this._url = value;
					this.SendPropertyChanged("url");
					this.OnurlChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="ccfCategory_ccfUrl", Storage="_ccfCategory", ThisKey="id_Category", OtherKey="ID", IsForeignKey=true, DeleteOnNull=true, DeleteRule="CASCADE")]
		public ccfCategory ccfCategory
		{
			get
			{
				return this._ccfCategory.Entity;
			}
			set
			{
				ccfCategory previousValue = this._ccfCategory.Entity;
				if (((previousValue != value) 
							|| (this._ccfCategory.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._ccfCategory.Entity = null;
						previousValue.ccfUrls.Remove(this);
					}
					this._ccfCategory.Entity = value;
					if ((value != null))
					{
						value.ccfUrls.Add(this);
						this._id_Category = value.ID;
					}
					else
					{
						this._id_Category = default(int);
					}
					this.SendPropertyChanged("ccfCategory");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.ccfIPv4")]
	public partial class ccfIPv4 : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _ID;
		
		private int _id_Category;
		
		private string _IP;
		
		private EntityRef<ccfCategory> _ccfCategory;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIDChanging(int value);
    partial void OnIDChanged();
    partial void Onid_CategoryChanging(int value);
    partial void Onid_CategoryChanged();
    partial void OnIPChanging(string value);
    partial void OnIPChanged();
    #endregion
		
		public ccfIPv4()
		{
			this._ccfCategory = default(EntityRef<ccfCategory>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ID", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int ID
		{
			get
			{
				return this._ID;
			}
			set
			{
				if ((this._ID != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._ID = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_id_Category", DbType="Int NOT NULL")]
		public int id_Category
		{
			get
			{
				return this._id_Category;
			}
			set
			{
				if ((this._id_Category != value))
				{
					if (this._ccfCategory.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.Onid_CategoryChanging(value);
					this.SendPropertyChanging();
					this._id_Category = value;
					this.SendPropertyChanged("id_Category");
					this.Onid_CategoryChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_IP", DbType="VarChar(15) NOT NULL", CanBeNull=false)]
		public string IP
		{
			get
			{
				return this._IP;
			}
			set
			{
				if ((this._IP != value))
				{
					this.OnIPChanging(value);
					this.SendPropertyChanging();
					this._IP = value;
					this.SendPropertyChanged("IP");
					this.OnIPChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="ccfCategory_ccfIPv4", Storage="_ccfCategory", ThisKey="id_Category", OtherKey="ID", IsForeignKey=true, DeleteOnNull=true, DeleteRule="CASCADE")]
		public ccfCategory ccfCategory
		{
			get
			{
				return this._ccfCategory.Entity;
			}
			set
			{
				ccfCategory previousValue = this._ccfCategory.Entity;
				if (((previousValue != value) 
							|| (this._ccfCategory.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._ccfCategory.Entity = null;
						previousValue.ccfIPv4s.Remove(this);
					}
					this._ccfCategory.Entity = value;
					if ((value != null))
					{
						value.ccfIPv4s.Add(this);
						this._id_Category = value.ID;
					}
					else
					{
						this._id_Category = default(int);
					}
					this.SendPropertyChanged("ccfCategory");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.ccfCategoryToBlock")]
	public partial class ccfCategoryToBlock : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _ID;
		
		private string _name;
		
		private bool _processDomains;
		
		private bool _processUrls;
		
		private bool _processIPs;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIDChanging(int value);
    partial void OnIDChanged();
    partial void OnnameChanging(string value);
    partial void OnnameChanged();
    partial void OnprocessDomainsChanging(bool value);
    partial void OnprocessDomainsChanged();
    partial void OnprocessUrlsChanging(bool value);
    partial void OnprocessUrlsChanged();
    partial void OnprocessIPsChanging(bool value);
    partial void OnprocessIPsChanged();
    #endregion
		
		public ccfCategoryToBlock()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ID", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int ID
		{
			get
			{
				return this._ID;
			}
			set
			{
				if ((this._ID != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._ID = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_name", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string name
		{
			get
			{
				return this._name;
			}
			set
			{
				if ((this._name != value))
				{
					this.OnnameChanging(value);
					this.SendPropertyChanging();
					this._name = value;
					this.SendPropertyChanged("name");
					this.OnnameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_processDomains", DbType="Bit NOT NULL")]
		public bool processDomains
		{
			get
			{
				return this._processDomains;
			}
			set
			{
				if ((this._processDomains != value))
				{
					this.OnprocessDomainsChanging(value);
					this.SendPropertyChanging();
					this._processDomains = value;
					this.SendPropertyChanged("processDomains");
					this.OnprocessDomainsChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_processUrls", DbType="Bit NOT NULL")]
		public bool processUrls
		{
			get
			{
				return this._processUrls;
			}
			set
			{
				if ((this._processUrls != value))
				{
					this.OnprocessUrlsChanging(value);
					this.SendPropertyChanging();
					this._processUrls = value;
					this.SendPropertyChanged("processUrls");
					this.OnprocessUrlsChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_processIPs", DbType="Bit NOT NULL")]
		public bool processIPs
		{
			get
			{
				return this._processIPs;
			}
			set
			{
				if ((this._processIPs != value))
				{
					this.OnprocessIPsChanging(value);
					this.SendPropertyChanging();
					this._processIPs = value;
					this.SendPropertyChanged("processIPs");
					this.OnprocessIPsChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.vw_ccfCategoryToBlock")]
	public partial class vw_ccfCategoryToBlock
	{
		
		private int _ID;
		
		private string _name;
		
		private string _desc_en;
		
		private string _default_type;
		
		private bool _processForISARule;
		
		private bool _processDomains;
		
		private bool _processUrls;
		
		private bool _processIPs;
		
		public vw_ccfCategoryToBlock()
		{
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ID", DbType="Int NOT NULL")]
		public int ID
		{
			get
			{
				return this._ID;
			}
			set
			{
				if ((this._ID != value))
				{
					this._ID = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_name", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string name
		{
			get
			{
				return this._name;
			}
			set
			{
				if ((this._name != value))
				{
					this._name = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_desc_en", DbType="VarChar(2048) NOT NULL", CanBeNull=false)]
		public string desc_en
		{
			get
			{
				return this._desc_en;
			}
			set
			{
				if ((this._desc_en != value))
				{
					this._desc_en = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_default_type", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string default_type
		{
			get
			{
				return this._default_type;
			}
			set
			{
				if ((this._default_type != value))
				{
					this._default_type = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_processForISARule", DbType="Bit NOT NULL")]
		public bool processForISARule
		{
			get
			{
				return this._processForISARule;
			}
			set
			{
				if ((this._processForISARule != value))
				{
					this._processForISARule = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_processDomains", DbType="Bit NOT NULL")]
		public bool processDomains
		{
			get
			{
				return this._processDomains;
			}
			set
			{
				if ((this._processDomains != value))
				{
					this._processDomains = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_processUrls", DbType="Bit NOT NULL")]
		public bool processUrls
		{
			get
			{
				return this._processUrls;
			}
			set
			{
				if ((this._processUrls != value))
				{
					this._processUrls = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_processIPs", DbType="Bit NOT NULL")]
		public bool processIPs
		{
			get
			{
				return this._processIPs;
			}
			set
			{
				if ((this._processIPs != value))
				{
					this._processIPs = value;
				}
			}
		}
	}
}
#pragma warning restore 1591
