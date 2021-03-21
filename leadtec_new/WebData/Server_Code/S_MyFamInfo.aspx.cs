using IncomeStatement.WebData.Server_Code.CommonModule;
using IncomeStatement.WebData.Server_Code.CommonModule.mssql;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace IncomeStatement.WebData.Server_Code
{
	public partial class S_MyFamInfo : System.Web.UI.Page
	{
		RequestHandler m_requestHandler;
		string m_szRole;
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

			m_szRole = Request.Cookies[ CookieKey.UserRole ].Value;

			m_requestHandler.StatusCode = (int)ErrorCode.Success;
			m_requestHandler.ReturnData = GetMyFamInfo();
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

		dynamic GetMyFamInfo( string szMyRecNo = null )
		{
			if( new List<string>() { "A", "B" }.Contains(m_szRole) == false ) {
				return null;
			}

			string szSQL = $"SELECT * FROM {TableName.CoRecFam} WHERE {TableName.CoRecFam}.fam_no IN (SELECT fam_no FROM co_rec_fam ";
			if( m_szRole.ToUpper() == "A" ) {
				szSQL += ")";
			}
			else {
				szSQL += $"WHERE rec_user = '{Request.Cookies[ CookieKey.UserID ].Value}'";
			}

			if( szMyRecNo != null ) {
				szSQL += $"AND rec_user='{szMyRecNo}'";
			}

			JArray result;
			m_mssql.TryQuery(szSQL, out result);
			return result;
		}

		MSSQL m_mssql = new MSSQL();
	}
}
