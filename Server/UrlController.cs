using System;
using System.Net;
using System.Text.Json;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.IO;
/// <TODO>
/// - todo Crear documentacion 
/// - Lucirme con el servidor y generar log con fecha, url consultada y respuestas http. >:) 
/// </TODO>
/// <sumary>
/// La clase <c>UrlInfo</c> se usa para definir la informacion necesaria
/// Para que los hijos de la clase <c>UrlController</c> puedan escuchar una nueva URL.
/// </sumary>
/// <example>
///		<include file="./doc/example/UrlController.xml" path='extradoc/class[@name="UrlInfo"]/codeMain*' />
/// </example>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
class UrlInfo: Attribute {
	public UrlInfo(string urlName):this(urlName,null){}
	public UrlInfo(string urlName, string? headerJson){
		UrlName=urlName;
		header=(headerJson!=null)?
			JsonSerializer.Deserialize<Dictionary<string,string>>(headerJson)
			:new Dictionary<string, string>();
	}

	protected string urlName="";// Para desactivar la null advertensia.
	public string UrlName{
		get {return urlName;}
		set {
			if (value==null)
				throw new ArgumentException("La URL no debe ser un valor nullo.",nameof(value));
			Uri? uriResult;
			// UriKind.Relative no found
			bool result = Uri.TryCreate(value, UriKind.Absolute, out uriResult) 
				&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
			if (result){
				throw new ArgumentException("La URL relativa no es valida", nameof(value));
			}
			this.urlName=value;
		}
	}
	public Dictionary<string,string> header=new Dictionary<string,string>;
}
/// <summary>
/// Class <c>UrlController</c> It helps with server response and facilitates listening for events related to requests.
/// </summary>
/// <TODO>Agregar ejemplo para heredar esta clase.</TODO>
class UrlController{
	// No debe contener valores nullos, pero no quiero ver esta advertensia.
	#pragma warning disable CS8618
	public HttpListenerContext context;
	#pragma warning restore CS8618
	protected const string path404codes="codes/404.html";
	public void ProcessEventUrl(){
		Type type = this.GetType();
		foreach(MethodInfo mInfo in type.GetMethods(BindingFlags.Public|BindingFlags.Instance)) {
			foreach(Attribute attr in Attribute.GetCustomAttributes(mInfo)){
				if (attr!=null && attr.GetType() == typeof(UrlInfo)
					&& context.Request.Url.AbsolutePath==((UrlInfo)attr).UrlName){
					/*-------------Url----------*/
					Send((object[] parameters)=>mInfo.Invoke(this,parameters),(UrlInfo)attr);
					return;
				}
			}
		}
		Send((object[] c)=>(object)UrlController.PageNotFound,null);
	}
	public static string PageNotFound()=>UrlController.GetStaticFile(path404codes);
	public static string GetStaticFile(string url){
		try{
			return File.ReadAllText($"./static/{url}");
		}catch(IOException e){
			Console.WriteLine($"{e.Message}\nError al leer el archivo \"./static/{url}\".");
			if (File.Exists(path404codes)){
				return UrlController.PageNotFound();
			}
		}
		return "<h1>Error 404: Pagina no disponible.</h1>";
	}
	/// <summary>
	/// Method <c>Send</c> Sets the body and header of the HTTP response.
	/// </summary>
	/// <param name="getResult"></param>
	/// <param name="attr"></param>
	protected void Send(Func<object[],object> getResult, UrlInfo attr){
		/*Obtenemos la pagina.*/
		/// <TODO> Modificar para ver si incluimos resultados binarios.</TODO>
		/// <TODO>Enviar como parametros la peticiones dada.</TODO>
		string responseString=(string)getResult(null);
		/*Enviamos la pagina.*/
		byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
		context.Response.ContentLength64 = buffer.Length;
		context.Response.OutputStream.Write(buffer, 0, buffer.Length);
		UrlController.MakeHeader(context,attr);
		/// <TODO>
		/// Tratar los header, Url debe preguntar lo necesario 
		/// y tambien tratar los atributos.
		/// recordar el encabezado 404
		/// </TODO>
	}
	/// <TODO>Hacer que los parametros de la función se tome de los Metodos Get o Post</TODO>
	/// <Remark>Al borrar esto Ver la funcion ComoPath</Remark>
	public static void MakeHeader(HttpListenerContext context, UrlInfo? attr=null){

	}
}