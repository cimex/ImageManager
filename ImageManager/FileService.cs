using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace ImageManager
{
	public class FileService : IFileService
	{
		private readonly HttpContext _context;

		public FileService(HttpContext context)
		{
			_context = context;
		}

		public Stream GetFile(string path)
		{
			var fullPath = _context.Server.MapPath(path);
			return File.Exists(fullPath) ? new FileStream(fullPath, FileMode.Open) : null;
		}

		public Stream GetTempFile(string path)
		{
			return GetFile(path);
		}

		public void DeleteFile(string path)
		{
			if (File.Exists(path))
				File.Delete(path);
		}

		public void DeleteTempFile(string path)
		{
			DeleteFile(path);
		}

		public void SaveFile(string path, Stream stream)
		{
			var len = (int) stream.Length;
			var bytes = new byte[len];
			stream.Read(bytes, 0, len);
			var fileStream = new FileStream(path, FileMode.OpenOrCreate);
			fileStream.Write(bytes, 0, len);
		}
	}
}
