using IncomeStatement.WebData.Server_Code.CommonModule;
using IncomeStatement.WebData.Server_Code.CommonModule.mssql;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Web;

namespace IncomeStatement.WebData.Server_Code
{
	public partial class S_LoginChecker : System.Web.UI.Page
	{
		RequestHandler m_requestHandler;
		string szUserId = "";
		string szRole = "";
		int m_nErrorCount = 0;
		AccountErrorCode aErrorCode = AccountErrorCode.success;
		JObject jReturnMsg = new JObject();
		protected void Page_Load( object sender, EventArgs e )
		{
			m_requestHandler = new RequestHandler();

			//set default response
			m_requestHandler.StatusCode = (int)ErrorCode.Error;
			m_requestHandler.ReturnData = string.Empty;

			//get user typing
			string szUserName = Request.Form[ Param.Username ].ToString();
			string szUserPassword = Request.Form[ Param.Password ].ToString();
			DateTime ExpireTime = DateTime.Now.AddDays(1d);

			JObject jUserInfo;
			if( isLoginSuccess(szUserName, szUserPassword, out jUserInfo) ) {
				// check account status
				string szState = jUserInfo[ "account" ][ "state" ].ToString();
				if( szState == "0" ) {
					aErrorCode = AccountErrorCode.accountDisabled;
				}
				else if( szState == "2" ) {
					aErrorCode = AccountErrorCode.accountLock;
				}
				else {
					// check account expired
					if( jUserInfo[ "account" ][ "exp_date" ] != null && jUserInfo[ "account" ][ "exp_date" ].ToString().Length > 0 ) {
						DateTime dtExpire = DateTime.Parse(jUserInfo[ "account" ][ "exp_date" ].ToString());
						TimeSpan tDiff = (dtExpire - DateTime.Now);
						if( tDiff.TotalDays < -1 ) {
							aErrorCode = AccountErrorCode.accountExpired;
						}
						else if( tDiff.TotalDays <= 30 ) {
							aErrorCode = AccountErrorCode.tobeExpired;
							jReturnMsg[ "expiredDate" ] = dtExpire.ToShortDateString();
						}
					}

					// change password
					if( jUserInfo[ "account" ][ "chg_pwd" ].ToString() == "Y" ) {
						aErrorCode = AccountErrorCode.changePassword;
					}
				}
			}

			// write login log
			bool isNeedToLock;
			LogAndCheckLock(aErrorCode, out isNeedToLock);
			if( isNeedToLock ) {
				aErrorCode = AccountErrorCode.accountLock;
			}

			if( (int)aErrorCode >= 0 ) {
				//create token 
				string szJWTToken = JWTChecker.CreateNewJWTObjectString(szUserName);
				Response.Cookies[ CookieKey.JWTName ].Value = szJWTToken;
				// Response.Cookies[ CookieKey.JWTName ].Expires = ExpireTime;
				Response.Cookies[ CookieKey.UserID ].Value = szUserId;
				// Response.Cookies[ CookieKey.UserID ].Expires = ExpireTime;
				Response.Cookies[ CookieKey.Username ].Value = szUserName;
				// Response.Cookies[ CookieKey.Username ].Expires = ExpireTime;
				Response.Cookies[ CookieKey.UserRole ].Value = szRole;
				// Response.Cookies[ CookieKey.UserRole ].Expires = ExpireTime;
				Response.Cookies[ CookieKey.Nickname ].Value = Convert.ToBase64String( System.Text.Encoding.UTF8.GetBytes( jUserInfo[ "user" ][ "user_name" ].ToString() ) );
			}

			//success
			m_requestHandler.StatusCode = (int)aErrorCode;
			m_requestHandler.ReturnData = GetReturnData(aErrorCode);
			Response.Write(m_requestHandler.GetReturnResult());
		}

		bool isParamValid()
		{
			try {
				string szUsername = Request.Form[ Param.Username ].ToString();
				string szPassword = Request.Form[ Param.Password ].ToString();
				return true;
			}
			catch {
				return false;
			}
		}
		bool isLoginSuccess( string szUserName, string szUserPassword, out JObject jUserInfo )
		{
			jUserInfo = new JObject();

			// get account info
			string szAccountInfo = $"SELECT * FROM {TableName.CoSysAuth} WHERE login_id='{szUserName}'";
			JArray jResult;
			m_mssql.TryQuery(szAccountInfo, out jResult);
			if( jResult == null || jResult.Count != 1 ) {
				aErrorCode = AccountErrorCode.noAccount;
				return false;
			}

			// check password
			jUserInfo[ "account" ] = (JObject)jResult[ 0 ];
			m_nErrorCount = int.Parse(jUserInfo[ "account" ][ "err_cnt" ].ToString());
			string szDBPassword = jUserInfo[ "account" ][ "pwd" ].ToString();
			if( szDBPassword != szUserPassword ) {
				aErrorCode = AccountErrorCode.passwordError;
				return false;
			}

			// reset error count
			string szErrorMsg;
			string szResetErrorCount = $"UPDATE {TableName.CoSysAuth} SET err_cnt=0 WHERE user_id='{szUserName}'";
			m_mssql.TryQuery(szResetErrorCount, out szErrorMsg);

			// get user info
			string szUserInfo = $"SELECT * FROM {TableName.CoSysUser} WHERE user_id='{szUserName}'";
			JArray jResult2;
			m_mssql.TryQuery( szUserInfo, out jResult2 );
			jUserInfo[ "user" ] = ( jResult2 == null) ? null : (JObject)jResult2[ 0 ];

			// assign local var
			szUserId = jUserInfo[ "user" ][ "user_id" ].ToString();
			szUserName = jUserInfo[ "user" ][ "user_name" ] == null ? szUserId : jUserInfo[ "user" ][ "user_name" ].ToString();
			szRole = jUserInfo[ "account" ][ "role" ].ToString();
			return true;
		}
		void LogAndCheckLock( AccountErrorCode errorcode, out bool isLock )
		{
			isLock = false;

			string szUserId = Request.Form[ Param.Username ].ToString();
			string szIP = Request.Form[ Param.IP ].ToString();
			string szErrorMsg;

			if( (int)errorcode > 0 ) {
				// write success login
				string szLogAuth = $"UPDATE {TableName.CoSysAuth} SET log_date=CURRENT_TIMESTAMP WHERE login_id='{szUserId}'";
				m_mssql.TryQuery(szLogAuth, out szErrorMsg);
			}

			// write log into co_sys_log
			string szLogSys = $"INSERT INTO {TableName.CoSysLog} VALUES (CURRENT_TIMESTAMP, '{szUserId}', '{szIP}', '{ParseErrorLog(errorcode)}', N'' )";
			m_mssql.TryQuery(szLogSys, out szErrorMsg);

			// record error count
			if( errorcode == AccountErrorCode.passwordError ) {
				m_nErrorCount = m_nErrorCount + 1;

				// get max error number
				int nMaxErrorCount = 3; // default
				JArray jErrorNumber;
				string szGetMaxErrTimes = $"SELECT * FROM {TableName.CoParam} WHERE par_typ='SYS' AND par_no='S002'";
				if( m_mssql.TryQuery(szGetMaxErrTimes, out jErrorNumber) && jErrorNumber.Count > 0 ) {
					nMaxErrorCount = int.Parse(jErrorNumber[ 0 ][ "par_val" ].ToString());
				}

				if( nMaxErrorCount > m_nErrorCount ) {
					// record
					string szErrorCount = $"UPDATE {TableName.CoSysAuth} SET err_cnt={m_nErrorCount} WHERE login_id='{szUserId}'";
					m_mssql.TryQuery(szErrorCount, out szErrorMsg);
				}
				else {
					// lock account
					string szLockAccount = $"UPDATE {TableName.CoSysAuth} SET err_cnt={m_nErrorCount}, state='2' WHERE login_id='{szUserId}'";
					m_mssql.TryQuery(szLockAccount, out szErrorMsg);
					isLock = true;
				}
			}
		}
		string ParseErrorLog( AccountErrorCode errorcode )
		{
			switch( errorcode ) {
				case AccountErrorCode.success:
					return "00";
				case AccountErrorCode.noAccount:
					return "01";
				case AccountErrorCode.passwordError:
					return "02";
				case AccountErrorCode.passwordErrorManyTimes:
					return "03";
				case AccountErrorCode.accountDisabled:
					return "04";
				case AccountErrorCode.accountInActive:
					return "05";
				case AccountErrorCode.accountExpired:
					return "06";
				case AccountErrorCode.accountLock:
					return "07";
				case AccountErrorCode.passwordExpired:
					return "08";
				case AccountErrorCode.dbConnectionError:
					return "09";
				default:
					return "10";
			}
		}
		string GetReturnData( AccountErrorCode errorCode )
		{
			switch( errorCode ) {
				case AccountErrorCode.tobeExpired:
					return jReturnMsg[ "expiredDate" ].ToString();
				default:
					return errorCode.ToString();
			}
		}
		class Param
		{
			public static string Username
			{
				get
				{
					return "UserName";
				}
			}
			public static string Password
			{
				get
				{
					return "Password";
				}
			}
			public static string IP
			{
				get
				{
					return "IP";
				}
			}
		}
		enum AccountErrorCode
		{
			tobeExpired = 2,
			changePassword = 1,
			success = 0,
			noAccount = -1,
			passwordError = -2,
			passwordErrorManyTimes = -3,
			accountLock = -4,
			accountDisabled = -5,
			accountInActive = -6,
			accountExpired = -7,
			passwordExpired = -8,
			dbConnectionError = -9,
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

		MSSQL m_mssql = new MSSQL();
	}
}
