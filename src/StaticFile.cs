using System.IO;
namespace SimpleServerHTTP{
	/// <sumary>
	/// This class is designed to read and return
	/// the contents of the file if it is found;
	/// otherwise, the corresponding error file is returned.
	/// </sumary>
	public static class StaticFile{
		public static string RootPath="./"; /// Root folder where files are searched.
		public static string Path404Codes="/codes/404.html";/// Where to find the 404 error page
		public static string Path404CodesImage="/media/imagen_not_found.png"; /// Where to find the 404 error image

		/// <summary>
		/// Read a file and return its contents as a string.
		/// </summary>
		/// <param name="url">The file search.</param>
		/// <return>
		/// The contents of the file or the contents
		/// of the file RootPath/Path404Codes.
		/// </return>
		public static string GetString(string url){
			string path=$"{RootPath}/{url}";
			try{
				return File.ReadAllText(path);
			}catch(IOException e){
				Console.WriteLine($"{e.Message}\nError reading file: \"{path}\".");
				path=$"{RootPath}/{Path404Codes}";
				if (File.Exists(path)){
					return StaticFile.GetString(path);
				}
			}
			return "<h1>Error 404: Page not available.</h1>";
		}

		/// <summary>Returns an image file as a byte array</summary>
		/// <param name="url">The image for search.</param>
		/// <return>The image or the file Path404CodesImage</return>
		public static byte[] GetImage(string url){
			// For binary files.
			byte[] image=GetBinary(url);
			return (image!=null)?image:GetBinary(Path404CodesImage);
		}

		/// <summary>Reads and returns a binary file.</summary>
		/// <param name="url">The file search.</param>
		/// <return>
		/// The contents of the file or the contents or
		///  null if it doesn't exist.
		/// </return>
		/// <exception cref="ArgumentException">The URL is invalid.</exception>
		public static byte[] GetBinary(string url){
			string PATH=$"{RootPath}/{url}";

			if (!UrlInfo.VerifyUrl(url))
				throw new ArgumentException("The URL is invalid", nameof(url));
			if (!File.Exists(PATH)){
				Console.WriteLine($"The file '{PATH}' does not exist.");
				return null;
			}
			byte[] bytes;
			using (FileStream fs = new FileStream(PATH, FileMode.Open, FileAccess.Read)){
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
}