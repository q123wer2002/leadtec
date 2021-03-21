namespace IncomeStatement.WebData.Server_Code.CommonModule
{
	public class JWTModel
	{
		//who assign this Token (ServerIP)
		public string iss
		{
			get;
			set;
		}
		
		//what kind of people will accept this token, sub="VaildUser", exception : SynFactory (sub = SynFactory)   
		public string sub
		{
			get;
			set;
		}

		// token expire time ( one days)
		public double exp
		{
			get;
			set;
		}
		
		//token create time (current time )
		public double iat
		{
			get;
			set;
		}
		
		//user Account 
		public string userName
		{
			get;
			set;
		}
		
		//user authorization
		public string authorization
		{
			get; set;
		}
	}
}