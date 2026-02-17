using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using SimpleServerHTTP;
using Scriban;
using Scriban.Runtime;

class HttpServer
{
	static void Main(string[] args){
		StaticFile.RootPath=Directory.GetCurrentDirectory()+"/..";
		ExampleClass.runServer("http://localhost:8090/");
	}
}
class ExampleClass : UrlController{
	private string _header="";
	private string _footer="";
	public ExampleClass():base(){
		string footer_path=$"{StaticFile.RootPath}/example/template/footer.html";
		string header_path=$"{StaticFile.RootPath}/example/template/header.html";
		var footer = Template.Parse(File.ReadAllText(footer_path), footer_path);
		_footer=footer.Render();

		var header = Template.Parse(File.ReadAllText(header_path), header_path);
		_header=header.Render();
	}
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
	}
	[UrlInfo("/")]
	public string RootPath(){
		var template = Template.Parse(File.ReadAllText($"{StaticFile.RootPath}/example/template/Basic.html"), "./example/template/Basic.html");
		var url_examples = new List<Dictionary<string, string>>{
			new Dictionary<string, string>(){{"url", "/hello/world"}, {"title", "Un clasico ejemplo de Hola Mundo."}}
		};
		return template.Render(new { header=this._header, footer=this._footer, examples=url_examples});
	}
	[UrlInfo("/favicon.ico",@"{""content-type"":""image/png""}")]
	public byte[] Favicon(){
		return StaticFile.GetImage("static/media/favicon.png");
	}
	[UrlInfo("/Basic.css")]
	public string basic_css(){
		return StaticFile.GetString("example/static/Basic.css");
	}
	/// <Remark>Al borrar esto Ver la funcion MakeHeader</Remark>
	[UrlInfo("/hello/world",@"{
			""Method"":""Get"",
			""Content-Type"":""text/html; charset=utf-8""
		}")]
	public string HelloWorld(string msg){
		return $"<link rel=\"stylesheet\" href=\"/Basic.css\"/>\n<header><h1>Hola mundo Ejemplo</h1></header>\n<main><h1>Hola mundo y hola {msg}.</h1></main>";
	}

}