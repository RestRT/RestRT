using System;
using System.Linq;
using System.IO;
using RestRT.IntegrationTests.Helpers;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Windows.Storage;
using System.Threading.Tasks;


namespace RestRT.IntegrationTests
{
    [TestClass]
	public class FileTests
	{
        [TestMethod]
		public async Task Handles_Binary_File_Download()
		{
			const string baseUrl = "http://localhost:8080/";
            using (HttpServer server = new HttpServer(8080))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest("Koala.jpg");

                var response = await client.DownloadDataAsync(request);

                // Look in the Data subdirectory of the app package
                string filePath = "Data\\Koala.jpg";
                StorageFolder LocalFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;

                var file = await LocalFolder.GetFileAsync(filePath);
                Stream s = await file.OpenStreamForReadAsync();
                byte[] expected = new byte[s.Length];
                await s.ReadAsync(expected, 0, expected.Length);

                var actual = response.Cast<byte>().ToArray();
                Assert.IsTrue(actual.SequenceEqual(expected));
			}
		}

        //NOTE: Not supporting write-to-stream yet
        //[TestMethod]
        //public void Writes_Response_To_Stream()
        //{
        //    const string baseUrl = "http://localhost:8080/";
        //    using (HttpServer server = new HttpServer(8080, (resp, path) => Handlers.FileHandler(resp, path)))
        //    {
        //        string tempFile = Path.GetTempFileName();

        //        using (var writer = File.OpenWrite(tempFile))
        //        {
        //            var client = new RestClient(baseUrl);
        //            var request = new RestRequest("Assets/Koala.jpg");
        //            request.ResponseWriter = (responseStream) => responseStream.CopyTo(writer);
        //            var response = client.DownloadData(request);
        //            Assert.Null(response);
        //        }
        //        var fromTemp = File.ReadAllBytes(tempFile);
        //        var expected = File.ReadAllBytes(Environment.CurrentDirectory + "\\Assets\\Koala.jpg");
        //        Assert.Equal(expected, fromTemp);
        //    }
        //}
	}
}
