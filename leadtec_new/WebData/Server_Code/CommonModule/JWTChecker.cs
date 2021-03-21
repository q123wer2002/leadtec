using Jose;
using System;
using System.Text;
using System.Threading.Tasks;

namespace IncomeStatement.WebData.Server_Code.CommonModule
{
	public class JWTChecker
	{
		#region Public Method
		public static ErrorCode isJWTValid( string szJWTToken, out JWTModel jwtObject )
		{
			ErrorCode eCode = ErrorCode.AuthenticationError;

			//parse
			if( TryParseJWTToken( szJWTToken, out jwtObject ) == false ) {
				//parse error, means token error
				return eCode;
			}

			//check server ip
			if( isServerIPVaild( jwtObject ) == false ) {
				return eCode;
			}

			//check token is expire or not
			if( isTokenExpire(jwtObject) ) {
				return eCode;
			}

			//valid autho
			if( isAuthAccess(jwtObject) == false ) {
				//means no autho
				eCode = ErrorCode.NoAuthorization;
				return eCode;
			}

			//success
			eCode = ErrorCode.Success;
			return eCode;
		}
		public static string CreateNewJWTObjectString( string szUserName )
		{
			JWTModel NewToken = CreateNewJWTObject( szUserName );
			return TokenObjecttoString( NewToken );
		}
		#endregion

		#region Private Method
		// parse
		static bool TryParseJWTToken( string szJWTToken, out JWTModel jwtObject )
		{
			//default
			jwtObject = null;

			//parse
			try {
				jwtObject = Jose.JWT.Decode<JWTModel>(
											szJWTToken,
											Encoding.UTF8.GetBytes( m_szJWTSecret ),
											JwsAlgorithm.HS256 );
				return true;
			}
			catch {
				return false;
			}
		}
		static string TokenObjecttoString( JWTModel objJWTToken )
		{
			string szJWT = Jose.JWT.Encode( objJWTToken, Encoding.UTF8.GetBytes( m_szJWTSecret ), JwsAlgorithm.HS256 );
			return szJWT;
		}

		// valid
		static bool isServerIPVaild( JWTModel obJWTToken )
		{
			if( obJWTToken.iss == System.Web.HttpContext.Current.Request.ServerVariables[ "LOCAL_ADDR" ] ) {
				return true;
			}
			return false;
		}
		static bool isTokenExpire( JWTModel obJWTToken )
		{
			double dCurrentTimestamp = ( DateTime.UtcNow.Subtract( new DateTime( 1970, 1, 1 ) ) ).TotalMilliseconds;
			if( obJWTToken.exp > dCurrentTimestamp ) {
				return false;
			}
			return true;
		}
		static bool isAuthAccess( JWTModel obJWTToken )
		{
			if( Authorization.isAccessAutho( obJWTToken.authorization ) ) {
				return true;
			}

			return false;
		}

		// create
		static JWTModel CreateNewJWTObject( string szUserName )
		{
			JWTModel objNewJWT = new JWTModel();
			double dCurrentTime = ( DateTime.UtcNow.Subtract( new DateTime( 1970, 1, 1 ) ) ).TotalMilliseconds;

			//assign
			objNewJWT.iss = System.Web.HttpContext.Current.Request.ServerVariables[ "LOCAL_ADDR" ];
			objNewJWT.sub = "VaildUser";
			objNewJWT.exp = dCurrentTime + m_nExpireTime;
			objNewJWT.iat = dCurrentTime;
			objNewJWT.userName = szUserName;

			// TODO
			objNewJWT.authorization = null;
			return objNewJWT;
		}

		#endregion

		#region Private Attribute
		const int m_nExpireTime = 86400000;
		const string m_szJWTSecret = "MyJWTSecretCode_Danny";//JWT Code
		#endregion
	}
}
