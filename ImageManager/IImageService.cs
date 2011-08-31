using System.Drawing;

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
		
		Bitmap GetCached(string relativeFilePath, int width, int height, ImageMod imageMod, string hexBackgroundColour, AnchorPosition? anchor);

		void Delete(string fullFilePath);

		Bitmap GetAndCrop(string relativeFilePath, int targetWidth, int targetHeight, double widthRatio, double heightRatio,
		                  double leftRatio, double topRatio);

		Bitmap Get(string relativeFilePath, int maxSideSize);
	}
}