using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
//Substring
/// <TODO>Hacer algunas opciones cli.</TODO>
class HttpServer
{
	static void Main(string[] args){
		string path="http://localhost:8090/";
		HttpListener listener = new HttpListener();
		SetUrls urlController=new SetUrls();

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
}
class SetUrls:UrlController{
	public SetUrls():base(){}
	[UrlInfo("/",@"{""Method"":""post""}")]
	public string RootPath(string a){
		return $"<h1> Roooot {a} path</h1>";
	}
	[UrlInfo("/favicon.ico",@"{""content-type"":""image/png""}")]
	public byte[] Favicon(string a){
		return StaticFile.GetImage("./static/media/favicon.png");
	}
	/// <Remark>Al borrar esto Ver la funcion MakeHeader</Remark>
	[UrlInfo("/como/",@"{
			""Method"":""Get"",
			""Content-Type"":""text/html; charset=utf-8""
		}")]
	public string ComoPath(){
		return "<h1>Yo bueeeeen</h1>";
	}
}