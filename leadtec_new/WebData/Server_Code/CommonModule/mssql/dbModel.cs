namespace IncomeStatement.WebData.Server_Code.CommonModule.mssql
{
	public class ColumnModel
	{
		public string ColName
		{
			get;set;
		}
		public string ColType
		{
			get;set;
		}
		public bool IsPrimaryKey
		{
			get;set;
		}
		public bool IsNull
		{
			get;set;
		}
	}
	public class TableName
	{
		public static string CoFam
		{
			get
			{
				return "co_fam";
			}
		}
		public static string CoFamLog
		{
			get
			{
				return "co_fam_log";
			}
		}
		public static string CoExpM
		{
			get
			{
				return "co_exp_m";
			}
		}
		public static string CoExpMLog
		{
			get
			{
				return "co_exp_m_log";
			}
		}
		public static string CoExpCode
		{
			get
			{
				return "co_exp_code";
			}
		}
		public static string CoParam
		{
			get
			{
				return "co_param";
			}
		}
		public static string CoExpCodeAttr
		{
			get
			{
				return "co_exp_code_attr";
			}
		}
		public static string CoExpD
		{
			get
			{
				return "co_exp_d";
			}
		}
		public static string CoExpDLog
		{
			get
			{
				return "co_exp_d_log";
			}
		}
		public static string CoRecFam
		{
			get
			{
				return "co_rec_fam";
			}
		}
		public static string CoFamMem
		{
			get
			{
				return "co_fam_mem";
			}
		}
		public static string CoFamMemLog
		{
			get
			{
				return "co_fam_mem_log";
			}
		}

		// system
		public static string CoSysAuth
		{
			get
			{
				return "co_sys_auth";
			}
		}
		public static string CoSysUser
		{
			get
			{
				return "co_sys_user";
			}
		}
		public static string CoSysLog
		{
			get
			{
				return "co_sys_log";
			}
		}
		public static string CoExpAudit
		{
			get
			{
				return "co_exp_audit";
			}
		}
	}
}