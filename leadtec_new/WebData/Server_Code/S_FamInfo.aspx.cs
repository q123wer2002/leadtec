using IncomeStatement.WebData.Server_Code.CommonModule;
using IncomeStatement.WebData.Server_Code.CommonModule.mssql;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IncomeStatement.WebData.Server_Code
{
	public partial class S_FamInfo : System.Web.UI.Page
	{
		RequestHandler m_requestHandler;
		string szUserCode;
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

			// get user id
			szUserCode = Request.Cookies[ CookieKey.UserID ].Value;

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

				if( szAction.ToUpper() == "UPDATE" ) {
					return ApiAction.UPDATE;
				}

				if( szAction.ToUpper() == "INSERT" ) {
					return ApiAction.INSERT;
				}

				if( szAction.ToUpper() == "DELETE" ) {
					return ApiAction.DELETE;
				}

				return ApiAction.UNKNOW;
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
						if( Request.Form[ Param.FamNoStart ] != null && Request.Form[ Param.FamNoEnd ] != null ) {
							string szFamNoStart = Request.Form[ Param.FamNoStart ].ToString();
							string szFamNoEnd = Request.Form[ Param.FamNoEnd ].ToString();

							if(
								int.TryParse(szFamNoStart, out nTempValue) == false ||
								int.TryParse(szFamNoEnd, out nTempValue) == false
							) {
								return false;
							}

							m_param.Add($"{TableName.CoRecFam}.fam_no BETWEEN {szFamNoStart} AND {szFamNoEnd}");
						}

						if( Request.Form[ Param.ChechInNo ] != null ) {
							string szCheckNo = Request.Form[ Param.ChechInNo ].ToString();
							m_param.Add($"{TableName.CoRecFam}.rec_user LIKE '%{szCheckNo}%'");
						}

						if( Request.Form[ Param.ReviewNo ] != null ) {
							string szReviewNo = Request.Form[ Param.ReviewNo ].ToString();
							m_param.Add($"{TableName.CoRecFam}.adi_user LIKE '%{szReviewNo}%'");
						}

						return true;
					}
					case ApiAction.DELETE:
						JObject jFamObj = JObject.Parse(Request.Form[ Param.FamObject ].ToString());
						return true;
					case ApiAction.UPDATE:
					case ApiAction.INSERT:
						JArray jFamArray = JArray.Parse(Request.Form[ Param.FamArray ].ToString());
						return true;
				}
				return false;
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
				case ApiAction.UPDATE:
					return UpdateFam();
				case ApiAction.DELETE:
					return DeleteFam();
				case ApiAction.INSERT:
					return InsertFam();
				default:
					return null;
			}
		}
		dynamic ReadData()
		{
			string szSelectSQL = $"SELECT {TableName.CoRecFam}.*, t1.user_name as rec_name, t2.user_name as adi_name, a1.state as rec_status, a2.state as adi_status FROM {TableName.CoRecFam} " +
				$"LEFT JOIN {TableName.CoSysUser} t1 ON t1.user_id={TableName.CoRecFam}.rec_user " +
				$"LEFT JOIN {TableName.CoSysAuth} a1 ON a1.user_id={TableName.CoRecFam}.rec_user " + 
				$"LEFT JOIN {TableName.CoSysUser} t2 ON t2.user_id={TableName.CoRecFam}.adi_user " + 
				$"LEFT JOIN {TableName.CoSysAuth} a2 ON a2.user_id={TableName.CoRecFam}.adi_user ";

			if( m_param.Count > 0 ) {
				szSelectSQL += "WHERE ";
				for( int i = 0; i < m_param.Count; i++ ) {
					szSelectSQL += m_param[ i ];
					szSelectSQL += i == m_param.Count - 1 ? " " : " AND ";
				}
			}

			szSelectSQL += $"ORDER BY {TableName.CoRecFam}.fam_no";

			JArray jResult;
			m_mssql.TryQuery(szSelectSQL, out jResult);
			return jResult;
		}
		dynamic InsertFam()
		{
			List<JObject> jFamList = JsonConvert.DeserializeObject<List<JObject>>(Request.Form[ Param.FamArray ].ToString());
			string szInsertSQL = $"INSERT INTO {TableName.CoRecFam} VALUES ";
			for( int i = 0; i < jFamList.Count; i++ ) {
				JObject jFamObj = jFamList[ i ];
				string szRecUser = jFamObj[ "rec_user" ].ToString().Length == 0 ? "NULL" : $"'{jFamObj[ "rec_user" ].ToString()}'";
				string szAdiUser = jFamObj[ "adi_user" ].ToString().Length == 0 ? "NULL" : $"'{jFamObj[ "adi_user" ].ToString()}'";
				string szState = jFamObj[ "state" ].ToString().Length == 0 ? "'1'" : $"'{jFamObj[ "state" ].ToString()}'";

				szInsertSQL += $"({szRecUser}, {szAdiUser}, '{jFamObj[ "fam_no" ].ToString()}', {szState}, CURRENT_TIMESTAMP, '{szUserCode}', NULL, NULL)";
				szInsertSQL += i == jFamList.Count - 1 ? " " : ", ";
			}
			string szErrorMsg;
			return m_mssql.TryQuery(szInsertSQL, out szErrorMsg);
		}
		dynamic UpdateFam()
		{
			List<JObject> jFamList = JsonConvert.DeserializeObject<List<JObject>>(Request.Form[ Param.FamArray ].ToString());
			bool isSuccess = false;
			for( int i = 0; i < jFamList.Count; i++ ) {
				JObject jFam = jFamList[ i ];
				string szRecUser = jFam[ "rec_user" ].ToString().Length == 0 ? "NULL" : $"'{jFam[ "rec_user" ].ToString()}'";
				string szAdiUser = jFam[ "adi_user" ].ToString().Length == 0 ? "NULL" : $"'{jFam[ "adi_user" ].ToString()}'";
				string szState = jFam[ "state" ].ToString().Length == 0 ? "'1'" : $"'{jFam[ "state" ].ToString()}'";
				string szErrorMsg;
				isSuccess = m_mssql.TryQuery($"UPDATE {TableName.CoRecFam} SET adi_user={szAdiUser}, state={szState}, rec_user={szRecUser}, upd_date=CURRENT_TIMESTAMP, upd_user='{szUserCode}' WHERE fam_no='{jFam[ "fam_no" ].ToString()}'", out szErrorMsg);

				if( isSuccess == false ) {
					return false;
				}
			}

			return true;
		}
		dynamic DeleteFam()
		{
			JObject jFamObject = JObject.Parse(Request.Form[ Param.FamObject ].ToString());
			string szErrorMsg;
			return m_mssql.TryQuery($"DELETE FROM {TableName.CoRecFam} WHERE rec_user='{jFamObject[ "rec_user" ].ToString()}' AND fam_no='{jFamObject[ "fam_no" ].ToString()}'", out szErrorMsg);
		}

		enum ApiAction
		{
			READ,
			UPDATE,
			DELETE,
			INSERT,
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
			public static string ChechInNo
			{
				get
				{
					return "ChechInNo";
				}
			}
			public static string ReviewNo
			{
				get
				{
					return "ReviewNo";
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

			// for update, delete
			public static string FamObject
			{
				get
				{
					return "FamObject";
				}
			}

			//for insert
			public static string FamArray
			{
				get
				{
					return "FamArray";
				}
			}
		}
		MSSQL m_mssql = new MSSQL();
		List<string> m_param = new List<string>();
	}
}
