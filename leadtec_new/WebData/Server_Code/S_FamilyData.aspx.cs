using IncomeStatement.WebData.Server_Code.CommonModule;
using IncomeStatement.WebData.Server_Code.CommonModule.mssql;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IncomeStatement.WebData.Server_Code
{
	public partial class S_FamilyData : System.Web.UI.Page
	{
		RequestHandler m_requestHandler;
		string m_szUserCode;
		protected void Page_Load( object sender, EventArgs e )
		{
			// check status, set default
			m_requestHandler = new RequestHandler();
			m_requestHandler.StatusCode = (int)ErrorCode.Error;
			m_requestHandler.ReturnData = "Param Error";

			ErrorCode ec = m_requestHandler.RequestValid(Request);
			if( ec != ErrorCode.Success ) {
				m_requestHandler.StatusCode = (int)ec;
				m_requestHandler.ReturnData = ec.ToString();
				Response.Write(m_requestHandler.GetReturnResult());
				return;
			}

			// get user info
			m_szUserCode = Request.Cookies[ CookieKey.UserID ].Value;

			// check action
			ApiAction action = GetAction();
			if( action == ApiAction.UNKNOW ) {
				Response.Write(m_requestHandler.GetReturnResult());
				return;
			}

			// check param valid
			if( isParamValid(action) == false ) {
				Response.Write(m_requestHandler.GetReturnResult());
				return;
			}

			// do action
			dynamic result = DoAction(action);
			m_requestHandler.StatusCode = (int)ErrorCode.Success;
			m_requestHandler.ReturnData = result;
			Response.Write(m_requestHandler.GetReturnResult());
		}
		protected void Page_Error( object sender, EventArgs e )
		{
			// get error
			Exception ex = Server.GetLastError();

			// return
			m_requestHandler.StatusCode = (int)ErrorCode.Error;
			m_requestHandler.ReturnData = (ConnectionInfo.isDebugMode) ? ex.ToString() : string.Empty;
			Response.Write(m_requestHandler.GetReturnResult());
			Server.ClearError();
		}

		#region Private Method
		ApiAction GetAction()
		{
			try {
				string szAction = Request.Form[ Param.Action ].ToString();
				switch( szAction.ToUpper() ) {
					case "READ":
						return ApiAction.READ;
					case "DELETE":
						return ApiAction.DELETE;
					case "UPDATE":
						return ApiAction.UPDATE;
					default:
						return ApiAction.UNKNOW;
				}
			}
			catch {
				return ApiAction.UNKNOW;
			}
		}
		bool isParamValid( ApiAction action )
		{
			try {
				int nTempValue;

				switch( action ) {
					case ApiAction.READ: {
						// check data
						if( Request.Form[ Param.Year ] != null && Request.Form[ Param.Month ] != null ) {
							string szYear = Request.Form[ Param.Year ].ToString();
							string szMonth = Request.Form[ Param.Month ].ToString();

							if(
								int.TryParse(szYear, out nTempValue) == false ||
								int.TryParse(szMonth, out nTempValue) == false
							) {
								return false;
							}

							m_paramList.Add($"{TableName.CoFam}.ie_year={szYear} AND {TableName.CoFam}.ie_mon={szMonth}");
						}

						// check family no
						if( Request.Form[ Param.FamNoStart ] != null && Request.Form[ Param.FamNoEnd ] != null ) {
							string szFamNoStart = Request.Form[ Param.FamNoStart ].ToString();
							string szFamNoEnd = Request.Form[ Param.FamNoEnd ].ToString();

							if(
								int.TryParse(szFamNoStart, out nTempValue) == false ||
								int.TryParse(szFamNoEnd, out nTempValue) == false
							) {
								return false;
							}

							m_paramList.Add($"{TableName.CoFam}.fam_no BETWEEN {szFamNoStart} AND {szFamNoEnd}");
						}

						// check record no
						if( Request.Form[ Param.RecUser ] != null ) {
							string szRecUser = Request.Form[ Param.RecUser ].ToString();
							m_paramList.Add($"{TableName.CoFam}.rec_user LIKE '%{szRecUser}%'");
						}

						// check record no
						if( Request.Form[ Param.AdiUser ] != null ) {
							string szAdiUser = Request.Form[ Param.AdiUser ].ToString();
							m_paramList.Add($"{TableName.CoFam}.adi_user LIKE '%{szAdiUser}%'");
						}

						// no param
						if( m_paramList.Count == 0 ) {
							return false;
						}

						return true;
					}
					case ApiAction.DELETE: {
						// check data
						if( Request.Form[ Param.Year ] != null && Request.Form[ Param.Month ] != null ) {
							string szYear = Request.Form[ Param.Year ].ToString();
							string szMonth = Request.Form[ Param.Month ].ToString();

							if(
								int.TryParse(szYear, out nTempValue) == false ||
								int.TryParse(szMonth, out nTempValue) == false
							) {
								return false;
							}

							m_paramList.Add($"{TableName.CoFam}.ie_year={szYear} AND {TableName.CoFam}.ie_mon={szMonth}");
						}

						if( Request.Form[ Param.FamilyDataList ] != null ) {
							JArray jData = JArray.Parse(Request.Form[ Param.FamilyDataList ].ToString());
						}

						if( Request.Form[ Param.FamMemData ] != null ) {
							JArray jDataAry = JArray.Parse(Request.Form[ Param.FamMemData ].ToString());
						}
						return true;
					}
					case ApiAction.UPDATE: {
						
						if( Request.Form[ Param.FamData ] != null ) {
							JArray jDataAry = JArray.Parse(Request.Form[ Param.FamData ].ToString());
						}
						else if( Request.Form[ Param.FamMemData ] != null ) {
							JArray jDataAry = JArray.Parse(Request.Form[ Param.FamMemData ].ToString());
						}
						else {
							return false;
						}
						
						return true;
					}
					default:
						return false;
				}
			}
			catch {
				return false;
			}
		}
		dynamic DoAction( ApiAction action )
		{
			switch( action ) {
				case ApiAction.READ:
					return ReadData();
				case ApiAction.DELETE:
					return DeleteData();
				case ApiAction.UPDATE:
					return UpdateData();
				default:
					return null;
			}
		}
		dynamic ReadData()
		{
			JObject jResult = new JObject();

			// get co_fam
			string szSQL = $"SELECT {TableName.CoFam}.* FROM {TableName.CoFam} WHERE ";
			for( int i = 0; i < m_paramList.Count; i++ ) {
				szSQL += $" {m_paramList[ i ]}";
				szSQL += i == m_paramList.Count - 1 ? " " : " AND";
			}

			// set auth
			if( Request.Cookies[ CookieKey.UserRole ].Value == "B" ) {
				szSQL += $" AND {TableName.CoFam}.fam_no IN (SELECT fam_no FROM co_rec_fam WHERE rec_user='{Request.Cookies[ CookieKey.UserID ].Value}')";
			}

			JArray result;
			bool isSuccess = m_mssql.TryQuery(szSQL, out result);
			jResult[ "co_fam" ] = result;

			// get co_fam_mem
			szSQL = $"SELECT * FROM {TableName.CoFamMem} " +
				$"WHERE fam_no IN ({string.Join(", ", result.Select(obj => $"'{obj[ "fam_no" ].ToString()}'"))})";

			// set filter
			if( Request.Form[ Param.Year ] != null && Request.Form[ Param.Month ] != null ) {
				szSQL += $" AND ie_year={Request.Form[ Param.Year ]} AND ie_mon={Request.Form[ Param.Month ]}";
			}

			isSuccess = m_mssql.TryQuery(szSQL, out result);
			jResult[ "co_fam_mem" ] = result;

			return jResult;
		}
		bool DeleteData()
		{
			if( Request.Form[ Param.FamMemData ] != null ) {
				return DeleteCoFamMemData();
			}

			List<JObject> famDataList = JsonConvert.DeserializeObject<List<JObject>>(Request.Form[ Param.FamilyDataList ].ToString());
			string szWhere = $"{TableName.CoFam}.fam_no IN ({string.Join(", ", famDataList.Select(famObj => $"'{famObj[ "fam_no" ]}'"))}) AND ";
			string szErrorMsg;
			for( int i = 0; i < m_paramList.Count; i++ ) {
				szWhere += $" {m_paramList[ i ]}";
				szWhere += i == m_paramList.Count - 1 ? " " : " AND";
			}

			// copy currnt data into log file
			string szInsertLog = $"INSERT INTO {TableName.CoFamLog} SELECT 'D', CURRENT_TIMESTAMP, '{m_szUserCode}', * FROM {TableName.CoFam} WHERE {szWhere}";
			bool isSuccess = m_mssql.TryQuery(szInsertLog, out szErrorMsg);
			if( isSuccess == false ) {
				return false;
			}

			// delete items
			string szDelete = $"DELETE FROM {TableName.CoFam} WHERE {szWhere}";
			isSuccess = m_mssql.TryQuery(szDelete, out szErrorMsg);
			return isSuccess;
		}
		bool DeleteCoFamMemData()
		{
			List<JObject> jDataList = JsonConvert.DeserializeObject<List<JObject>>(Request.Form[ Param.FamMemData ].ToString());
			bool isSuccess = false;
			for( int i = 0; i < jDataList.Count; i++ ) {
				JObject jData = jDataList[ i ];
				List<string> keys = jData.Properties().Select(p => p.Name).ToList();
				string szErrorMsg;
				string szWhere = $"{TableName.CoFamMem}.ie_year='{jData[ "ie_year" ].ToString()}' " +
					$"AND {TableName.CoFamMem}.ie_mon='{jData[ "ie_mon" ].ToString()}' " +
					$"AND {TableName.CoFamMem}.fam_no='{jData[ "fam_no" ].ToString()}' " +
					$"AND {TableName.CoFamMem}.mem_no='{jData[ "mem_no" ].ToString()}' ";

				// insert into log
				string szInsertLog = $"INSERT INTO {TableName.CoFamMemLog} SELECT 'D', CURRENT_TIMESTAMP, '{m_szUserCode}', * FROM {TableName.CoFamMem} WHERE {szWhere}";
				isSuccess = m_mssql.TryQuery(szInsertLog, out szErrorMsg);
				if( isSuccess == false ) {
					break;
				}

				// delete items
				isSuccess = m_mssql.TryQuery($"DELETE FROM {TableName.CoFamMem} WHERE {szWhere}", out szErrorMsg);

				if( isSuccess == false ) {
					return false;
				}
			}

			return true;
			
		}
		bool UpdateData()
		{
			if( Request.Form[ Param.FamData ] != null ) {
				return UpdateCoFamData();
			}

			if( Request.Form[ Param.FamMemData ] != null ) {
				return UpdateCoFamMemData();
			}

			return false;
		}
		bool UpdateCoFamData()
		{
			List<JObject> jDataList = JsonConvert.DeserializeObject<List<JObject>>(Request.Form[ Param.FamData ].ToString());
			List<string> ignoreKeys = new List<string>()
			{
				"ie_year",
				"ie_mon",
				"fam_no",
				"state",
				"crt_date",
				"crt_user",
				"upd_date",
				"upd_user",
			};

			bool isSuccess = false;
			string szErrorMsg;
			for( int i = 0; i < jDataList.Count; i++ ) {
				JObject jData = jDataList[ i ];
				string szWhere = $"{TableName.CoFam}.ie_year='{jData[ "ie_year" ].ToString()}' " +
					$"AND {TableName.CoFam}.ie_mon='{ParseTwoDigital(jData[ "ie_mon" ].ToString())}' " +
					$"AND {TableName.CoFam}.fam_no='{jData[ "fam_no" ].ToString()}' ";

				// select once
				JArray jResult;
				m_mssql.TryQuery($"SELECT * FROM {TableName.CoFam} WHERE {szWhere}", out jResult);
				if( jResult.Count == 0 ) {
					// means insert
					jData[ "crt_date" ] = "CURRENT_TIMESTAMP";
					jData[ "crt_user" ] = m_szUserCode;
					jData[ "ie_mon" ] = ParseTwoDigital(jData[ "ie_mon" ].ToString());
					string szInsertAa = $"INSERT INTO {TableName.CoFam} ({string.Join(", ", jData.Properties().Select(p => p.Name).ToList())}) VALUES ({string.Join(", ", jData.Properties().Select(p => p.Value.ToString() == "CURRENT_TIMESTAMP" ? "CURRENT_TIMESTAMP" : p.Value.ToString().Length == 0 || p.Value.ToString() == "\r" ? "NULL" : $"'{p.Value}'").ToList())})";
					isSuccess = m_mssql.TryQuery(szInsertAa, out szErrorMsg);
					if( isSuccess == false ) {
						return false;
					}
				}
				else {
					List<string> keys = jData.Properties().Select(p => p.Name).ToList();

					// update
					List<string> setSQLList = new List<string>();
					for( int j = 0; j < keys.Count; j++ ) {
						if( ignoreKeys.Contains(keys[ j ]) ) {
							continue;
						}
						string szOriginalValue = jData[ keys[ j ] ].ToString();
						string szKey = keys[ j ];
						string szValue = szOriginalValue == "\r" || szOriginalValue.Length == 0 ? "NULL" : $"'{jData[ keys[ j ] ].ToString()}'";

						setSQLList.Add($"{szKey}={szValue}");
					}
					string szSetSql = string.Join(", ", setSQLList.Select(sql => sql));

					// copy currnt data into log file
					string szInsertLog = $"INSERT INTO {TableName.CoFamLog} SELECT 'M', CURRENT_TIMESTAMP, '{m_szUserCode}', * FROM {TableName.CoFam} WHERE {szWhere}";
					isSuccess = m_mssql.TryQuery(szInsertLog, out szErrorMsg);
					if( isSuccess == false ) {
						return false;
					}

					// update items
					string szUpdate = $"UPDATE {TableName.CoFam} SET upd_date=CURRENT_TIMESTAMP, upd_user='{m_szUserCode}', {szSetSql} WHERE {szWhere}";
					isSuccess = m_mssql.TryQuery(szUpdate, out szErrorMsg);
					if( isSuccess == false ) {
						return false;
					}
				}
			}

			return true;
		}
		bool UpdateCoFamMemData()
		{
			List<JObject> jDataList = JsonConvert.DeserializeObject<List<JObject>>(Request.Form[ Param.FamMemData ].ToString());
			List<string> ignoreKeys = new List<string>()
			{
				"ie_year",
				"ie_mon",
				"fam_no",
				"crt_date",
				"crt_user",
				"upd_date",
				"upd_user",
			};

			bool isSuccess = false;
			for( int i = 0; i < jDataList.Count; i++ ) {
				JObject jData = jDataList[ i ];
				
				string szErrorMsg;
				string szWhere = $"{TableName.CoFamMem}.ie_year='{jData[ "ie_year" ].ToString()}' " +
					$"AND {TableName.CoFamMem}.ie_mon='{jData[ "ie_mon" ].ToString()}' " +
					$"AND {TableName.CoFamMem}.fam_no='{jData[ "fam_no" ].ToString()}' " +
					$"AND {TableName.CoFamMem}.mem_no='{jData[ "mem_no" ].ToString()}' ";

				List<string> Add0Keys = new List<string>() { "bir_mon", "edu_no" };

				// select once
				JArray jResult;
				m_mssql.TryQuery($"SELECT * FROM {TableName.CoFamMem} WHERE {szWhere}", out jResult);
				if( jResult.Count == 0 ) {
					// means insert
					jData[ "crt_date" ] = "CURRENT_TIMESTAMP";
					jData[ "crt_user" ] = m_szUserCode;
					jData[ "ie_mon" ] = ParseTwoDigital(jData[ "ie_mon" ].ToString());
					isSuccess = m_mssql.TryQuery($@"
						INSERT INTO {TableName.CoFamMem} ({string.Join(", ", jData.Properties().Select(p => p.Name).ToList())}) 
						VALUES ({
							string.Join(
								", ",
								jData.Properties().Select(
									p => p.Value.ToString() == "CURRENT_TIMESTAMP"
										? "CURRENT_TIMESTAMP"
										: p.Value.ToString().Length == 0 || p.Value.ToString() == "\r"
											? "NULL"
											: Add0Keys.Contains(p.Name)
												? $"'{ParseTwoDigital(p.Value.ToString())}'"
												: $"'{p.Value}'"
								).ToList()
							)
						}) ", out szErrorMsg);
				}
				else {
					List<string> keys = jData.Properties().Select(p => p.Name).ToList();

					// means update
					List<string> setSQLList = new List<string>();
					for( int j = 0; j < keys.Count; j++ ) {
						if( ignoreKeys.Contains(keys[ j ]) ) {
							continue;
						}
						string szKey = keys[ j ];
						string szValue;

						if( Add0Keys.Contains(szKey) ) {
							szValue = jData[ keys[ j ] ].ToString().Length == 0 ? "NULL" : $"'{ParseTwoDigital(jData[ keys[ j ] ].ToString())}'";
						}
						else {
							szValue = jData[ keys[ j ] ].ToString().Length == 0 ? "NULL" : $"'{jData[ keys[ j ] ].ToString()}'";
						}

						setSQLList.Add($"{szKey}={szValue}");
					}
					string szSetSql = string.Join(", ", setSQLList.Select(sql => sql));

					// copy currnt data into log file
					string szInsertLog = $"INSERT INTO {TableName.CoFamMemLog} SELECT 'M', CURRENT_TIMESTAMP, '{m_szUserCode}', * FROM {TableName.CoFamMem} WHERE {szWhere}";
					isSuccess = m_mssql.TryQuery(szInsertLog, out szErrorMsg);
					if( isSuccess == false ) {
						break;
					}

					// update items
					string szUpdate = $"UPDATE {TableName.CoFamMem} SET upd_date=CURRENT_TIMESTAMP, upd_user='{m_szUserCode}', {szSetSql} WHERE {szWhere}";
					isSuccess = m_mssql.TryQuery(szUpdate, out szErrorMsg);
				}

				if( isSuccess == false ) {
					return false;
				}
			}

			return true;
		}
		string ParseTwoDigital( string szTempString )
		{
			if( szTempString.Length >= 2 ) {
				return szTempString;
			}

			return $"0{szTempString}";
		}
		#endregion

		#region Private Attribute
		enum ApiAction
		{
			READ,
			UPDATE,
			INSERT,
			DELETE,
			UNKNOW
		}
		class Param
		{
			public static string Action
			{
				get
				{
					return "Action";
				}
			}

			// for read
			public static string Year
			{
				get
				{
					return "Year";
				}
			}
			public static string Month
			{
				get
				{
					return "Month";
				}
			}
			public static string FamNoStart
			{
				get
				{
					return "FamNoStart";
				}
			}
			public static string FamNoEnd
			{
				get
				{
					return "FamNoEnd";
				}
			}
			public static string RecUser
			{
				get
				{
					return "RecUser";
				}
			}
			public static string AdiUser
			{
				get
				{
					return "AdiUser";
				}
			}

			// for delete
			public static string FamilyDataList
			{
				get
				{
					return "FamilyDataList";
				}
			}

			// for update
			public static string FamData
			{
				get
				{
					return "FamData";
				}
			}
			public static string FamMemData
			{
				get
				{
					return "FamMemData";
				}
			}
		}
		MSSQL m_mssql = new MSSQL();
		List<string> m_paramList = new List<string>();
		#endregion
	}
}
