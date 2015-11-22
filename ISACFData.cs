namespace LAMSoft.ISACFMgr
{
	partial class ISACFDataContext : System.Data.Linq.DataContext
	{
		partial void OnCreated()
		{
			this.CommandTimeout = 3600; // Segundos 
		}
	}
}
