//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a tool.
//     Runtime Version: 1.1.4322.2032
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </autogenerated>
//------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.OleDb;
using System.Collections;
using System.ComponentModel;
using System.Globalization;

/// <summary>
/// This Class is used as a wrapper around Databinding
/// </summary>
/// <remarks>
/// 	created by - Forstmeier Peter
/// 	created on - 16.10.2005 14:49:43
/// </remarks>
namespace SharpReportCore {
	
	public class DataManager :IDisposable {
		

		ReportSettings reportSettings;
		object dataSource;
		string dataMember;
		ConnectionObject connectionObject;
		IDbConnection connection;
		IDataViewStrategy dataViewStrategy;
		
		public event EventHandler <ListChangedEventArgs> ListChanged;
		
		/// <summary>
		/// use this Constructor for PullDataReports
		/// </summary>
		/// <param name="connection">A valid connection</param>
		/// <param name="reportSettings">a <see cref="ReportSettings"></see></param>
		
		#region Constructores
		public DataManager(ConnectionObject connectionObject, ReportSettings reportSettings){
			if (connectionObject == null) {
				throw new ArgumentNullException("DataManager:ConnectionObject");
			}
			
			if (reportSettings == null) {
				throw new ArgumentNullException("reportSettings");
			}
			try {
				this.connectionObject = connectionObject;
				this.reportSettings = reportSettings;
				CheckConnection (this.connectionObject);
				CheckReportSettings(reportSettings);
				CheckDataSource(this.FillDataSet().Tables[0]);

				this.dataViewStrategy = new TableStrategy((DataTable)this.dataSource,
				                                          reportSettings);
				
				this.dataViewStrategy.ListChanged += new EventHandler <ListChangedEventArgs> (NotifyListChanged);
//			this.dataViewStrategy.GroupChanged += new EventHandler<GroupChangedEventArgs> (OnGroupChange);
			}
			catch (Exception) {
				throw;
			} finally {
				if (this.connectionObject.Connection.State == ConnectionState.Open) {
					this.connectionObject.Connection.Close();
				}
			}
		}
		
		public DataManager(DataTable dataSource, ReportSettings reportSettings){
			
			this.InitDataManager(reportSettings,dataSource);
			this.dataViewStrategy = new TableStrategy((DataTable)this.dataSource,
			                                          reportSettings);
			this.dataViewStrategy.ListChanged += new EventHandler <ListChangedEventArgs> (NotifyListChanged);
			
		}
		
		public DataManager(DataSet dataSource, ReportSettings reportSettings)
			:this (dataSource,"",reportSettings){
			
		}
		
		public DataManager(DataSet dataSource,string dataMember, ReportSettings reportSettings){
			
			this.dataMember = dataMember;
			this.InitDataManager(reportSettings,dataSource);
			this.dataViewStrategy = new TableStrategy((DataTable)this.dataSource,
			                                          reportSettings);
			this.dataViewStrategy.ListChanged += new EventHandler <ListChangedEventArgs> (NotifyListChanged);
		}
		
		public DataManager(IList dataSource, ReportSettings reportSettings){
			
			this.InitDataManager(reportSettings,dataSource);
			
			this.dataViewStrategy = new CollectionStrategy ((IList)this.dataSource,
			                                                this.dataMember,
			                                                reportSettings);
			this.dataViewStrategy.ListChanged += new EventHandler <ListChangedEventArgs> (NotifyListChanged);
			
		}
		
		
		public void  DataBind() {
			CheckReportColumns();
			this.dataViewStrategy.Bind();
		}
		
		#endregion
		
		void InitDataManager (ReportSettings reportSettings,object dataSource) {
			try {
				CheckReportSettings(reportSettings);
				CheckDataSource(dataSource);
			} catch (Exception) {
				throw;
			}
		}
		
		void CheckReportSettings(ReportSettings settings) {
			try {
				if (settings.DataModel != GlobalEnums.PushPullModelEnum.PushData) {
					SqlQueryChecker.Check(settings.CommandType,settings.CommandText);
				}
				
			} catch (IllegalQueryException) {
				throw;
			}
			catch (Exception) {
				throw;
			}
			this.reportSettings = settings;
		}
		
		
		void CheckDataSource(object source) {
			if (source == null) {
				throw new MissingDataSourceException();
			}
			
			if (source is IList ||source is IListSource || source is IBindingList) {
				
				//DataTable
				this.dataSource = source;
				DataTable table = this.dataSource as DataTable;
				if (table != null) {
					
					this.dataMember = table.TableName;
					return;
				}

				//DataSet
				DataSet dataSet = this.dataSource as DataSet;
				if (dataSet != null) {
					if (dataSet.Tables.Count > 0) {
						DataTable tbl;
						if (String.IsNullOrEmpty(this.dataMember)){
							tbl = dataSet.Tables[0];
						} else {
							DataTableCollection tcol = dataSet.Tables;
							if (tcol.Contains(this.dataMember)) {
								tbl = tcol[this.dataMember];
								this.dataSource = tbl;
							}
						}
					}else {
						throw new MissingDataSourceException();
					}
					return;
				}
				
				//IList
				IList list = source as IList;
				if (list != null) {
					this.dataSource = list;
					this.dataMember = source.ToString();
					if (list.Count == 0) {
						throw new MissingDataSourceException();
					}
					return;
					
				}
			} else {
				throw new MissingDataSourceException();
			}
		}
		
		void CheckConnection (ConnectionObject connectionObject) {
			try {
				connection = connectionObject.Connection;
				if (connection.State == ConnectionState.Open) {
					connection.Close();
				}
				connection.Open();
				connection.Close();
			} catch (Exception) {
				throw;
			}
		}
		
		private DataSet FillDataSet() {
			try {
				if (this.connection.State == ConnectionState.Closed) {
					this.connection.Open();
				}
				OleDbCommand command = ((OleDbConnection)this.connection).CreateCommand();
				command.CommandText = reportSettings.CommandText;
				command.CommandType = reportSettings.CommandType;
				// We have to check if there are parameters for this Query, if so
				// add them to the command
				
				BuildQueryParameters(command,reportSettings);
				
				OleDbDataAdapter adapter = new OleDbDataAdapter(command);
				
				DataSet ds = new DataSet();
				
				ds.Locale = CultureInfo.CurrentCulture;
				adapter.Fill (ds);
				return ds;
			} catch (Exception) {
				throw;
			} finally {
				if (this.connection.State == ConnectionState.Open) {
					this.connection.Close();
				}
			}
		}
		
		
		private static void BuildQueryParameters (OleDbCommand cmd,ReportSettings reportSettings) {
			if (reportSettings.SqlParametersCollection != null && reportSettings.SqlParametersCollection.Count > 0) {
				SqlParameter rpPar;
				OleDbParameter oleDBPar = null;
				
				for (int i = 0;i < reportSettings.SqlParametersCollection.Count ; i++) {
					rpPar  = (SqlParameter)reportSettings.SqlParametersCollection[i];

					if (rpPar.DataType != System.Data.DbType.Binary) {
						oleDBPar = new OleDbParameter(rpPar.ParameterName,
						                              rpPar.DataType);
						oleDBPar.Value = rpPar.ParameterValue;
					} else {
						oleDBPar = new OleDbParameter(rpPar.ParameterName,
						                              System.Data.DbType.Binary);
					}
					
					oleDBPar.Direction = rpPar.ParameterDirection;
					cmd.Parameters.Add(oleDBPar);
					
				}
			}
		}
		
		private void CheckReportColumns() {
			if (this.reportSettings.SortColumnCollection.Count > 0) {
				
				if (this.dataViewStrategy.AvailableFields.Count > 0) {
					foreach (SortColumn col in this.reportSettings.SortColumnCollection) {
						string colName = col.ColumnName;
						AbstractColumn c = this.dataViewStrategy.AvailableFields.Find (colName);
						if (c == null) {
							string str = String.Format (CultureInfo.CurrentCulture,
							                            "<{0}> is not a member of <{1}>",colName,this.reportSettings.ReportName);
							throw new SharpReportException(str);
						}
					}
				}
			}
		}
		
		
		#region Event Handling
		private void NotifyListChanged (object sender, ListChangedEventArgs e) {
			if (this.ListChanged != null) {
				this.ListChanged (this,e);
			}
		}
		
		private void NotifyGroupChanging () {
//			if (this.GroupChanging!= null) {
//				this.GroupChanging (this,EventArgs.Empty);
//			}		
		}
		
		
		private void NotifyGroupChanged() {
			if (this.IsGrouped) {
//				if (this.GroupChanged != null) {
//					this.GroupChanged (this,new GroupChangedEventArgs(this.groupSeperator));
//				}
			}
		}
		
		private void OnGroupChange (object sender,GroupChangedEventArgs e) {
			this.NotifyGroupChanging();
		}
		#endregion
		
		public string DataMember {
			get {
				return dataMember;
			}
		}
		
		public ColumnCollection AvailableFields  {
			get {
				return this.dataViewStrategy.AvailableFields;
			}
			
		}
		
	
		public object DataSource {
			get {
				return this.dataSource;
			}
		}
	
		
		/// <summary>
		/// Returns a <see cref="SharpArrayList"></see>, be carefull, this list is only a Indexlist
		/// to the actuall data
		/// </summary>
		
		
		public SharpIndexCollection ChildRows {
			get {
				return this.dataViewStrategy.ChildRows;
			}
		}
		
		public string Filter {
			get {
				throw new NotImplementedException();
				
			}
			set {
				throw new NotImplementedException();
			}
		}
	
		
		public DataNavigator GetNavigator {
			get {
				return new DataNavigator(this.dataViewStrategy);
			}
		}
		

		
		public bool IsGrouped {
			get {
				return this.dataViewStrategy.IsGrouped;
			}
		}
		
		public bool IsSorted {
			get {
				return this.dataViewStrategy.IsSorted;
			}
		}
		
		public bool IsFiltered {
			get {
				return this.dataViewStrategy.IsFiltered;
			}
		}
		
		#region System.IDisposable interface implementation
		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		
			~DataManager(){
			Dispose(false);
		}
		
		protected virtual void Dispose(bool disposing){
			try {
				if (disposing) {
					// Free other state (managed objects).
					if (this.dataViewStrategy != null) {
						this.dataViewStrategy.Dispose();
					}
				}
			} finally {
				// Release unmanaged resources.
				// Set large fields to null.
				// Call Dispose on your base class.
			}
		}
		#endregion
		
	}
}
