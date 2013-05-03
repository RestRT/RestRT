using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RestRT.IntegrationTests.Helpers;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Threading.Tasks;

namespace RestRT.IntegrationTests
{
    [TestClass]
	public class AsyncTests
	{
		[TestMethod]
		public async Task Can_Perform_GET_Async()
		{
			const string baseUrl = "http://localhost:8080/";
			const string val = "Basic async test";
			var resetEvent = new ManualResetEvent(false);
			//using (SimpleServer.Create(baseUrl, Handlers.EchoValue(val)))
            using (HttpServer server = new HttpServer(8080, request => Handlers.EchoValue(request.Path)))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest("");

                var response = await client.ExecuteAsync(request);
                
                Assert.IsNotNull(response.Content);
                Assert.AreEqual(val, response.Content);
			}
		}

		[TestMethod]
		public async Task Can_Perform_GET_Async_Without_Async_Handle()
		{
			const string baseUrl = "http://localhost:8080/";
			const string val = "Basic async test";
			var resetEvent = new ManualResetEvent(false);

            //using (SimpleServer.Create(baseUrl, Handlers.EchoValue(val)))
            using (HttpServer server = new HttpServer(8080, request =>  Handlers.EchoValue(request.Path)))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest("");

				var response = await client.ExecuteAsync(request);

				Assert.IsNotNull(response.Content);
				Assert.AreEqual(val, response.Content);
				
			}
		}

	}
}
