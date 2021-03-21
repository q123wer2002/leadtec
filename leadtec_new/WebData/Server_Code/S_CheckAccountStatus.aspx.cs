using IncomeStatement.WebData.Server_Code.CommonModule;
using System;

namespace IncomeStatement.WebData.Server_Code
{
	public partial class S_CheckAccountStatus : System.Web.UI.Page
	{
		RequestHandler m_requestHandler;
		protected void Page_Load( object sender, EventArgs e )
		{
			m_requestHandler = new RequestHandler();
			ErrorCode accountEC = m_requestHandler.RequestValid(Request);

			m_requestHandler.StatusCode = (int)accountEC;
			m_requestHandler.ReturnData = accountEC.ToString();

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
	}
}