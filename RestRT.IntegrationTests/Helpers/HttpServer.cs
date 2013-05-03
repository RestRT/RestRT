using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using Windows.Storage.Streams;
using Newtonsoft.Json;
using System.Runtime.Serialization.Json;

namespace RestRT.IntegrationTests.Helpers
{
    public class HttpServer : IDisposable
    {
        public delegate byte[] HttpServerResponse(HttpServerRequest request);
        public delegate Task<byte[]> HttpServerResponseAsync(HttpServerRequest request);

        private const uint BufferSize = 8192;
        private static readonly StorageFolder LocalFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;

        private readonly StreamSocketListener listener;

        public HttpServerResponse ServerResponseCallback { get; private set; }
        public HttpServerResponseAsync ServerResponseCallbackAsync { get; private set; }

        public HttpServer(int port)
        {
            this.listener = new StreamSocketListener();
            this.listener.ConnectionReceived += (s, e) => ProcessRequestAsync(e.Socket);
            this.listener.BindServiceNameAsync(port.ToString());
        }

        public HttpServer(int port, HttpServerResponse callback)
        {
            this.ServerResponseCallback = callback;

            this.listener = new StreamSocketListener();
            this.listener.ConnectionReceived += (s, e) => ProcessRequestAsync(e.Socket);
            this.listener.BindServiceNameAsync(port.ToString());
        }

        public HttpServer(int port, HttpServerResponseAsync callback)
        {
            this.ServerResponseCallbackAsync = callback;

            this.listener = new StreamSocketListener();
            this.listener.ConnectionReceived += (s, e) => ProcessRequestAsync(e.Socket);
            this.listener.BindServiceNameAsync(port.ToString());
        }

        public void Dispose()
        {
            this.listener.Dispose();
        }

        private async void ProcessRequestAsync(StreamSocket socket)
        {
            // this works for text only
            StringBuilder rawRequest = new StringBuilder();
            using (IInputStream input = socket.InputStream)
            {
                byte[] data = new byte[BufferSize];
                IBuffer buffer = data.AsBuffer();
                uint dataRead = BufferSize;
                while (dataRead == BufferSize)
                {
                    await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                    rawRequest.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                    dataRead = buffer.Length;
                }
            }

            var request = ParseRawRequest(rawRequest.ToString());

            using (IOutputStream output = socket.OutputStream)
            {
                if (request.Method == "GET")
                    await WriteResponseAsync(request, output);
                else
                    throw new InvalidDataException("HTTP method not supported: " + request.Method);
            }
        }

        private HttpServerRequest ParseRawRequest(string rawrequest)
        {
            var request = new HttpServerRequest();
            
            string[] requestParts = rawrequest.ToString().Split(new string[] { "\r\n\r\n" }, StringSplitOptions.None);

            string rawHeader = requestParts[0];
            request.Body = requestParts[1];

            string[] headerParts = rawHeader.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None);
            string[] httpStartLineParts = headerParts[0].Split(' ');

            request.Method = httpStartLineParts[0];
            request.Path = httpStartLineParts[1];
            
            request.Headers = new Dictionary<string, string>();
            for (int i = 1; i < headerParts.Length; i++)
            {
                string[] header = headerParts[i].Split(':');

                request.Headers.Add(header[0].Trim(), header[1].Trim());
            }

            return request;
        }

        private async Task WriteResponseAsync(HttpServerRequest request, IOutputStream os)
        {
            using (Stream response = os.AsStreamForWrite())
            {
                bool exists = true;
                try
                {
                    if (ServerResponseCallback != null)
                    {
                        byte[] bytes = ServerResponseCallback(request);
                        await response.WriteAsync(bytes, 0, bytes.Length);
                    }
                    else if (ServerResponseCallbackAsync != null)
                    {
                        byte[] bytes = await ServerResponseCallbackAsync(request);

                        System.Diagnostics.Debug.WriteLine(System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length));

                        await response.WriteAsync(bytes, 0, bytes.Length);
                    }
                    else
                    {
                        // Look in the Data subdirectory of the app package
                        string filePath = "Data" + request.Path.Replace('/', '\\');
                        using (Stream fs = await LocalFolder.OpenStreamForReadAsync(filePath))
                        {
                            string header = String.Format("HTTP/1.1 200 OK\r\n" +
                                             "Content-Length: {0}\r\n" +
                                             "Connection: close\r\n\r\n",
                                             fs.Length);
                            byte[] headerArray = Encoding.UTF8.GetBytes(header);
                            await response.WriteAsync(headerArray, 0, headerArray.Length);
                            await fs.CopyToAsync(response);
                        }
                    }
                }
                catch (FileNotFoundException)
                {
                    exists = false;
                }

                if (!exists)
                {
                    byte[] headerArray = Encoding.UTF8.GetBytes(
                                          "HTTP/1.1 404 Not Found\r\n" +
                                          "Content-Length:0\r\n" +
                                          "Connection: close\r\n\r\n");
                    await response.WriteAsync(headerArray, 0, headerArray.Length);
                }

                await response.FlushAsync();
            }
        }
    }

    public class HttpServerRequest
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }
    }
    
    public class HttpServerResponse
    {
        public string StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }
    }
}
