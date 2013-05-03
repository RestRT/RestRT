using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using RestRT.IntegrationTests.Helpers;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RestRT.IntegrationTests
{
    [TestClass]
    public class StatusCodeTests
    {
        [TestMethod]
        public async Task Handles_GET_Request_404_Error()
        {
            const string baseUrl = "http://localhost:8080/";
            using (HttpServer server = new HttpServer(8080, request => UrlToStatusCodeHandler(request.Path) ))
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("404");
                var response = await client.ExecuteAsync(request);

                Assert.AreEqual((int)HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        /// <summary>
        /// Success of this test is based largely on the behavior of your current DNS.
        /// For example, if you're using OpenDNS this will test will fail; ResponseStatus will be Completed.
        /// </summary>
        [TestMethod]
        public async Task Handles_Non_Existent_Domain()
        {
            var client = new RestClient("http://nonexistantdomainimguessing.org");
            var request = new RestRequest("foo");
            var response = await client.ExecuteAsync(request);
            Assert.AreEqual(ResponseStatus.Error, response.ResponseStatus);
        }

        [TestMethod]
        public async Task Handles_Different_Root_Element_On_Error()
        {
            const string baseUrl = "http://localhost:8080/";
            using (HttpServer server = new HttpServer(8080, request => ResponseHandler(request.Path)) )
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("error.json.txt");

                var response = await client.ExecuteAsync(request);

                var deserializer = new RestRT.Deserializers.JsonDeserializer();
                deserializer.RootElement = "Success";

                if (response.StatusCode == (int)HttpStatusCode.BadRequest)
                {
                    deserializer.RootElement = "Error";
                }

                var result = (Response)deserializer.Deserialize(response, typeof(Response));

                Assert.AreEqual((int)HttpStatusCode.BadRequest, response.StatusCode);
                Assert.AreEqual("Not found!", result.Message);
            }
        }

        [TestMethod]
        public async Task Handles_Default_Root_Element_On_No_Error()
		{
			const string baseUrl = "http://localhost:8080/";
            using (HttpServer server = new HttpServer(8080, request => ResponseHandler(request.Path)) )
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest("success.json.txt");

                var response = await client.ExecuteAsync(request);

                var deserializer = new RestRT.Deserializers.JsonDeserializer();
                deserializer.RootElement = "Success";

                if (response.StatusCode == (int)HttpStatusCode.NotFound)
                {
                    deserializer.RootElement = "Error";
                }
                var result = (Response)deserializer.Deserialize(response, typeof(Response));

                Assert.AreEqual((int)HttpStatusCode.OK, response.StatusCode);
                Assert.AreEqual("Works!", result.Message);
			}
		}

        private async Task<byte[]> ResponseHandler(string path)
        {
            var response = await Handlers.LoadResponse(path);

            byte[] bodyArray = Encoding.UTF8.GetBytes(response.Body.ToString());

            string intro = String.Format("HTTP/1.1 {0} OK\r\n", response.StatusCode);

            string headers = Handlers.BuildHeaders(response.Headers);
            headers = headers + string.Format("Content-Length: {0}\r\n\r\n", bodyArray.Length);

            byte[] headerArray = Encoding.UTF8.GetBytes(intro + headers);

            byte[] result = headerArray.Concat(bodyArray).ToArray();
            return result;
        }

        private async static Task<byte[]> UrlToStatusCodeHandler(string path)
        {
            var response = await Handlers.LoadResponse(path);

            byte[] bodyArray = Encoding.UTF8.GetBytes(response.Body.ToString());

            string intro = String.Format("HTTP/1.1 {0} OK\r\n", "404");

            string headers = Handlers.BuildHeaders(response.Headers);
            headers = headers + string.Format("Content-Length: {0}\r\n", bodyArray.Length);

            byte[] headerArray = Encoding.UTF8.GetBytes(intro + headers);

            byte[] result = headerArray.Concat(bodyArray).ToArray();
            return result;

            //obj.Response.StatusCode = int.Parse(obj.Request.Url.Segments.Last());
        }
    }

	public class Response
	{
		public string Message { get; set; }
	}
}