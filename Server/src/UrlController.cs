using System;
using System.Net;
using System.Text.Json;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.IO;
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
	protected string urlName="";// To disable the null warning.
	public string UrlName{
		get {return urlName;}
		set {
			if (value==null)
				throw new ArgumentException("The URL cannot be a null value.",nameof(value));
			Uri? uriResult;
			bool result = Uri.TryCreate(value, UriKind.Absolute, out uriResult) // UriKind.Relative no found
				&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)
			;
			if (result)
				throw new ArgumentException("The URL is invalid", nameof(value));
			this.urlName=value;
		}
	}
	public Dictionary<string,string> header;
}
/// <summary>
/// Class <c>UrlController</c> It helps with server response
/// and facilitates listening for events related to requests.
/// </summary>
class UrlController{
	// It should not contain null values, but I don't want to see this warning.
	#pragma warning disable CS8618
	public HttpListenerContext context;
	#pragma warning restore CS8618
	protected const string path404codes="codes/404.html";
	/// <summary>
	/// According to the request within the variable <c>UrlController.context</c>,
	/// Search within the instances of this class for a function that
	/// has the attribute <c>UrlInfo</c> and the requested URL is
	/// the same as <c>UrlInfo.UrlName</c> If it finds one, it calls
	/// that function and gives a response to the client.
	/// </summary>
	public void ProcessEventUrl(){
		Type type = this.GetType();
		foreach(MethodInfo mInfo in type.GetMethods(BindingFlags.Public|BindingFlags.Instance)) {
			foreach(Attribute attr in Attribute.GetCustomAttributes(mInfo)){
				if (attr!=null && attr.GetType() == typeof(UrlInfo)
					&& context.Request.Url.AbsolutePath==((UrlInfo)attr).UrlName){
					context.Response.StatusCode = 200; // Default
					//-------------Url----------
					Send(mInfo,(UrlInfo)attr);
					return;
				}
			}
		}
		Send(((Func<string>)PageNotFound).Method, null);
	}
	
	/// <summary>
	/// Method <c>Send</c> Sets the body and header of the HTTP response.
	/// </summary>
	/// <param name="func">
	/// It executes the function and obtains the response from the requested URL.
	/// </param>
	/// <param name="attr">
	/// Information needed to create and obtain the header 
	/// information defined by the function.
	/// </param>
	/// <exception cref="OperationCanceledException">
	/// The return type of the function to respond to the client has not been addressed.
	/// </exception>
	/// <exception cref="NullReferenceException">
	/// The function to respond to the client is return null.
	/// </exception>
	private void Send(MethodInfo func, UrlInfo? attr){
		byte[] buffer;
		string method=(attr!=null && attr.header.TryGetValue("Method",out method))?
			method:"GET" // Get by default.
		;
		string[] paramValue=(!string.Equals(method,"POST", StringComparison.OrdinalIgnoreCase))?
			GetMethodParam(context,func) : PostMethodParam(context,func)
		;
		Func<Object, Object[], Object> funcInvoke=func.Invoke;

		// Configuramos los Header
		MakeHeader(context, attr);

		// Obtenemos la pagina.
		if (typeof(string)==func.ReturnType){
			funcInvoke=(object class_name, object[] param)=>{
				string? responseString=(string)func.Invoke(this,(object[])paramValue);
				return (responseString==null)?
					null:
					System.Text.Encoding.UTF8.GetBytes(responseString);
			};
		}else if (typeof(byte[])!=func.ReturnType)
			throw new OperationCanceledException($"Para el tipo '{func.ReturnType}' no se ha hecho una opción para convertirse en 'byte[]' y ser enviado al cliente.");
		
		buffer=(byte[])funcInvoke(this,(object[])paramValue);
		if (buffer==null)
			throw new NullReferenceException($"La función '{func.ReflectedType}.{func.Name}' retornó null, pero no se puede enviar 'null' como respuesta al cliente.");
		// Enviamos la pagina.
		context.Response.ContentLength64 = buffer.Length;
		context.Response.OutputStream.Write(buffer, 0, buffer.Length);

	}

	/// <summary>
	/// Modify the header of the response to the customer.
	/// </summary>
	/// <param name="context">It is used to modify the response header.</param>
	/// <param name="attr">It obtains the information to modify the header.</param>
	private void MakeHeader(HttpListenerContext context, UrlInfo? attr=null){
		if (attr==null)
			return;
		foreach (KeyValuePair<string, string> heaDic in attr.header)
			context.Response.Headers.Add(heaDic.Key, heaDic.Value.ToString());
	}

	/// <summary>
	/// It obtains the parameters for the function from the client's GET request.
	/// </summary>
	/// <param name="context">Get the keys and values ​​passed by the URL.</param>
	/// <param name="func">
	/// It obtains the necessary information to order, view, and know about 
	/// the attributes of the function.
	/// </param>
	/// <returns>The values ​​used to pass as a parameter to the function.</returns>
	/// <exception cref="ArgumentException"><see cref="ArgumentException"/></exception>
	private string[] GetMethodParam(HttpListenerContext context, MethodInfo func){
		ParameterInfo[] pars = func.GetParameters();
		string[] paramValue=new string[pars.Length];
		string? value;
		for (uint i=0; i<pars.Length; i++){
			// Si hay un argumento que no sea string: Error.
			if (pars[i].ParameterType!=typeof(string))
				UrlController.ArgumentException(pars[i],func);

			value=context.Request.QueryString.Get(pars[i].Name);
			paramValue[pars[i].Position]=(value==null)?
				(string)((pars[i].HasDefaultValue)?pars[i].DefaultValue:"")
				:
				value;
		}
		return paramValue;
	}

	/// <summary>
	/// It obtains the parameters for the function from the client's POST request.
	/// </summary>
	/// <param name="context">Get the keys and values ​​passed by the URL.</param>
	/// <param name="func">
	/// It obtains the necessary information to order, view, 
	/// and know about the attributes of the function.
	/// </param>
	/// <returns>The values ​​used to pass as a parameter to the function.</returns>
	/// <exception cref="ArgumentException"><see cref="ArgumentException"/></exception>
	private string[] PostMethodParam(HttpListenerContext context, MethodInfo func){
		ParameterInfo[] pars = func.GetParameters();
		string[] paramValue=new string[pars.Length];
		bool[] paramFree=new bool[pars.Length];
		
		for (uint i=0; i<pars.Length;i++){
			// Check if all arguments are of a valid type.
			if (pars[i].ParameterType!=typeof(string))
				UrlController.ArgumentException(pars[i],func);
			paramFree[i]=false;
			paramValue[pars[i].Position]=(string)(
				(pars[i].HasDefaultValue)?
					pars[i].DefaultValue:""
			);
		}
		using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding)){
			string[] param = reader.ReadToEnd().Split('&');
			// Buscamos el parametro que se quiere dentro de la función.
			for (uint x=0;x<param.Length;x++){
				string[] keyValue=param[x].Split('=');
				for (uint y=0; y<pars.Length;y++)
					if (
						!paramFree[y] && 
						string.Equals(keyValue[0],pars[y].Name, StringComparison.OrdinalIgnoreCase)
					){
						paramValue[pars[y].Position]=keyValue[1];
						paramFree[y]=true;// We prevent the same chain from being verified.
					}
			}
		}
		return paramValue;
	}

	/// <summary>Returns the file dedicated to 404 responses.</summary>
	/// <returns>Response http 404</returns>
	public string PageNotFound(){
		if (context!=null)
			context.Response.StatusCode = 404;
		return StaticFile.GetString(StaticFile.path404codes);
	}
	/// <summary>To inform about what type of parameter should be used.</summary>
	/// <param name="pars">Information about the invalid parameter name and type.</param>
	/// <param name="func">Information about the class name and function.</param>
	/// <exception cref="ArgumentException">
	/// All parameters of functions with the UrlInfo attribute must be string
	/// </exception>
	static void ArgumentException(ParameterInfo pars,MethodInfo func){
		throw new ArgumentException($@"The argument '{pars.ParameterType} {pars.Name}' of the function '{func.ReflectedType}.{func.Name}' is not of type 'string'.
	Note: All functions with 'UrlInfo' attributes must accept only string parameters.");
	}
}