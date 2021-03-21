using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace IncomeStatement.WebData.Server_Code.CommonModule
{
	public class ConfigReader
	{
		#region Public Method
		public static JObject GetConfigFile( Config config )
		{
			string szConfigName = GetConfigName( config );

			// no this config type
			if( szConfigName.Length == 0 ) {
				return null;
			}

			string szFileContent = ReadConfigJsonString(szConfigName)
				.Replace("\t", "")
				.Replace("\r\n", "");

			return JObject.Parse(szFileContent);
		}
		public static string GetConnection()
		{
			JObject jConn = GetConfigFile( Config.Connection );
			if( jConn == null ) {
				return string.Empty;
			}

			// for private
			string szTempIP = jConn[ "IP" ].ToString();
			string szTempPort = jConn[ "Port" ].ToString();
			if( szTempIP.Length == 0 || szTempPort.Length == 0 ) {
				szTempIP = ConnectionInfo.szConnectIP;
				szTempPort = ConnectionInfo.szConnectPort;
			}

			return szTempIP + ":" + szTempPort;
		}
		#endregion

		#region Public Attribute
		public enum Config
		{
			DBConnection,
			Connection,
			Report,
		}
		#endregion

		#region Private Method
		static string GetConfigName( Config config )
		{
			switch( config ) {
				case Config.DBConnection:
					return "dbConnection.json";
				case Config.Connection:
					return "connection.json";
				case Config.Report:
					return "report.json";
			}

			return string.Empty;
		}
		static string ReadConfigJsonString( string szConfigFileName )
		{
			// only accept json in Object, not allow Array
			string szConfigFilePath = m_szConfigPath + szConfigFileName;
			string szTotalString = "";
			if( File.Exists( szConfigFilePath ) == false ) {
				return "{}";
			}

			// read config file
			using( StreamReader file = new StreamReader(szConfigFilePath) ) {
				szTotalString += file.ReadToEnd();
			}

			return szTotalString;
		}
		#endregion

		#region Private Attribute
		static string m_szConfigPath = FileLocation.ConfigPath;
		#endregion
	}
}
