# Simple Server for HTTP

This project was created to facilitate the rapid development of a local server.

## Requirement
- dotnet: This project is compiled using dotnet version 8.0.401.

## Compilation and Execution
Only <code>dotnet build</code> and <code>dotnet run</code> are needed to run the example.

## Getting Started
The way to create a URL is based on Python's Flask because of its ease, but the configurations are different:
```
using System.Net;
using SimpleServerHTTP;
// The "UrlController" class is responsible for finding the requested URLs and calling the corresponding function, and for handling the headers.
class ExampleClass: UrlController{
	public ExampleClass():base(){}
	// To register a URL and its function, you must do the following:
	// /favicon.ico
	// [UrlInfo(url,@"{""opcional-config-of-header"":""...""}")]
	[UrlInfo("/favicon.ico",@"{""content-type"":""image/png""}")]
	public byte[] Favicon(){
		return StaticFile.GetImage("./static/media/favicon.png");
	}
	// or
	// /hello/world?msg=lector
	[UrlInfo("/hello/world",@"{
		""Method"":""Get"",
		""Content-Type"":""text/html; charset=utf-8""
	}")]
	public string HelloWorld(string msg){
		return $"<h1>Hola mundo</h1>y<h1>Hola \"{msg}\".</h1>";
	}
	// or
	[UrlInfo("/")]
	public string RootPath(){
		return @"
<html>
<head>
	<title>SimpleServerHTTP</title>
</head>
<body>
	<h1>Welcome to SimpleServerHTTP</h1>
	<p>The available URLs are:</p>
	<ul>
		<li><a href=""/hello/world"">Hello World</a></li>
		<li><a href=""/favicon.ico"">Page icon</a></li>
	</ul>
</body>
</html>
";
	}
	public void run(){
		StaticFile.RootPath="YOU_PATH/FILES/AND_IMAGE/";
		var listener = new HttpListener();
		string path = "http://localhost:8090/";
		listener.Prefixes.Add(path);
		listener.Start();
		Console.WriteLine($"Listening on '{path}'...");
		while (true){
			this.ProcessEventUrl(listener.GetContext());
			this.context.Response.Close();
		}
	}
}
```
As shown in the code, we can analyze the following:

- All functions with UrlInfo attributes will be called by their corresponding URLs.

- To process the client's request, you must call <code>ProcessEventUrl</code>. This is done this way to give you, the developer, more freedom.

- For now, only two return types are allowed: byte[] and string. This is because the response is ultimately sent as byte[], and the string is converted to byte[].

- Header configurations are currently only available in JSON-formatted strings and are optional parameters.

- Function parameters are taken from the URL. If they are not available, then "" is passed.

- The return value will be the response to the client.

## Dependencies:
- Scriban 6.5.2: To provide templates.