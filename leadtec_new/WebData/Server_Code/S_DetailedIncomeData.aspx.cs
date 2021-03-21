using IncomeStatement.WebData.Server_Code.CommonModule;
using IncomeStatement.WebData.Server_Code.CommonModule.mssql;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IncomeStatement.WebData.Server_Code
{
	public partial class S_DetailedIncomeData : System.Web.UI.Page
	{
		RequestHandler m_requestHandler;
		string szUserCode;
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

			// get user id
			szUserCode = Request.Cookies[ CookieKey.UserID ].Value;

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
			try {
				string szAction = Request.Form[ Param.Action ].ToString();
				if( szAction.ToUpper() == "READ" ) {
					return ApiAction.READ;
				}

				if( szAction.ToUpper() == "WRITE" ) {
					return ApiAction.WRITE;
				}

				if( szAction.ToUpper() == "DELETE" ) {
					return ApiAction.DELETE;
				}

				if( szAction.ToUpper() == "HUGEINSERT" ) {
					return ApiAction.HUGEINSERT;
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
				int nTempValue;

				// check fam no
				if( Request.Form[ Param.FamNo ] == null ) {
					return false;
				}

				string szFamNo = Request.Form[ Param.FamNo ].ToString();
				int.Parse(szFamNo);

				m_paramExpDList.Add($"{TableName.CoExpD}.fam_no={szFamNo}");
				m_paramExpMList.Add($"{TableName.CoExpM}.fam_no={szFamNo}");

				if( action == ApiAction.READ ) {
					// check date
					if( Request.Form[ Param.Year ] != null && Request.Form[ Param.Month ] != null ) {
						string szYear = Request.Form[ Param.Year ].ToString();
						string szMonth = Request.Form[ Param.Month ].ToString();

						if(
							int.TryParse(szYear, out nTempValue) == false ||
							int.TryParse(szMonth, out nTempValue) == false
						) {
							return false;
						}

						m_paramExpDList.Add($"{TableName.CoExpD}.ie_year={szYear} AND {TableName.CoExpD}.ie_mon={szMonth}");
						m_paramExpMList.Add($"{TableName.CoExpM}.ie_year={szYear} AND {TableName.CoExpM}.ie_mon={szMonth}");
					}

					if( Request.Form[ Param.Day ] != null ) {
						string szDay = Request.Form[ Param.Day ].ToString();
						if( int.TryParse(szDay, out nTempValue) == false ) {
							return false;
						}

						m_paramExpDList.Add($"{TableName.CoExpD}.ie_day={szDay}");
						m_paramExpMList.Add($"{TableName.CoExpM}.ie_day={szDay}");
					}

					// check duration
					if( Request.Form[ Param.DurationStart ] != null && Request.Form[ Param.DurationEnd ] != null ) {
						string szDurationStart = Request.Form[ Param.DurationStart ].ToString();
						string szDurationEnd = Request.Form[ Param.DurationEnd ].ToString();

						if(
							int.TryParse(szDurationStart, out nTempValue) == false ||
							int.TryParse(szDurationEnd, out nTempValue) == false
						) {
							return false;
						}

						m_paramExpDList.Add($"{TableName.CoExpD}.ie_day BETWEEN {szDurationStart} AND {szDurationEnd}");
						m_paramExpMList.Add($"{TableName.CoExpM}.ie_day BETWEEN {szDurationStart} AND {szDurationEnd}");
					}

					// check no
					if( Request.Form[ Param.CodeNo ] != null ) {
						string szCodeNo = Request.Form[ Param.CodeNo ].ToString();
						int nCode = int.Parse(szCodeNo);
						m_paramExpDList.Add($"{TableName.CoExpD}.code_no LIKE '{nCode}%'");
					}

					// check no
					if( Request.Form[ Param.CodeName ] != null ) {
						string szCodeName = Request.Form[ Param.CodeName ].ToString();
						m_paramExpDList.Add($"{TableName.CoExpD}.code_name LIKE '%{szCodeName}%'");
					}

					if( m_paramExpDList.Count == 0 ) {
						return false;
					}
					return true;
				}
				if( action == ApiAction.WRITE ) {
					// get item array
					string szUpdateItems = Request.Form[ Param.UpdateItems ].ToString();
					JArray.Parse(szUpdateItems);
					string szInsertItems = Request.Form[ Param.InsertItems ].ToString();
					JArray.Parse(szInsertItems);

					// get date
					string szYear = Request.Form[ Param.Year ].ToString();
					int.Parse(szYear);
					string szMonth = Request.Form[ Param.Month ].ToString();
					int.Parse(szMonth);
					string szDay = Request.Form[ Param.Day ].ToString();
					int.Parse(szDay);

					m_paramExpDList.Add($"{TableName.CoExpD}.ie_year={szYear} AND {TableName.CoExpD}.ie_mon={szMonth} AND {TableName.CoExpD}.ie_day={szDay}");
					m_paramExpMList.Add($"{TableName.CoExpM}.ie_year={szYear} AND {TableName.CoExpM}.ie_mon={szMonth} AND {TableName.CoExpM}.ie_day={szDay}");

					// check total cosst
					string szTotalCost = Request.Form[ Param.TotalCost ].ToString();
					int.Parse(szTotalCost);

					// check remark
					string szRemark = Request.Form[ Param.DayRemark ].ToString();
					return true;
				}
				if( action == ApiAction.DELETE ) {
					string szItemArray = Request.Form[ Param.ItemArray ].ToString();
					JArray.Parse(szItemArray);
					return true;
				}

				if( action == ApiAction.HUGEINSERT ) {
					string szInsertItems = Request.Form[ Param.InsertItems ].ToString();
					JArray.Parse(szInsertItems);

					// get date
					string szYear = Request.Form[ Param.Year ].ToString();
					int.Parse(szYear);
					string szMonth = Request.Form[ Param.Month ].ToString();
					int.Parse(szMonth);
					string szDay = Request.Form[ Param.Day ].ToString();
					int.Parse(szDay);

					m_paramExpDList.Add($"{TableName.CoExpD}.ie_year={szYear} AND {TableName.CoExpD}.ie_mon={szMonth} AND {TableName.CoExpD}.ie_day={szDay}");
					m_paramExpMList.Add($"{TableName.CoExpM}.ie_year={szYear} AND {TableName.CoExpM}.ie_mon={szMonth} AND {TableName.CoExpM}.ie_day={szDay}");
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

			if( action == ApiAction.WRITE ) {
				return Write();
			}

			if( action == ApiAction.DELETE ) {
				return Delete();
			}

			if( action == ApiAction.HUGEINSERT ) {
				return HugeInsert();
			}

			return null;
		}
		JObject ReadData()
		{
			JObject jResult = new JObject();

			// get co_exp_d
			string szSQL = $"SELECT {TableName.CoExpD}.*, {TableName.CoExpM}.exp_amt FROM {TableName.CoExpD} " +
				$"LEFT JOIN {TableName.CoExpM} ON {TableName.CoExpD}.ie_year={TableName.CoExpM}.ie_year AND {TableName.CoExpD}.ie_mon={TableName.CoExpM}.ie_mon AND {TableName.CoExpD}.ie_day={TableName.CoExpM}.ie_day AND {TableName.CoExpD}.fam_no={TableName.CoExpM}.fam_no " +
				$"WHERE ";
			for( int i = 0; i < m_paramExpDList.Count; i++ ) {
				szSQL += $" {m_paramExpDList[ i ]}";
				szSQL += i == m_paramExpDList.Count - 1 ? " " : " AND";
			}

			// set auth
			if( Request.Cookies[ CookieKey.UserRole ].Value == "B" ) {
				szSQL += $" AND {TableName.CoExpD}.fam_no IN (SELECT fam_no FROM co_rec_fam WHERE rec_user='{szUserCode}')";
			}

			JArray result;
			bool isSuccess = m_mssql.TryQuery(szSQL, out result);
			if( isSuccess ) {
				jResult[ "CoExpD" ] = result;
			}

			// get co_exp_m
			szSQL = $"SELECT * FROM {TableName.CoExpM} WHERE ";
			for( int i = 0; i < m_paramExpMList.Count; i++ ) {
				szSQL += $" {m_paramExpMList[ i ]}";
				szSQL += i == m_paramExpMList.Count - 1 ? " " : " AND";
			}
			isSuccess = m_mssql.TryQuery(szSQL, out result);
			if( isSuccess ) {
				jResult[ "CoExpM" ] = result;
			}

			return jResult;
		}
		bool Write()
		{
			#region FOR EXPD
			// for update
			List<JObject> updateItems = JsonConvert.DeserializeObject<List<JObject>>(Request.Form[ Param.UpdateItems ].ToString());
			if( updateItems.Count > 0 ) {
				DeleteItems( "M", updateItems.Select( obj => int.Parse( obj[ "item_no" ].ToString() ) ).ToList() );
				InsertIntems( true, updateItems );
			}

			// for insert
			List<JObject> insertItems = JsonConvert.DeserializeObject<List<JObject>>(Request.Form[ Param.InsertItems ].ToString());
			if( insertItems.Count > 0 ) {
				InsertIntems(false, insertItems);
			}
			#endregion
			#region FOR EXPM
			// update cost
			string szErrorMsg;
			int nTotalCosst = int.Parse(Request.Form[ Param.TotalCost ].ToString());
			string szRemark = Request.Form[ Param.DayRemark ].ToString();
			string szWhere = "";
			for( int i = 0; i < m_paramExpMList.Count; i++ ) {
				szWhere += $" {m_paramExpMList[ i ]}";
				szWhere += i == m_paramExpMList.Count - 1 ? " " : " AND";
			}

			// select once
			string szGetExpM = $"SELECT * FROM {TableName.CoExpM} WHERE {szWhere}";
			JArray jResult;
			m_mssql.TryQuery( szGetExpM, out jResult );

			// update or insert
			if( jResult.Count == 0 ) {
				string szYear = Request.Form[ Param.Year ].ToString();
				string szMonth = Request.Form[ Param.Month ].ToString();
				string szDay = Request.Form[ Param.Day ].ToString();
				string szFamNo = Request.Form[ Param.FamNo ].ToString();
				string szInsertql = $"INSERT INTO {TableName.CoExpM} VALUES ('{szYear}', '{parse2TwoDigital( szMonth )}', '{parse2TwoDigital( szDay )}', '{szFamNo}', '{nTotalCosst}', '{szRemark}', '1', CURRENT_TIMESTAMP, '{szUserCode}', NULL, NUll, NULL, NULL, NULL, NULL)";
				return m_mssql.TryQuery( szInsertql, out szErrorMsg );
			}
			else {
				string szInsertLogSql = $"INSERT INTO {TableName.CoExpMLog} SELECT 'M', CURRENT_TIMESTAMP, '{szUserCode}', * FROM {TableName.CoExpM} WHERE {szWhere}";
				string szUpdate = $"UPDATE {TableName.CoExpM} SET exp_amt='{nTotalCosst}', day_rem='{szRemark}', upd_date=CURRENT_TIMESTAMP, upd_user='{szUserCode}' WHERE {szWhere}";
				return m_mssql.TryQuery( szInsertLogSql, out szErrorMsg ) && m_mssql.TryQuery( szUpdate, out szErrorMsg );
			}
			#endregion
		}
		bool Delete()
		{
			List<int> itemNoList = JsonConvert.DeserializeObject<List<int>>(Request.Form[ Param.ItemArray ].ToString());
			return DeleteItems("D", itemNoList);
		}
		bool DeleteItems( string szReason, List<int> itemNos )
		{
			string szWhere = $"item_no IN({string.Join(", ", itemNos.Select(itemNo => $"'{itemNo}'"))}) AND ";
			string szErrorMsg;
			for( int i = 0; i < m_paramExpDList.Count; i++ ) {
				szWhere += $" {m_paramExpDList[ i ]}";
				szWhere += i == m_paramExpDList.Count - 1 ? " " : " AND";
			}

			// copy currnt data into log file
			string szInsertLog = $"INSERT INTO {TableName.CoExpDLog} SELECT '{szReason}', CURRENT_TIMESTAMP, '{szUserCode}', * FROM {TableName.CoExpD} WHERE {szWhere}";
			bool isSuccess = m_mssql.TryQuery(szInsertLog, out szErrorMsg);
			if( isSuccess == false ) {
				return false;
			}

			// delete items
			string szDelete = $"DELETE FROM {TableName.CoExpD} WHERE {szWhere}";
			isSuccess = m_mssql.TryQuery(szDelete, out szErrorMsg);
			return isSuccess;
		}
		bool InsertIntems( bool isOwnItemNo, List<JObject> items )
		{
			int nNextItemNo = 0;
			if(isOwnItemNo == false) {
				// get the newest item_no
				JObject jItem = items[ 0 ];
				string szGetItemNo = $"SELECT TOP(1) item_no FROM {TableName.CoExpD} WHERE ie_year={jItem[ "ie_year" ].ToString()} AND ie_mon={jItem[ "ie_mon" ].ToString()} ORDER BY item_no DESC";
				JArray result;
				m_mssql.TryQuery( szGetItemNo, out result );
				if(result.Count == 0) {
					nNextItemNo = 0;
				}
				else {
					nNextItemNo = int.Parse( ( (JObject)result[ 0 ] )[ "item_no" ].ToString() );
				}
			}

			string szFamNo = Request.Form[ Param.FamNo ].ToString();
			string szErrorMsg;
			string szInsert = $"INSERT INTO {TableName.CoExpD} (ie_year,ie_mon,ie_day,fam_no,item_no,place,code_amt,code_no,code_name,crt_date,crt_user,upd_date,upd_user,unit,qty) VALUES ";
			for(int i = 0; i < items.Count; i++) {
				JObject jItem = items[ i ];
				int nItemNo = isOwnItemNo == false ? nNextItemNo + i + 1 : int.Parse( jItem[ "item_no" ].ToString() );
				string szCodeName = jItem[ "code_name" ] == null ? "NULL" : $"'{jItem[ "code_name" ].ToString()}'";
				string szRecDate = jItem[ "crt_date" ] == null ? "NULL" : $"'{jItem[ "crt_date" ]}'";
				string szRecUser = jItem[ "crt_user" ] == null ? "NULL" : $"'{jItem[ "crt_user" ].ToString()}'";
				string szUnit = jItem[ "unit" ] == null || jItem[ "unit" ].ToString().Length == 0 ? "NULL" : $"N'{jItem[ "unit" ].ToString()}'";
				string szQty = jItem[ "qty" ] == null || jItem[ "qty" ].ToString().Length == 0 ? "NULL" : $"'{jItem[ "qty" ].ToString()}'";

				string szLastFourSQL = isOwnItemNo ? $"{szRecDate}, {szRecUser}, CURRENT_TIMESTAMP, '{szUserCode}'" : $"CURRENT_TIMESTAMP, '{szUserCode}', NULL, NULL";
				szInsert += $"('{parse2TwoDigital( jItem[ "ie_year" ].ToString() )}', '{parse2TwoDigital( jItem[ "ie_mon" ].ToString() )}', '{parse2TwoDigital( jItem[ "ie_day" ].ToString() )}', '{szFamNo}', '{nItemNo}', '{jItem[ "place" ].ToString()}', '{jItem[ "code_amt" ].ToString()}', '{jItem[ "code_no" ].ToString()}', {szCodeName}, {szLastFourSQL}, {szUnit}, {szQty} )";
				szInsert += i == items.Count - 1 ? " " : ", ";
			}
			bool isSuccess = m_mssql.TryQuery( szInsert, out szErrorMsg );
			return isSuccess;
		}
		bool HugeInsert()
		{
			// for insert
			List<JObject> insertItems = JsonConvert.DeserializeObject<List<JObject>>(Request.Form[ Param.InsertItems ].ToString());
			if( insertItems.Count == 0 ) {
				return false;
			}

			// insert data
			InsertIntems(false, insertItems);

			// insert total cost
			int nTotalCosst = int.Parse(Request.Form[ Param.TotalCost ].ToString());
			string szRemark = Request.Form[ Param.DayRemark ].ToString();
			string szWhere = "";
			for( int i = 0; i < m_paramExpMList.Count; i++ ) {
				szWhere += $" {m_paramExpMList[ i ]}";
				szWhere += i == m_paramExpMList.Count - 1 ? " " : " AND";
			}

			string szErrorMsg;
			string szYear = Request.Form[ Param.Year ].ToString();
			string szMonth = Request.Form[ Param.Month ].ToString();
			string szDay = Request.Form[ Param.Day ].ToString();
			string szFamNo = Request.Form[ Param.FamNo ].ToString();
			string szInsertql = $"INSERT INTO {TableName.CoExpM} VALUES ('{szYear}', '{parse2TwoDigital(szMonth)}', '{parse2TwoDigital(szDay)}', '{szFamNo}', '{nTotalCosst}', '{szRemark}', '1', CURRENT_TIMESTAMP, '{szUserCode}', NULL, NUll, NULL, NULL, NULL, NULL)";
			return m_mssql.TryQuery(szInsertql, out szErrorMsg);
		}
		public string parse2TwoDigital(string szNumber)
		{
			int nTemp;
			if( int.TryParse(szNumber, out nTemp) == false ) {
				return string.Empty;
			}

			if( nTemp < 10 ) {
				return $"0{nTemp}";
			}

			return nTemp.ToString();
		}

		enum ApiAction
		{
			READ,
			WRITE,
			DELETE,
			HUGEINSERT,
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
			public static string Year
			{
				get
				{
					return "Year";
				}
			}
			public static string Month
			{
				get
				{
					return "Month";
				}
			}
			public static string FamNo
			{
				get
				{
					return "FamNo";
				}
			}
			public static string DurationStart
			{
				get
				{
					return "DurationStart";
				}
			}
			public static string DurationEnd
			{
				get
				{
					return "DurationEnd";
				}
			}
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

			// for write
			public static string Day
			{
				get
				{
					return "Day";
				}
			}
			public static string UpdateItems
			{
				get
				{
					return "UpdateItems";
				}
			}
			public static string InsertItems
			{
				get
				{
					return "InsertItems";
				}
			}
			public static string TotalCost
			{
				get
				{
					return "TotalCost";
				}
			}
			public static string DayRemark
			{
				get
				{
					return "Remark";
				}
			}

			// for delete
			public static string ItemArray
			{
				get
				{
					return "ItemArray";
				}
			}
		}
		MSSQL m_mssql = new MSSQL();
		List<string> m_paramExpDList = new List<string>();
		List<string> m_paramExpMList = new List<string>();
	}
}
