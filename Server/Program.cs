using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
//Substring
/// <TODO>Hacer algunas opciones cli.</TODO>
class HttpServer
{
	static void Main(string[] args){
		HttpListener listener = new HttpListener();
		SetUrls urlController=new SetUrls();

		listener.Prefixes.Add("http://localhost:8090/");
		listener.Start();
		Console.WriteLine("Listening...");

		while (true){
			urlController.context = listener.GetContext();
			urlController.ProcessEventUrl();
			urlController.context.Response.Close();
		}
	}
}
class SetUrls:UrlController{
	public SetUrls():base(){}
	[UrlInfo("/")]
	public string RootPath(){
		return "<h1>Hola como estas vossssss.</h1>";
	}
	/// <TODO>Hacer que los parametros de la función se tome de los Metodos Get o Post</TODO>
	/// <Remark>Al borrar esto Ver la funcion MakeHeader</Remark>
	[UrlInfo("/como.html",@"{
			'Method':'Get',
			'Content-Type':'text/html; charset=utf-8'
		}")
	]
	public string ComoPath(){
		return "<h1>Yo bueeeeen</h1>";
	}
}