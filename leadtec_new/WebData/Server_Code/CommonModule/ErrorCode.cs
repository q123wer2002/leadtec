namespace IncomeStatement.WebData.Server_Code.CommonModule
{
	public enum ErrorCode : int
	{
		Error = 1000,
		Success = 1001,
		AuthenticationError = 1002,
		NoAuthorization = 1003,
		CheckEventNoEvent = 1,
		CheckEventExist = 2,
		UpdateSuccess = 3,
		UpdateAlreadyEnd = 4,
		InsertSuccess = 5,
		InsertPrevious = 6,
	}
}
