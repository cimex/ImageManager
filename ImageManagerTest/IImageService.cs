using System.Drawing;

namespace ImageManagerTest
{
	public interface IImageService
	{
		bool Save(string fileName, string absoluteFilePath, string destinationFolderName);
		Image Get(string fileName, string contentType, int width, int height);
	}
}