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
		var server=new ExampleClass();
		var msg_end=@"         Server is down.
Thank you for trying SimpleServerHTTP.";
		Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e)=>{
			Console.WriteLine(msg_end);
			if (server.context!=null)
				server.context.Response.Close();
			e.Cancel=true;
			Environment.Exit(0);
		};
		listener.Prefixes.Add(path);
		listener.Start();
		Console.WriteLine($"Listening on '{path}'...");
		Console.WriteLine($"Ctlr+C for finish.");
		while (true){
			#if DEBUG
				String hourMinute = DateTime.Now.ToString("HH:mm:ss.fff");
			#endif
			server.ProcessEventUrl(listener.GetContext());
			#if DEBUG
				Console.Write($"- {hourMinute} - '{server.context.Request.Url.AbsolutePath}' -> ");
				Console.Write($" {DateTime.Now.ToString("HH:mm:ss.fff")} - 'HTTP {server.context.Response.StatusCode}'\n");
			#endif
			server.context.Response.Close();
			server.context=null;
		}
		Console.WriteLine(msg_end);
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