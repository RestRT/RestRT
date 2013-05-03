using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using RestRT.IntegrationTests.Helpers;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestRT.IntegrationTests
{
    [TestClass]
	public class CompressionTests
	{
        [TestMethod]
		public async Task Can_Handle_Gzip_Compressed_Content()
		{
			const string baseUrl = "http://localhost:8080/";
            using (HttpServer server = new HttpServer(8080, request => GzipEchoValue(request.Path)))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest("gzip.json.txt");
				var response = await client.ExecuteAsync(request);

                System.Diagnostics.Debug.WriteLine("CONTENT: " + response.Content);

                Assert.AreEqual("This is some gzipped content", response.Content);
			}
		}

        [TestMethod]
		public async Task Can_Handle_Deflate_Compressed_Content()
		{
			const string baseUrl = "http://localhost:8080/";
            using (HttpServer server = new HttpServer(8080, request => DeflateEchoValue(request.Path)))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest("deflate.json.txt");
				var response = await client.ExecuteAsync(request);

                System.Diagnostics.Debug.WriteLine("CONTENT: " + response.Content);

                Assert.AreEqual("This is some deflated content", response.Content);
			}
		}

        [TestMethod]
		public async Task Can_Handle_Uncompressed_Content()
		{
			const string baseUrl = "http://localhost:8080/";
            using (HttpServer server = new HttpServer(8080, request => Handlers.EchoValue(request.Path) ))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest("uncompressed.json.txt");
				var response = await client.ExecuteAsync(request);

                Assert.AreEqual("This is some sample content", response.Content);
			}
		}


        private async Task<byte[]> GzipEchoValue(string path)
        {
            var response = await Handlers.LoadResponse(path);

            byte[] bodyArray = Encoding.UTF8.GetBytes(response.Body.ToString());
            
            MemoryStream memory = new MemoryStream();
	        using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
	        {
    		    gzip.Write(bodyArray, 0, bodyArray.Length);
	        }
            byte[] compressedArray = memory.ToArray();
    
            string intro = String.Format("HTTP/1.1 {0} OK\r\n", response.StatusCode);

            string headers = Handlers.BuildHeaders(response.Headers);
            headers = headers + string.Format("Content-Length: {0}\r\n\r\n", compressedArray.Length);

            byte[] headerArray = Encoding.UTF8.GetBytes(intro + headers);

            byte[] result = headerArray.Concat(compressedArray).ToArray();
            return result;
        }

        private static async Task<byte[]> DeflateEchoValue(string path)
        {
            var response = await Handlers.LoadResponse(path);

            byte[] bodyArray = Encoding.UTF8.GetBytes(response.Body.ToString());

            MemoryStream memory = new MemoryStream();
            using (DeflateStream gzip = new DeflateStream(memory, CompressionMode.Compress, true))
            {
                gzip.Write(bodyArray, 0, bodyArray.Length);
            }
            byte[] compressedArray = memory.ToArray();

            string intro = String.Format("HTTP/1.1 {0} OK\r\n", response.StatusCode);

            string headers = Handlers.BuildHeaders(response.Headers);
            headers = headers + string.Format("Content-Length: {0}\r\n\r\n", compressedArray.Length);

            byte[] headerArray = Encoding.UTF8.GetBytes(intro + headers);

            byte[] result = headerArray.Concat(compressedArray).ToArray();
            return result;
        }
	}
}
