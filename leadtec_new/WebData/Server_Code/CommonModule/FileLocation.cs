using System;

namespace IncomeStatement.WebData.Server_Code.CommonModule
{
	public class FileLocation
	{
		// public
		public static string ConfigPath
		{
			get
			{
				return AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"WebData\ConfigFile\";
			}
		}

		// private
		readonly static string m_szHostString = @"..\WebsiteData\{UserName}\{UserID}\";
		readonly static string szExecuteRoute = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + m_szHostString;
	}
}
