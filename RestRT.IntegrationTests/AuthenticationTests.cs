using System;
using System.Linq;
using System.Diagnostics;
using System.Net;
using System.Text;
using RestRT.Authenticators;
using RestRT.Contrib;
using RestRT.IntegrationTests.Helpers;
using Windows.Foundation;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace RestRT.IntegrationTests
{
    [TestClass]
	public class AuthenticationTests
	{
		[TestMethod]
		public async Task Can_Authenticate_With_Basic_Http_Auth()
		{
			const string baseUrl = "http://localhost:8080/";
            using (HttpServer server = new HttpServer(8080, request => UsernamePasswordEchoHandler(request) ))
			{
				var client = new RestClient(baseUrl);
				client.Authenticator = new HttpBasicAuthenticator("testuser", "testpassword");

				var request = new RestRequest("basicAuth.json.txt");
				var response = await client.ExecuteAsync(request);

				Assert.AreEqual("testuser|testpassword", response.Content);
			}
		}

        private async static Task<byte[]> UsernamePasswordEchoHandler(HttpServerRequest request)
        {
            var response = await Handlers.LoadResponse(request.Path);

            var authHeader = request.Headers["Authorization"];
            byte[] base64 = Convert.FromBase64String(authHeader.Substring("Basic ".Length));
            var parts = Encoding.UTF8.GetString(base64, 0, base64.Length).Split(':');

            byte[] bodyArray = Encoding.UTF8.GetBytes(string.Join("|", parts));

            string intro = String.Format("HTTP/1.1 {0} OK\r\n", response.StatusCode);

            string headers = Handlers.BuildHeaders(response.Headers);
            headers = headers + string.Format("Content-Length: {0}\r\n\r\n", bodyArray.Length);

            byte[] headerArray = Encoding.UTF8.GetBytes(intro + headers);

            byte[] result = headerArray.Concat(bodyArray).ToArray();
            return result;
        }

		//[Fact]
		public async void Can_Authenticate_With_OAuth()
		{
            WwwFormUrlDecoder decoder;

			var baseUrl = "https://api.twitter.com";
			var client = new RestClient(baseUrl);
			client.Authenticator = OAuth1Authenticator.ForRequestToken(
				"CONSUMER_KEY", "CONSUMER_SECRET"
				);
			var request = new RestRequest("oauth/request_token");
			var response = await client.ExecuteAsync(request);

			Assert.IsNotNull(response);
			Assert.AreEqual( (int)HttpStatusCode.OK, response.StatusCode);

            decoder = new WwwFormUrlDecoder(response.Content);
            var oauth_token = decoder.GetFirstValueByName("oauth_token");
            var oauth_token_secret = decoder.GetFirstValueByName("oauth_token_secret");

            //var qs = HttpUtility.ParseQueryString(response.Content);
            //var oauth_token = qs["oauth_token"];
            //var oauth_token_secret = qs["oauth_token_secret"];

            Assert.IsNotNull(oauth_token);
			Assert.IsNotNull(oauth_token_secret);

			request = new RestRequest("oauth/authorize?oauth_token=" + oauth_token);
			var url = client.BuildUri(request).ToString();
			//Process.Start(url);

			var verifier = "123456"; // <-- Breakpoint here (set verifier in debugger)
			request = new RestRequest("oauth/access_token");
			client.Authenticator = OAuth1Authenticator.ForAccessToken(
				"P5QziWtocYmgWAhvlegxw", "jBs07SIxJ0kodeU9QtLEs1W1LRgQb9u5Lc987BA94", oauth_token, oauth_token_secret, verifier
				);
			response = await client.ExecuteAsync(request);

			Assert.IsNotNull(response);
			Assert.AreEqual( (int)HttpStatusCode.OK, response.StatusCode);

            decoder = new WwwFormUrlDecoder(response.Content);
            oauth_token = decoder.GetFirstValueByName("oauth_token");
            oauth_token_secret = decoder.GetFirstValueByName("oauth_token_secret");

            //qs = HttpUtility.ParseQueryString(response.Content);
            //oauth_token = qs["oauth_token"];
            //oauth_token_secret = qs["oauth_token_secret"];
			
            Assert.IsNotNull(oauth_token);
			Assert.IsNotNull(oauth_token_secret);

			request = new RestRequest("account/verify_credentials.xml");
			client.Authenticator = OAuth1Authenticator.ForProtectedResource(
				"P5QziWtocYmgWAhvlegxw", "jBs07SIxJ0kodeU9QtLEs1W1LRgQb9u5Lc987BA94", oauth_token, oauth_token_secret
				);

			response = await client.ExecuteAsync(request);

			Assert.IsNotNull(response);
			Assert.AreEqual( (int)HttpStatusCode.OK, response.StatusCode);
		}

		//[Fact]
		//public void Can_Obtain_OAuth_Request_Token()
		//{
		//    var baseUrl = "http://term.ie/oauth/example";
		//    var client = new RestClient(baseUrl);
		//    client.Authenticator = new OAuthAuthenticator(baseUrl, "key", "secret");
		//    var request = new RestRequest("request_token.php");
		//    var response = client.Execute(request);

		//    Assert.NotNull(response);
		//    Assert.Equal("oauth_token=requestkey&oauth_token_secret=requestsecret", response.Content);
		//}

		//[Fact]
		//public void Can_Obtain_OAuth_Access_Token()
		//{
		//    var baseUrl = "http://term.ie/oauth/example";
		//    var client = new RestClient(baseUrl);
		//    client.Authenticator = new OAuthAuthenticator(baseUrl, "key", "secret", "requestkey", "requestsecret");
		//    var request = new RestRequest("access_token.php");
		//    var response = client.Execute(request);

		//    Assert.NotNull(response);
		//    Assert.Equal("oauth_token=accesskey&oauth_token_secret=accesssecret", response.Content);

		//}

		//[Fact]
		//public void Can_Make_Authenticated_OAuth_Call_With_Parameters()
		//{
		//    var baseUrl = "http://term.ie/oauth/example";
		//    var client = new RestClient(baseUrl);
		//    client.Authenticator = new OAuthAuthenticator(baseUrl, "key", "secret", "accesskey", "accesssecret");
		//    var request = new RestRequest("echo_api.php");
		//    request.AddParameter("foo", "bar");
		//    request.AddParameter("fizz", "pop");
		//    var response = client.Execute(request);

		//    Assert.NotNull(response);
		//    Assert.Equal("fizz=pop&foo=bar", response.Content);
		//}

		//[Fact]
		//public void Can_Make_Authenticated_OAuth_Call()
		//{
		//    var baseUrl = "http://term.ie/oauth/example";
		//    var client = new RestClient(baseUrl);
		//    client.Authenticator = new OAuthAuthenticator(baseUrl, "key", "secret", "accesskey", "accesssecret");
		//    var request = new RestRequest("echo_api.php");
		//    var response = client.Execute(request);

		//    Assert.NotNull(response);
		//    Assert.Equal(string.Empty, response.Content);

		//}

	}
}
