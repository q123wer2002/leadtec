using Newtonsoft.Json.Linq;
using System.Web;

namespace IncomeStatement.WebData.Server_Code.CommonModule
{
	public class RequestHandler
	{
		#region Constructor
		public RequestHandler()
		{
		}

		public ErrorCode RequestValid( HttpRequest httpRequest )
		{
			ErrorCode eCode = ErrorCode.AuthenticationError;
			if( httpRequest.Cookies[ CookieJWTName ] == null ) {
				return eCode;
			}

			//get JWT Object 
			HttpCookie objCookie = httpRequest.Cookies[ CookieJWTName ];
			string szTokenString = objCookie.Value;

			//check token is valid or not
			ErrorCode jwtCode = JWTChecker.isJWTValid(szTokenString, out m_JWTToken);
			if( jwtCode != ErrorCode.Success ) {
				return jwtCode;
			}

			eCode = ErrorCode.Success;
			return eCode;
		}
		#endregion

		#region Public Method
		public dynamic GetReturnResult()
		{
			return m_objReturnReturn;
		}
		#endregion

		#region Public Attribute
		public int StatusCode
		{
			get
			{
				return m_objReturnReturn.status;
			}
			set
			{
				m_objReturnReturn.status = value;
			}
		}
		public dynamic ReturnData
		{
			get
			{
				return m_objReturnReturn.data;
			}
			set
			{
				m_objReturnReturn.data = value;
			}
		}
		#endregion

		#region Private Method
		#endregion

		#region Private Arrtibute
		const string CookieJWTName = "IncomeJWT";
		JWTModel m_JWTToken;
		dynamic m_objReturnReturn = new JObject();
		#endregion
	}
}
