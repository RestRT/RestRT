using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace RestRT.IntegrationTests.Helpers
{
	public static class Handlers
	{
        public async static Task<HttpServerResponse> LoadResponse(string path)
        {
            // Look in the Data subdirectory of the app package
            string filePath = "Data" + path.Replace('/', '\\');
            StorageFolder LocalFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;

            var file = await LocalFolder.GetFileAsync(filePath);
            var data = await FileIO.ReadTextAsync(file);

            var result = await JsonConvert.DeserializeObjectAsync<HttpServerResponse>(data);

            return result;
        }

        public static string BuildHeaders(Dictionary<string, string> headers)
        {
            string header = string.Empty;
            foreach (var key in headers.Keys)
            {
                header = header + string.Format("{0}: {1}\r\n", key, headers[key]);
            }

            return header;
        }

		/// <summary>
		/// Echoes the request input back to the output.
		/// </summary>
        //public static void Echo(HttpListenerContext context)
        //{
        //    context.Request.InputStream.CopyTo(context.Response.OutputStream);
        //}

		/// <summary>
		/// Echoes the given value back to the output.
		/// </summary>
        public async static Task<byte[]> EchoValue(string path)
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

		/// <summary>
		/// Response to a request like this:  http://localhost:8080/assets/koala.jpg
		/// by streaming the file located at "assets\koala.jpg" back to the client.
		/// </summary>
        public async static Task<byte[]> FileHandler(string path)
        {
            var response = await Handlers.LoadResponse(path);

            string filePath = "Data\\" + path.Replace('/', '\\');
            StorageFolder LocalFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;

            var file = await LocalFolder.GetFileAsync(filePath);
            Stream stream = await file.OpenStreamForReadAsync();
            byte[] data = new byte[stream.Length];
            await stream.ReadAsync(data, 0, data.Length);

            string intro = String.Format("HTTP/1.1 {0} OK\r\n", response.StatusCode);

            string headers = Handlers.BuildHeaders(response.Headers);
            headers = headers + string.Format("Content-Length: {0}\r\n", data.Length);

            byte[] headerArray = Encoding.UTF8.GetBytes(intro + headers);

            byte[] result = headerArray.Concat(data).ToArray();
            return result;
        }

		/// <summary>
		/// T should be a class that implements methods whose names match the urls being called, and take one parameter, an HttpListenerContext.
		/// e.g.
		/// urls exercised:  "http://localhost:8080/error"  and "http://localhost:8080/get_list"
		/// 
		/// class MyHandler
		/// {
		///   void error(HttpListenerContext ctx)
		///   {
		///     // do something interesting here
		///   }
		///
		///   void get_list(HttpListenerContext ctx)
		///   {
		///     // do something interesting here
		///   }
		/// }
		/// </summary>
        //public static Action<HttpListenerContext> Generic<T>() where T : new()
        //{
        //    return ctx =>
        //    {
        //        var methodName = ctx.Request.Url.Segments.Last();
        //        var method = typeof(T).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

        //        if(method.IsStatic)
        //        {
        //            method.Invoke(null, new object[] { ctx });
        //        }
        //        else
        //        {
        //            method.Invoke(new T(), new object[] { ctx });
        //        }
        //    };
        //}
	}
}