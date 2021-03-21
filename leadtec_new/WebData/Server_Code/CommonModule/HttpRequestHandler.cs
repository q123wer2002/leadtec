using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IncomeStatement.WebData.Server_Code.CommonModule
{
	public class HttpRequestHandler
	{
		public async Task<dynamic> SendPostReturnJObject( dynamic dySendObject, string szSendRestfulAddress, Dictionary<string, string> AdditionalHeaderInfos )
		{
			using( HttpClient client = new HttpClient() ) {
				dynamic dyResult;
				try {
					// for aws
					if( AdditionalHeaderInfos != null ) {
						foreach( KeyValuePair<string, string> AdditionalHeaderInfo in AdditionalHeaderInfos ) {
							client.DefaultRequestHeaders.Add( AdditionalHeaderInfo.Key, AdditionalHeaderInfo.Value );
						}
					}

					string szJSONContent = JsonConvert.SerializeObject( dySendObject );
					HttpResponseMessage response = await client.PostAsync( szSendRestfulAddress, new StringContent( szJSONContent, Encoding.UTF8, "application/json" ) );
					response.EnsureSuccessStatusCode();
					string responseBody = await response.Content.ReadAsStringAsync();
					//parse body as dynamic json object
					JObject jobjResult = JObject.Parse( responseBody );
					dyResult = jobjResult as dynamic;
				}
				catch( HttpRequestException e ) {
					dyResult = new JObject();
					dyResult.Error = e.ToString();
				}
				return dyResult;
			}
		}
	}
}
