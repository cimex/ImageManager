using System;
using System.Drawing;
using System.IO;

namespace ImageManagerTest
{
	public class ImageService : IImageService
	{
		//TODO: MOVE TO CONFIG
		private string targetDirectory = Environment.CurrentDirectory + @"\Images\";
		private float maxDimension;

		public bool Save(string fileName, string absoluteFilePath, string contentType)
		{
			var filePath = absoluteFilePath + fileName;
			var image = Image.FromFile(filePath);
			maxDimension = 600;
			var scaleFactor = getScaleFactor(image, maxDimension);

			var defaultWidth = (int)(image.Width * scaleFactor);
			var defaultHeight = (int)(image.Height * scaleFactor);

			var specificContentTypeFilePath = targetDirectory + contentType + "\\";
			if(!Directory.Exists(specificContentTypeFilePath))
			{
				Directory.CreateDirectory(specificContentTypeFilePath);
			}

			var thumbnailImage = image.GetThumbnailImage(defaultWidth, defaultHeight, thumbnailCallback, IntPtr.Zero);

			if (File.Exists(targetDirectory + fileName))
			{
				File.Delete(targetDirectory + fileName);
			}

			thumbnailImage.Save(specificContentTypeFilePath + fileName, System.Drawing.Imaging.ImageFormat.Png);
			
			image.Dispose();
			thumbnailImage.Dispose();
			return true;
		}


		public Image Get(string fileName, string contentType, int width, int height)
		{
			var image = Image.FromFile(targetDirectory + contentType + "\\" + fileName);
			var thumbnailImage = image.GetThumbnailImage(width, height, thumbnailCallback, IntPtr.Zero);
			return thumbnailImage;
		}


		private float getScaleFactor(Image image, float maxDimension)
		{
			float scaleFactor;
			if(image.Width > image.Height)
			{
				scaleFactor = maxDimension/image.Width;
			}else{
				scaleFactor = maxDimension/image.Height;
			}
			return scaleFactor;
		}

		private bool thumbnailCallback()
		{
			return true;
		}
	}
}