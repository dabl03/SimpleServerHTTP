# Simple Server for HTTP

Este proyecto fue creado para facilitar la elaboración rápida de un servidor en local.

## Requisito
- dotnet: Este proyecto se compila usando la versión 8.0.401 de dotnet.

## Compilación y Ejecución
Solo se necesitan <code>dotnet build</code> y <code>dotnet run</code> para ejecutar el ejemplo.

## Empezar a desarrollar
La forma de crear una url se basa en flask de python por su facilidad, pero las configuraciones son diferentes:
<pre><code>
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
	}
</code></pre>
Como se muestra en el código podemos analizar lo siguiente:
- Todas las funciones con atributos UrlInfo serán llamadas por las url correspondientes.
- Por ahora solo se permiten dos tipos de retornos: byte[] y string, esto porque al final la respuesta se envía en byte[], y el string se convierte en byte[].
- Las configuraciones del header por ahora son solo en string con formato JSON, y son parámetros opcionales.
- Los parámetros de las funciones se toman de la url, si no se consiguen entonces se pasa "" .
- El retorno será la respuesta al cliente.

## todo
[-] Mejorar la documentación y buscar la manera de generarla automáticamente.
[-] Mejorar el ejemplo para que enseñe esta documentación.
[-] Traducir todo al ingles.
[-] Crear un sistema de plantilla.
[-] Facilitar la respuesta html haciendo que automáticamente se cree el header.
[-] Arreglar bug de la codificacion en la respuesta 404
[-] El servidor se sigue ejecutando en MobaXterm al precionar ctrl+c