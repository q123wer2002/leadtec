using IncomeStatement.WebData.Server_Code.CommonModule;
using IncomeStatement.WebData.Server_Code.CommonModule.mssql;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IncomeStatement.WebData.Server_Code
{
	public partial class S_SubjectData : System.Web.UI.Page
	{
		RequestHandler m_requestHandler;
		string szUserCode;
		protected void Page_Load( object sender, EventArgs e )
		{
			// check status, set default
			m_requestHandler = new RequestHandler();
			m_requestHandler.StatusCode = (int)ErrorCode.Error;
			m_requestHandler.ReturnData = "Param Error";

			ErrorCode ec = m_requestHandler.RequestValid( Request );
			if( ec != ErrorCode.Success ) {
				m_requestHandler.StatusCode = (int)ec;
				m_requestHandler.ReturnData = ec.ToString();
				Response.Write( m_requestHandler.GetReturnResult() );
				return;
			}

			// get user id
			szUserCode = Request.Cookies[ CookieKey.UserID ].Value;

			// check action
			ApiAction action = GetAction();
			if( action == ApiAction.UNKNOW ) {
				Response.Write( m_requestHandler.GetReturnResult() );
				return;
			}

			// check param valid
			if( isParamValid( action ) == false ) {
				Response.Write( m_requestHandler.GetReturnResult() );
				return;
			}

			// do action
			dynamic result = DoAction( action );
			m_requestHandler.StatusCode = (int)ErrorCode.Success;
			m_requestHandler.ReturnData = result;
			Response.Write( m_requestHandler.GetReturnResult() );
		}
		protected void Page_Error( object sender, EventArgs e )
		{
			// get error
			Exception ex = Server.GetLastError();

			// return
			m_requestHandler.StatusCode = (int)ErrorCode.Error;
			m_requestHandler.ReturnData = ( ConnectionInfo.isDebugMode ) ? ex.ToString() : string.Empty;
			Response.Write( m_requestHandler.GetReturnResult() );
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

				if( szAction.ToUpper() == "UPDATE" ) {
					return ApiAction.UPDATE;
				}

				if( szAction.ToUpper() == "INSERT" ) {
					return ApiAction.INSERT;
				}

				if( szAction.ToUpper() == "DELETE" ) {
					return ApiAction.DELETE;
				}

				if( szAction.ToUpper() == "SETDEFAULTNAME" ) {
					return ApiAction.SETDEFAULTNAME;
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
					if( Request.Form[ Param.CodeNo ] != null ) {
						string szCodeNo = Request.Form[ Param.CodeNo ].ToString();
						int nCode = int.Parse( szCodeNo );
						if( nCode != -1 ) {
							m_paramList.Add( $"code_no LIKE '{nCode}%'" );
						}
						else {
							m_paramList.Add( $"code_no LIKE '%'" );
						}

					}

					if( Request.Form[ Param.CodeName ] != null ) {
						string szCodeName = Request.Form[ Param.CodeName ].ToString();
						m_paramList.Add( $"code_name LIKE '%{szCodeName}%'" );
					}

					if( m_paramList.Count == 0 ) {
						return false;
					}
					return true;
				}
				if( action == ApiAction.INSERT ) {
					string szSubject = Request.Form[ Param.Subject ].ToString();
					JObject.Parse( szSubject );
					return true;
				}
				if( action == ApiAction.UPDATE ) {
					string szSubject = Request.Form[ Param.PreSubject ].ToString();
					JObject.Parse( szSubject );
					szSubject = Request.Form[ Param.NewSubject ].ToString();
					JObject.Parse( szSubject );
					return true;
				}
				if( action == ApiAction.DELETE ) {
					string szSubjectAry = Request.Form[ Param.SubjectArray ].ToString();
					JArray.Parse( szSubjectAry );
					return true;
				}
				if( action == ApiAction.SETDEFAULTNAME ) {
					string szSubject = Request.Form[ Param.Subject ].ToString();
					JObject.Parse( szSubject );
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

			if( action == ApiAction.INSERT ) {
				return Insert();
			}

			if( action == ApiAction.UPDATE ) {
				return Update();
			}

			if( action == ApiAction.DELETE ) {
				return Delete();
			}

			if( action == ApiAction.SETDEFAULTNAME ) {
				return SetDefaultName();
			}

			return null;
		}
		JArray ReadData()
		{
			// get param
			string szSQL = $"SELECT * FROM {TableName.CoExpCode} WHERE";
			for( int i = 0; i < m_paramList.Count; i++ ) {
				szSQL += $" {m_paramList[ i ]}";
				szSQL += i == m_paramList.Count - 1 ? " " : " AND";
			}
			JArray result;
			bool isSuccess = m_mssql.TryQuery( szSQL, out result );
			return result;
		}
		bool Insert()
		{
			// get subject object
			JObject jSubject = JObject.Parse( Request.Form[ Param.Subject ].ToString() );

			int nUpLim, nLowLim, nUppSys, nLowSys;
			if( int.TryParse( jSubject[ "upp_lim" ].ToString(), out nUpLim ) == false ) {
				nUpLim = -1;
			}
			if( int.TryParse( jSubject[ "low_lim" ].ToString(), out nLowLim ) == false ) {
				nLowLim = -1;
			}
			if( int.TryParse( jSubject[ "upp_sys" ].ToString(), out nUppSys ) == false ) {
				nUppSys = -1;
			}
			if( int.TryParse( jSubject[ "low_sys" ].ToString(), out nLowSys ) == false ) {
				nLowSys = -1;
			}

			// insert or update
			string szInsertOrUpdate = $"INSERT INTO {TableName.CoExpCode} VALUES ({jSubject[ "code_no" ]}, N'{jSubject[ "code_name" ]}', N'{jSubject[ "code_desc" ]}', {jSubject[ "code1" ]}, {parse2TwoDigital( jSubject[ "code2" ].ToString() )}, N'{jSubject[ "code_rem" ]}', {( nUpLim >= 0 ? nUpLim.ToString() : "NULL" )}, {( nLowLim >= 0 ? nLowLim.ToString() : "NULL" )}, '{jSubject[ "place" ]}', N'{jSubject[ "param1" ]}', N'{jSubject[ "param2" ]}', '{jSubject[ "stop_fg" ]}', CURRENT_TIMESTAMP, '{szUserCode}', {( nUppSys >= 0 ? nUppSys.ToString() : "NULL" )}, {( nLowSys >= 0 ? nLowSys.ToString() : "NULL" )}, N'{jSubject[ "def_fg" ]}')";
			string szErrorMsg;
			return m_mssql.TryQuery( szInsertOrUpdate, out szErrorMsg );
		}
		bool Delete()
		{
			List<JObject> jSubjectList = JsonConvert.DeserializeObject<List<JObject>>( Request.Form[ Param.SubjectArray ].ToString() );
			string szWhere = string.Join( "OR ", jSubjectList.Select( obj => $"(code_no='{obj[ "code_no" ]}' AND code_name='{obj[ "code_name" ]}')" ) );

			string szDelete = $"DELETE FROM {TableName.CoExpCode} WHERE {szWhere}";
			string szErrorMsg;
			return m_mssql.TryQuery( szDelete, out szErrorMsg );
		}
		bool Update()
		{
			// get subject object
			JObject jPreSubject = JObject.Parse( Request.Form[ Param.PreSubject ].ToString() );
			JObject jNewSubject = JObject.Parse( Request.Form[ Param.NewSubject ].ToString() );

			int nUpLim, nLowLim;
			if( int.TryParse( jNewSubject[ "upp_lim" ].ToString(), out nUpLim ) == false ) {
				nUpLim = -1;
			}
			if( int.TryParse( jNewSubject[ "low_lim" ].ToString(), out nLowLim ) == false ) {
				nLowLim = -1;
			}

			// update
			string szUpdate = $"UPDATE {TableName.CoExpCode} SET code_no={jNewSubject[ "code_no" ]}, code_name='{jNewSubject[ "code_name" ]}', code_desc=N'{jNewSubject[ "code_desc" ]}', code_rem=N'{jNewSubject[ "code_rem" ]}', upp_lim={( nUpLim >= 0 ? nUpLim.ToString() : "NULL" )}, low_lim={( nLowLim >= 0 ? nLowLim.ToString() : "NULL" )}, place='{jNewSubject[ "place" ]}', param1=N'{jNewSubject[ "param1" ]}', param2=N'{jNewSubject[ "param2" ]}', stop_fg='{jNewSubject[ "stop_fg" ]}', upd_date=CURRENT_TIMESTAMP, upd_user='{szUserCode}' WHERE code_no={jPreSubject[ "code_no" ]} AND code_name='{jPreSubject[ "code_name" ]}'";
			string szErrorMsg;
			return m_mssql.TryQuery( szUpdate, out szErrorMsg );
		}
		bool SetDefaultName()
		{
			// get subject object
			JObject jSubject = JObject.Parse( Request.Form[ Param.Subject ].ToString() );
			string szCode = jSubject[ "code_no" ].ToString();
			string szCodeName = jSubject[ "code_name" ].ToString();
			string szErrorMsg;

			// set all code are empty
			string szEmptySQL = $"UPDATE {TableName.CoExpCode} SET def_fg='' WHERE code_no='{szCode}'";
			if( m_mssql.TryQuery( szEmptySQL, out szErrorMsg ) == false ) {
				return false;
			}

			// set the subject is the default name
			string szSetDefaultName = $"UPDATE {TableName.CoExpCode} SET def_fg = 'Y' WHERE code_no='{szCode}' AND code_name='{szCodeName}'";
			return m_mssql.TryQuery( szSetDefaultName, out szErrorMsg );
		}
		public string parse2TwoDigital( string szNumber )
		{
			int nTemp;
			if( int.TryParse( szNumber, out nTemp ) == false ) {
				return string.Empty;
			}

			if( nTemp < 10 ) {
				return $"0{nTemp}";
			}

			return nTemp.ToString();
		}
		#endregion

		#region Private Attribute
		enum ApiAction
		{
			READ,
			UPDATE,
			INSERT,
			DELETE,
			SETDEFAULTNAME,
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
			public static string CodeNo
			{
				get
				{
					return "CodeNo";
				}
			}
			public static string CodeName
			{
				get
				{
					return "CodeName";
				}
			}

			// for update, insert
			public static string Subject
			{
				get
				{
					return "Subject";
				}
			}
			public static string PreSubject
			{
				get
				{
					return "PreSubject";
				}
			}
			public static string NewSubject
			{
				get
				{
					return "NewSubject";
				}
			}

			// for delete
			public static string SubjectArray
			{
				get
				{
					return "SubjectArray";
				}
			}
		}
		MSSQL m_mssql = new MSSQL();
		List<string> m_paramList = new List<string>();
		#endregion
	}
}
