using IncomeStatement.WebData.Server_Code.CommonModule;
using IncomeStatement.WebData.Server_Code.CommonModule.mssql;
using Newtonsoft.Json.Linq;
using System;

namespace IncomeStatement.WebData.Server_Code
{
	public partial class S_ParamInfo : System.Web.UI.Page
	{
		RequestHandler m_requestHandler;
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

			// do action
			
			m_requestHandler.StatusCode = (int)ErrorCode.Success;
			m_requestHandler.ReturnData = GetParamList();
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

		JObject GetParamList()
		{
			JObject jParam = new JObject();

			// get param 
			string szSQL = $"SELECT * FROM {TableName.CoParam}";
			JArray result;
			bool isSuccess = m_mssql.TryQuery(szSQL, out result);
			jParam[ "param" ] = result;

			// get code attr
			isSuccess = m_mssql.TryQuery( $"SELECT * FROM {TableName.CoExpCodeAttr}", out result );
			jParam[ "code_attr" ] = result;

			return jParam;
		}

		MSSQL m_mssql = new MSSQL();
	}
}
