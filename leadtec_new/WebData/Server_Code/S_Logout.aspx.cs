using IncomeStatement.WebData.Server_Code.CommonModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IncomeStatement.WebData.Server_Code
{
	public partial class S_Logout : System.Web.UI.Page
	{
		RequestHandler m_requestHandler;
		protected void Page_Load( object sender, EventArgs e )
		{
			m_requestHandler = new RequestHandler();
			m_requestHandler.StatusCode = (int)ErrorCode.Success;

			// clear all cookies
			HttpCookieCollection Cookies = Request.Cookies;
			string[] szCookieArray = Cookies.AllKeys;
			DateTime ExpireTime = DateTime.Now.AddDays(-1d);
			foreach( string szCookieKey in szCookieArray ) {
				HttpCookie myCookie = new HttpCookie(szCookieKey);
				myCookie.Expires = ExpireTime;
				Response.Cookies.Add(myCookie);
			}

			Response.Write(m_requestHandler.GetReturnResult());
		}
	}
}
