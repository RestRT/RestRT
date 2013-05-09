RestRT is a simple, easy to use REST and HTTP client for Windows Store Apps. Its public API is inspired by RestSharp, while internally it takes advantage of many new features of .NET 4.5 such as async/await and the new HttpClient API's.

### Installation

The best and easiest way to add RestRT to your Windows Store App project is to use the NuGet package manager. NuGet is a Visual Studio extension that makes it easy to install and update third-party libraries and tools in Visual Studio.

NuGet is available for Visual Studio 2010 and Visual Studio 2012, and you can find instructions for installing the NuGet extension on the NuGet.org website:

http://docs.nuget.org/docs/start-here/installing-nuget

Once you have installed the NuGet extension, you can choose to install the Twilio libraries using either the Package Manager dialog, or using the Package Manager console.

### Examples

Once you've added RestRT to your project its easy to use with eother C# or JavaScript.

#### CSharp

```
var client = new RestClient("http://example.com");
            
var request = new RestRequest("resource/{id}");
              
request.AddParameter("name", "value");  // adds to POST or URL querystring based on Method
request.AddUrlSegment("id", 123); // replaces matching token in request.Resource

// execute the request
var response = await client.ExecuteAsync(request);
var content = response.Content; // raw content as string

//deserialize JSON response
var deserializer = new Deserializers.JsonDeserializer();
var person = deserializer.Deserialize(response, typeof(Person));
```

#### JavaScript

```
var client = new RestRT.RestClient("http://example.com");
            
var request = new RestRT.RestRequest("resource/{id}");

request.addParameter("name", "value");  // adds to POST or URL querystring based on Method
request.addUrlSegment("id", 123); // replaces matching token in request.Resource

// execute the request
client.executeAsync(request).done(function(response) {
  var content = response.content;  // raw content as string

  //deserialize JSON response
  var person = JSON.parse(content);
});
```

### Limitations

RestRT currently has several limitations when compared to RestSharp:

* Only supports serialization and deserialization of JSON formatted data. XML serialization/deserialization is not supported.
* Only supports OAuth1 authorization. OAuth2 is supported by native Windows API's
* NTLM authentication is not supported
