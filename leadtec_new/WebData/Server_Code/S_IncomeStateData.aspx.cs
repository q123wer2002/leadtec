using IncomeStatement.WebData.Server_Code.CommonModule;
using IncomeStatement.WebData.Server_Code.CommonModule.mssql;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IncomeStatement.WebData.Server_Code
{
	public partial class S_IncomeStateData : System.Web.UI.Page
	{
		RequestHandler m_requestHandler;
		string szUserCode;
		string szUserName;
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

			// get user code
			szUserCode = Request.Cookies[ CookieKey.UserID ].Value;
			szUserName = Request.Cookies[ CookieKey.Username ].Value;

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
				if( szAction.ToUpper() == "READ" ) {
					return ApiAction.READ;
				}

				if( szAction.ToUpper() == "CONFIRM" ) {
					return ApiAction.CONFIRM;
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
				if( action == ApiAction.READ ) {
					int nTempValue;

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

						m_paramExpDList.Add($"{TableName.CoFam}.ie_year={szYear} AND {TableName.CoFam}.ie_mon={szMonth}");
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

						m_paramExpDList.Add($"{TableName.CoFam}.fam_no BETWEEN {szFamNoStart} AND {szFamNoEnd}");
					}

					// check record no
					if( Request.Form[ Param.RecNo ] != null ) {
						string szRecNo = Request.Form[ Param.RecNo ].ToString();

						if( int.TryParse(szRecNo, out nTempValue) == false ) {
							return false;
						}

						m_paramExpDList.Add($"{TableName.CoFam}.rec_user LIKE '%{szRecNo}%'");
					}

					// check record no
					if( Request.Form[ Param.AdiUser ] != null ) {
						string szAdiUser = Request.Form[ Param.AdiUser ].ToString();
						m_paramExpDList.Add($"{TableName.CoFam}.adi_user LIKE '%{szAdiUser}%'");
					}

					// check record no
					if( Request.Form[ Param.State ] != null ) {
						string szState = Request.Form[ Param.State ].ToString();
						if( szState != "0" ) {
							m_paramExpDList.Add($"{TableName.CoFam}.state = {szState}");
						}
					}

					// no param
					if( m_paramExpDList.Count == 0 ) {
						return false;
					}

					return true;
				}
				if( action == ApiAction.CONFIRM ) {
					string szConfirmList = Request.Form[ Param.ConfirmList ].ToString();
					string szConfirmType = Request.Form[ Param.ConfirmType ].ToString();
					int.Parse(szConfirmType);
					JArray.Parse(szConfirmList);
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
			if( action == ApiAction.READ ) {
				return ReadData();
			}

			if( action == ApiAction.CONFIRM ) {
				int nType = int.Parse(Request.Form[ Param.ConfirmType ].ToString());
				return isConfirmData(codeToEnum(nType));
			}

			return null;
		}
		dynamic ReadData()
		{
			// get param
			string szSQL = $"SELECT {TableName.CoFamMem}.job_typ_no, {TableName.CoFamMem}.job_no, {TableName.CoFam}.* FROM {TableName.CoFam} " +
				$"LEFT JOIN {TableName.CoFamMem} ON {TableName.CoFamMem}.ie_year={TableName.CoFam}.ie_year AND {TableName.CoFamMem}.ie_mon={TableName.CoFam}.ie_mon AND {TableName.CoFamMem}.fam_no={TableName.CoFam}.fam_no " +
				$" WHERE {TableName.CoFamMem}.title='戶長' AND ";
			for( int i = 0; i < m_paramExpDList.Count; i++ ) {
				szSQL += $" {m_paramExpDList[ i ]}";
				szSQL += i == m_paramExpDList.Count - 1 ? " " : " AND";
			}

			// set auth
			if( Request.Cookies[ CookieKey.UserRole ].Value == "B" ) {
				szSQL += $" AND {TableName.CoFam}.fam_no IN (SELECT fam_no FROM co_rec_fam WHERE rec_user='{szUserCode}')";
			}

			JArray result;
			bool isSuccess = m_mssql.TryQuery(szSQL, out result);
			return result;
		}
		bool isConfirmData( status state )
		{
			// get all need to confirm data
			List<JObject> jObjectList = JsonConvert.DeserializeObject<List<JObject>>(Request.Form[ Param.ConfirmList ].ToString());
			string szWhere = $"fam_no IN({string.Join(", ", jObjectList.Select(data => $"'{data[ "fam_no" ]}'").ToArray())}) ";
			szWhere += $"AND ie_year={jObjectList[ 0 ][ "ie_year" ]} AND ie_mon={jObjectList[ 0 ][ "ie_mon" ]}";
			string szErrorMsg;

			// insert into log
			string szInertCoFamLog = $"INSERT INTO {TableName.CoFamLog} SELECT 'M', CURRENT_TIMESTAMP, '{szUserCode}', * FROM {TableName.CoFam} WHERE {szWhere}";
			string szInsertExMLog = $"INSERT INTO {TableName.CoExpMLog} SELECT 'M', CURRENT_TIMESTAMP, '{szUserCode}', * FROM {TableName.CoExpM} WHERE {szWhere}";
			m_mssql.TryQuery(szInertCoFamLog, out szErrorMsg);
			m_mssql.TryQuery(szInsertExMLog, out szErrorMsg);

			// update co_fam
			string szOtherSetting = "";
			if( state == status.RecordConfirm ) {
				szOtherSetting = $", rec_user='{szUserCode}', rec_name='{szUserName}'";
			}
			else if( state == status.ReviewConfirm ) {
				szOtherSetting = $", adi_user='{szUserCode}', adi_name='{szUserName}'";
			}
			string szUpdateCoFam = $"UPDATE {TableName.CoFam} " +
				$"SET state={(int)state}{szOtherSetting}, upd_date=CURRENT_TIMESTAMP, upd_user='{szUserCode}' " +
				$"WHERE {szWhere}";
			bool isSuccess = m_mssql.TryQuery(szUpdateCoFam, out szErrorMsg);
			if( isSuccess == false ) {
				return false;
			}

			// update co_exp_m
			szOtherSetting = "";
			if( state == status.RecordConfirm ) {
				szOtherSetting = $", rec_date=CURRENT_TIMESTAMP, rec_user='{szUserCode}'";
			}
			else if( state == status.ReviewConfirm ) {
				szOtherSetting = $", adi_date=CURRENT_TIMESTAMP, adi_user='{szUserCode}'";
			}
			string szUpdateCoExpM = $"UPDATE {TableName.CoExpM} " +
				$"SET state={(int)state}{szOtherSetting} , upd_date=CURRENT_TIMESTAMP, upd_user='{szUserCode}'" +
				$"WHERE {szWhere}";
			isSuccess = m_mssql.TryQuery(szUpdateCoFam, out szErrorMsg);
			return isSuccess;
		}
		#endregion

		#region Private Attribute
		enum ApiAction
		{
			READ,
			INSERT,
			CONFIRM,
			UNKNOW,
		}
		enum status:int
		{
			All = 0,
			Inprogress = 1,
			RecordConfirm = 2,
			ReviewConfirm = 3,
		}
		status codeToEnum( int nCode)
		{
			if( nCode == 1 ) {
				return status.Inprogress;
			}

			if( nCode == 2 ) {
				return status.RecordConfirm;
			}

			return status.ReviewConfirm;
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

			// for query data
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
			public static string RecNo
			{
				get
				{
					return "RecNo";
				}
			}
			public static string AdiUser
			{
				get
				{
					return "AdiUser";
				}
			}
			public static string State
			{
				get
				{
					return "State";
				}
			}

			// for confirm data
			public static string ConfirmList
			{
				get
				{
					return "ConfirmList";
				}
			}
			public static string ConfirmType
			{
				get
				{
					return "ConfirmType";
				}
			}

			// for insert
		}
		MSSQL m_mssql = new MSSQL();
		List<string> m_paramExpDList = new List<string>();
		#endregion
	}
}
