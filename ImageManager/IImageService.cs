using System.Drawing;
using System.IO;

namespace ImageManager
{
	public interface IImageService
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceFileName">name.jpg</param>
		/// <param name="relativeSourcePath">~/folder/</param>
		/// <param name="relativeTargetPath">e.g. "~/folder/"</param>
		/// <returns></returns>
		bool SaveForWeb(string sourceFileName, string relativeSourcePath, string relativeTargetPath);

		Bitmap Get(string relativeFilePath, int width, int height, ImageMod imageMod, string hexBackgroundColour, AnchorPosition? anchor);

		byte[] Get(string relativeFilePath, int width, int height, ImageMod imageMod, string hexBackgroundColour, AnchorPosition? anchor, OutputFormat outputFormat);
		Stream Get(Stream file, int width, int height, ImageMod imageMod, string hexBackgroundColour, AnchorPosition? anchor, OutputFormat outputFormat);

		byte[] GetCached(string relativeFilePath, int width, int height, ImageMod imageMod, string hexBackgroundColour, AnchorPosition? anchor, OutputFormat outputFormat);

		void Delete(string fullFilePath);

		Bitmap GetAndCrop(string relativeFilePath, int targetWidth, int targetHeight, double widthRatio, double heightRatio, double leftRatio, double topRatio);

		byte[] GetAndCrop(string relativeFilePath, int targetWidth, int targetHeight, double widthRatio, double heightRatio, double leftRatio, double topRatio, OutputFormat outputFormat);
		Stream GetAndCrop(Stream file, int targetWidth, int targetHeight, double widthRatio, double heightRatio, double leftRatio, double topRatio, OutputFormat outputFormat);

		Bitmap Get(string relativeFilePath, int maxSideSize);

		byte[] Get(string relativeFilePath, int maxSideSize, OutputFormat outputFormat);
		Stream Get(Stream stream, int maxSideSize, OutputFormat outputFormat);

		byte[] Get(string relativeFilePath, int maxWidth, int maxHeight, OutputFormat outputFormat);
		Stream Get(Stream stream, int maxWidth, int maxHeight, OutputFormat outputFormat);
	}
}