using IncomeStatement.WebData.Server_Code.CommonModule;
using IncomeStatement.WebData.Server_Code.CommonModule.mssql;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace IncomeStatement.WebData.Server_Code
{
	public partial class S_ChangePassword : System.Web.UI.Page
	{
		RequestHandler m_requestHandler;
		string m_szUserId, m_szUserName;
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

			// get input
			string szOriginalPwd = Request.Form[ "OPwd" ].ToString();
			string szNewPwd = Request.Form[ "NPwd" ].ToString();

			// get user info from cookie
			m_szUserId = Request.Cookies[ CookieKey.UserID ].Value;
			m_szUserName = Request.Cookies[ CookieKey.Username ].Value;

			// select data once
			JArray jResult;
			m_mssql.TryQuery($"SELECT * FROM {TableName.CoSysAuth} WHERE user_id='{m_szUserId}'", out jResult);
			if( jResult.Count != 1 ) {
				m_requestHandler.StatusCode = (int)ErrorCode.Error;
				m_requestHandler.ReturnData = "沒有使用者的資料";
				Response.Write(m_requestHandler.GetReturnResult());
				return;
			}

			// check password
			JObject jData = (JObject)jResult[ 0 ];
			if( szOriginalPwd != jData[ "pwd" ].ToString() ) {
				m_requestHandler.StatusCode = (int)ErrorCode.Error;
				m_requestHandler.ReturnData = "原密碼輸入錯誤";
				Response.Write(m_requestHandler.GetReturnResult());
				return;
			}

			if( jData[ "pwd" ].ToString() == szNewPwd ) {
				m_requestHandler.StatusCode = (int)ErrorCode.Error;
				m_requestHandler.ReturnData = "與原密碼重覆";
				Response.Write(m_requestHandler.GetReturnResult());
				return;
			}

			// check new password is same as last three times
			if( (new List<string>() { jData[ "pwd1" ].ToString(), jData[ "pwd2" ].ToString(), jData[ "pwd3" ].ToString() }).Contains(szNewPwd) ) {
				m_requestHandler.StatusCode = (int)ErrorCode.Error;
				m_requestHandler.ReturnData = "與過去的三次密碼重覆";
				Response.Write(m_requestHandler.GetReturnResult());
				return;
			}

			// set password
			JArray jExtendDays;
			int nExtendDays = 30;
			if(
				m_mssql.TryQuery($"SELECT * FROM {TableName.CoParam} WHERE par_typ='PWD' AND par_no='P002'", out jExtendDays) &&
				jExtendDays.Count > 0
			) {
				nExtendDays = int.Parse(jExtendDays[ 0 ][ "par_val" ].ToString());
			}

			string szErrorMsg;
			string pwd2 = jData[ "pwd1" ].ToString().Length == 0 ? "NULL" : $"'{jData[ "pwd1" ].ToString()}'";
			string pwd3 = jData[ "pwd2" ].ToString().Length == 0 ? "NULL" : $"'{jData[ "pwd2" ].ToString()}'";
			bool isSuccess = m_mssql.TryQuery($"UPDATE {TableName.CoSysAuth} SET pwd='{szNewPwd}', " +
				$"pwd1='{szOriginalPwd}', " +
				$"pwd2={pwd2}, " +
				$"pwd3={pwd3}, " +
				$"exp_date='{DateTime.Now.AddDays(nExtendDays).ToString("yyyy-MM-dd")}', " +
				$"chg_pwd=NULL, " + 
				$"upd_date=CURRENT_TIMESTAMP, upd_user='{m_szUserId}' " +
				$"WHERE user_id='{m_szUserId}'", out szErrorMsg);

			if( isSuccess == false ) {
				m_requestHandler.StatusCode = (int)ErrorCode.Error;
				m_requestHandler.ReturnData = "系統發生錯誤";
				Response.Write(m_requestHandler.GetReturnResult());
				return;
			}

			m_requestHandler.StatusCode = (int)ErrorCode.Success;
			m_requestHandler.ReturnData = "success";
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

		MSSQL m_mssql = new MSSQL();
	}
}
