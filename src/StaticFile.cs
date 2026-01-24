using System.IO;
/// <sumary>
/// This class is designed to read and return
/// the contents of the file if it is found;
/// otherwise, the corresponding error file is returned.
/// </sumary>
class StaticFile{
	public const string path404codes="codes/404.html";
	public const string path404codesImage="./static/media/imagen_not_found.png";
	/// <summary>
	/// Read a file and return its contents as a string.
	/// </summary>
	/// <param name="url">The file search.</param>
	/// <return>
	/// The contents of the file or the contents
	/// of the file path404codes.
	/// </return>
	public static string GetString(string url){
		try{
			return File.ReadAllText($"./static/{url}");
		}catch(IOException e){
			Console.WriteLine($"{e.Message}\nError reading file: \"./static/{url}\".");
			if (File.Exists(path404codes)){
				return StaticFile.GetString(path404codes);
			}
		}
		return "<h1>Error 404: Page not available.</h1>";
	}

	/// <summary>Reads and returns a binary file.</summary>
	/// <param name="url">The file search.</param>
	/// <return>
	/// The contents of the file or the contents
	/// of the file path404codesImage.
	/// </return>
	public static byte[] GetImage(string url){
		// For binary files.
		byte[] image=GetBinary(url);
		return (image!=null)?image:GetBinary(path404codesImage);
	}
	public static byte[] GetBinary(string url){
		if (!File.Exists(url)){
			Console.WriteLine($"The file '{url}' does not exist.");
			return null;
		}
		byte[] bytes;
		using (FileStream fs = new FileStream(url, FileMode.Open, FileAccess.Read)){
			bytes = new byte[fs.Length];
            int numBytesRead = 0;
            int n=0;
            // In case you don't read the entire file.
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