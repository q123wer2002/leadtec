using IncomeStatement.WebData.Server_Code.CommonModule;
using IncomeStatement.WebData.Server_Code.CommonModule.mssql;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace IncomeStatement.WebData.Server_Code
{
	public partial class S_Tableau : System.Web.UI.Page
	{
		RequestHandler m_requestHandler;
		JObject m_Config = ConfigReader.GetConfigFile(ConfigReader.Config.Report);
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
			string szAction = Request.Form[ Param.Action ].ToString().ToUpper();
			switch( szAction ) {
				case "GETREPORTS":
					return ApiAction.GetReports;
				case "GETTICKET":
					return ApiAction.GetTicket;
				default:
					return ApiAction.UNKNOW;
			}
		}
		bool isParamValid( ApiAction action )
		{
			switch( action ) {
				case ApiAction.GetReports:
					return true;
				case ApiAction.GetTicket:
					if( Request.Form[ Param.SubUrl ] == null ) {
						return false;
					}
					return true;
				default:
					return false;
			}
		}
		string GetTicket()
		{
			string szURL = $"{m_Config[ "ReportUrl" ].ToString()}/trusted";
			StringBuilder data = new StringBuilder();
			data.Append($"username={HttpUtility.UrlEncode(m_Config[ "username" ].ToString(), Encoding.UTF8)}");
			data.Append($"&server={HttpUtility.UrlEncode(m_Config[ "ReportUrl" ].ToString(), Encoding.UTF8)}");
			data.Append($"&client_ip=''");
			data.Append($"&target_site={HttpUtility.UrlEncode(m_Config[ "target_site" ].ToString(), Encoding.UTF8)}");

			WebRequest request = WebRequest.Create(szURL);
			request.Method = "POST";
			byte[] byteArray = Encoding.UTF8.GetBytes(data.ToString());
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = byteArray.Length;
			using( Stream dataStream = request.GetRequestStream() ) {
				dataStream.Write(byteArray, 0, byteArray.Length);
			}

			try {
				WebResponse webResponse = request.GetResponse();
				using( StreamReader responseReader = new StreamReader(webResponse.GetResponseStream()) ) {
					return responseReader.ReadToEnd();
				}
			}
			catch {
				return string.Empty;
			}
		}
		dynamic DoAction( ApiAction action )
		{
			switch( action ) {
				case ApiAction.GetReports:
					JArray jResult;
					m_mssql.TryQuery($"SELECT * FROM {TableName.CoParam} WHERE par_typ='{m_Config[ "TableauKey" ]}'", out jResult);
					return jResult;
				case ApiAction.GetTicket:
					return $"{m_Config[ "ReportUrl" ]}/trusted/{GetTicket()}/t/{Request.Form[ Param.SubUrl ].ToString()}";
				default:
					return null;
			}
		}

		enum ApiAction
		{
			GetReports,
			GetTicket,
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
			public static string SubUrl
			{
				get
				{
					return "SubUrl";
				}
			}
		}

		MSSQL m_mssql = new MSSQL();
	}
}
