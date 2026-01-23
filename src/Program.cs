using System;
using System.Net;
using System.IO;
using System.Collections.Generic;

class HttpServer
{
	static void Main(string[] args){
		ExampleClass.runServer("http://localhost:8090/");
	}
}
class ExampleClass:UrlController{
	public ExampleClass():base(){}
	static public void runServer(string path){
		var listener = new HttpListener();
		var urlController=new ExampleClass();

		listener.Prefixes.Add(path);
		listener.Start();
		Console.WriteLine($"Listening on '{path}'...");
		while (true){
			urlController.context = listener.GetContext();
			#if DEBUG
				String hourMinute = DateTime.Now.ToString("HH:mm:ss.fff");
			#endif
			urlController.ProcessEventUrl();
			#if DEBUG
				Console.Write($"- {hourMinute} - '{urlController.context.Request.Url.AbsolutePath}' -> ");
				Console.Write($" {DateTime.Now.ToString("HH:mm:ss.fff")} - 'HTTP {urlController.context.Response.StatusCode}'\n");
			#endif
			urlController.context.Response.Close();
		}
	}
	[UrlInfo("/")]
	public string RootPath(){
		return @"
<html>
<head>
	<title>SimpleServerHTTP</title>
</head>
<body>
	<h1>Bienvenido al SimpleServerHTTP</h1>
	<p>Las urls disponibles son:</p>
	<ul>
		<li><a href=""/hello/world"">Hola mundo</a></li>
		<li><a href=""/favicon.ico"">Icono de la página</a></li>
	</ul>
</body>
</html>
";
	}
	[UrlInfo("/favicon.ico",@"{""content-type"":""image/png""}")]
	public byte[] Favicon(string a){
		return StaticFile.GetImage("./static/media/favicon.png");
	}
	/// <Remark>Al borrar esto Ver la funcion MakeHeader</Remark>
	[UrlInfo("/hello/world",@"{
			""Method"":""Get"",
			""Content-Type"":""text/html; charset=utf-8""
		}")]
	public string HelloWorld(string msg){
		return $"<h1>Hola mundo</h1>y<h1>Hola {msg}</h1>";
	}

}