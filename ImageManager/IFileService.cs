using System.IO;

namespace ImageManager
{
	public interface IFileService
	{
		Stream GetFile(string path);
		Stream GetTempFile(string path);
		void SaveFile(string path, Stream stream);
		void DeleteFile(string path);
		void DeleteTempFile(string path);
	}
}
