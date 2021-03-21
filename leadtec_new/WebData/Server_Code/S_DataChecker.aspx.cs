using IncomeStatement.WebData.Server_Code.CommonModule;
using IncomeStatement.WebData.Server_Code.CommonModule.mssql;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IncomeStatement.WebData.Server_Code
{
	public partial class S_DataChecker : System.Web.UI.Page
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

		ApiAction GetAction()
		{
			try {
				string szAction = Request.Form[ Param.Action ].ToString();
				if( szAction.ToUpper() == "READ" ) {
					return ApiAction.READ;
				}

				if( szAction.ToUpper() == "CHECK" ) {
					return ApiAction.CHECK;
				}

				if( szAction.ToUpper() == "GETCHECKTIME" ) {
					return ApiAction.GetCheckTime;
				}

				return ApiAction.UNKNOW;
			}
			catch {
				return ApiAction.UNKNOW;
			}
		}
		bool isParamValid( ApiAction action )
		{
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

						m_param.Add($"{TableName.CoExpAudit}.ie_year={szYear} AND {TableName.CoExpAudit}.ie_mon={ParseTwoDigital(szMonth)}");
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

						m_param.Add($"{TableName.CoExpAudit}.fam_no BETWEEN {szFamNoStart} AND {szFamNoEnd}");
					}

					if( Request.Form[ Param.CheckType ] != null ) {
						string szCheckType = Request.Form[ Param.CheckType ].ToString();
						if( szCheckType != "0" ) {
							m_param.Add($"{TableName.CoExpAudit}.chk_no='{szCheckType}'");
						}
					}

					if( Request.Form[ Param.CheckTime ] != null ) {
						string szCheckTime = Request.Form[ Param.CheckTime ].ToString();
						m_param.Add($"CAST({TableName.CoExpAudit}.chk_date AS smalldatetime)='{szCheckTime}'");
					}

					return true;
				}
				case ApiAction.CHECK: {
					if(Request.Form[ Param.Year ] == null ||Request.Form[ Param.Month ] == null) {
						return false;
					}

					return true;
				}
				case ApiAction.GetCheckTime:
					return true;
				default:
					return false;
			}
		}
		dynamic DoAction( ApiAction action )
		{
			switch( action ) {
				case ApiAction.READ:
					return ReadData();
				case ApiAction.CHECK:
					return DoChecker();
				case ApiAction.GetCheckTime:
					return GetCheckTime();
				default:
					return null;
			}
		}
		dynamic DoChecker()
		{
			string szYear = Request.Form[ Param.Year ].ToString();
			string szMonth = ParseTwoDigital(Request.Form[ Param.Month ].ToString());

			string szErrorCode;
			string szSQL = $"{PRODUCENAME} '{szYear}', '{szMonth}', '{m_szUserCode}'";
			return m_mssql.TryQuery(szSQL, out szErrorCode);
		}
		dynamic ReadData()
		{
			string szSelectSQL = $"SELECT * FROM {TableName.CoExpAudit} WHERE {TableName.CoExpAudit}.chk_user LIKE '%{m_szUserCode}%' ";
			if( m_param.Count > 0 ) {
				szSelectSQL += "AND ";
				for( int i = 0; i < m_param.Count; i++ ) {
					szSelectSQL += m_param[ i ];
					szSelectSQL += i == m_param.Count - 1 ? " " : "AND ";
				}
			}
			szSelectSQL += $" AND {TableName.CoExpAudit}.chk_user='{m_szUserCode}'";
			szSelectSQL += $" ORDER BY {TableName.CoExpAudit}.chk_date DESC, {TableName.CoExpAudit}.fam_no ASC";

			JArray jResult;
			if( m_mssql.TryQuery(szSelectSQL, out jResult) ) {
				return jResult;
			}

			return null;
		}
		dynamic GetCheckTime()
		{
			string szSQL = $"SELECT DISTINCT chk_date, ie_year, ie_mon FROM {TableName.CoExpAudit} WHERE chk_user='{m_szUserCode}'";
			JArray jResult;
			m_mssql.TryQuery(szSQL, out jResult);
			return jResult;
		}
		string ParseTwoDigital(string szTempString )
		{
			if( szTempString.Length >= 2 ) {
				return szTempString;
			}

			return $"0{szTempString}";
		}

		enum ApiAction
		{
			GetCheckTime,
			READ,
			CHECK,
			UNKNOW,
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
			public static string CheckNo
			{
				get
				{
					return "CheckNo";
				}
			}
			public static string CheckType
			{
				get
				{
					return "CheckType";
				}
			}
			public static string CheckTime
			{
				get
				{
					return "ChechTime";
				}
			}
		}

		const string PRODUCENAME= "p_co_audit";
		MSSQL m_mssql = new MSSQL();
		List<string> m_param = new List<string>();
	}
}
