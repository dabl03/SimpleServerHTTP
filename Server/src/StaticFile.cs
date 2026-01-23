using System.IO;

class StaticFile{
	public const string path404codes="codes/404.html";
	/// <summary>Lee un archivo y retorna su contenido como string.</summary>
	public static string GetString(string url){
		try{
			return File.ReadAllText($"./static/{url}");
		}catch(IOException e){
			Console.WriteLine($"{e.Message}\nError al leer el archivo \"./static/{url}\".");
			if (File.Exists(path404codes)){
				return StaticFile.GetString(path404codes);
			}
		}
		return "<h1>Error 404: Pagina no disponible.</h1>";
	}
	public static byte[] GetImage(string url){
		// Para los archivos binarios.
		byte[] image=GetBinary(url);
		url="./static/media/imagen_not_found.png";
		return (image!=null)?image:GetBinary(url);
	}
	public static byte[] GetBinary(string url){
		if (!File.Exists(url)){
			Console.WriteLine($"El archivo '{url}' no existe.");
			return null;
		}
		byte[] bytes;
		using (FileStream fs = new FileStream(url, FileMode.Open, FileAccess.Read)){
			bytes = new byte[fs.Length];
            int numBytesRead = 0;
            int n=0;
            // Por si no lee el archivo completo.
            for (int i=(int)fs.Length; i > 0; i-=n) {
                n = fs.Read(bytes, numBytesRead, i);
                if (n == 0)
                    break;
                numBytesRead += n;
            }
			return bytes;
		}
	}
}