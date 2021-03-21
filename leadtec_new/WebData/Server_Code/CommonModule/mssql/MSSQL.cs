using IncomeStatement.WebData.Server_Code.CommonModule.mssql;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace IncomeStatement.WebData.Server_Code.CommonModule
{
	public class MSSQL
	{
		public MSSQL()
		{
			JObject info = ConfigReader.GetConfigFile(ConfigReader.Config.DBConnection);

			// set connection
			m_builder = new SqlConnectionStringBuilder();
			m_builder.DataSource = info[ DBInfo.DataSource ].ToString();
			m_builder.UserID = info[ DBInfo.Username ].ToString();
			m_builder.Password = info[ DBInfo.Password ].ToString();
			m_builder.InitialCatalog = info[ DBInfo.Catalog ].ToString();
			m_builder.ConnectTimeout = 0;
		}

		#region Public Method
		public void CreateEsriTable(string szTablename, List<ColumnModel> colList)
		{
			string szCreateSQL = $"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{szTablename}')";
			szCreateSQL += $"CREATE TABLE {szTablename} ( ";
			for( int i = 0; i < colList.Count; i++ ) {
				ColumnModel col = colList[ i ];
				szCreateSQL += $"{col.ColName} {col.ColType}";
				szCreateSQL += col.IsNull ? "" : " NOT NULL";
				szCreateSQL += col.IsPrimaryKey ? "" : " PRIMARY KEY";
				szCreateSQL += i == colList.Count - 1 ? "" : ", ";
			}
			szCreateSQL += ")";

			string szErrorMsg;
			bool isSuccess = TryQuery(szCreateSQL, out szErrorMsg);
		}
		public bool TryQuery( string szSQL, out string szErrorMsg )
		{
			szErrorMsg = null;
			try {
				string szConnection = m_builder.ConnectionString;
				using( SqlConnection con = new SqlConnection( szConnection ) ) {
					con.Open();
					using( SqlCommand cmd = new SqlCommand(szSQL, con) ) {
						cmd.CommandTimeout = 0;
						int nAffact = cmd.ExecuteNonQuery();
					}
					con.Close();
				}
				return true;
			}
			catch( Exception ex ) {
				szErrorMsg = ex.ToString();
				return false;
			}
		}
		public bool TryQuery( string szSQL, out JArray resultArray )
		{
			try {
				using( SqlConnection con = new SqlConnection(m_builder.ConnectionString) ) {
					con.Open();
					using( SqlCommand cmd = new SqlCommand(szSQL, con) ) {

						// start sql cmd
						resultArray = new JArray();
						using( SqlDataReader reader = cmd.ExecuteReader() ) {

							// read result
							while( reader.Read() ) {
								JObject tempObject = new JObject();
								for( int i = 0; i < reader.FieldCount; i++ ) {
									string szKey = reader.GetName(i);
									string szValue = reader.GetValue(i).ToString();
									if( szKey.Contains("date") && szValue.Length > 0 ) {
										tempObject[ szKey ] = ((DateTime)reader.GetValue(i)).ToString("yyyy/MM/dd HH:mm:ss");
									}
									else {
										tempObject[ szKey ] = szValue;
									}
								}

								resultArray.Add((JToken)tempObject);
							}
						}
					}
					con.Close();
				}

				return true;
			}
			catch (Exception ex){
				string szError = ex.ToString();
				resultArray = null;
				return false;
			}
		}
		#endregion

		#region Public Attribute
		public bool isConnected
		{
			get
			{
				try {
					using( SqlConnection con = new SqlConnection(m_builder.ConnectionString) ) {
						con.Open();
						con.Close();
					}

					return true;
				}
				catch {
					return false;
				}
			}
		}
		#endregion

		#region Private Attribute
		const int MAXINSERT = 999;
		SqlConnectionStringBuilder m_builder;
		const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
		#endregion

		public class DBInfo
		{
			public static string DataSource
			{
				get
				{
					return "DataSource";
				}
			}
			public static string Username
			{
				get
				{
					return "Username";
				}
			}
			public static string Password
			{
				get
				{
					return "Password";
				}
			}
			public static string Catalog
			{
				get
				{
					return "Catalog";
				}
			}
		}
	}
}