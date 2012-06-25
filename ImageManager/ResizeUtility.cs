using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ImageManager
{
	static class ResizeUtility
	{
		public static Bitmap Get(Image image, int width, int height, ImageMod imageMod, string hexBackgroundColour, AnchorPosition? anchor)
		{
			switch (imageMod)
			{
				case ImageMod.Scale:
					return Scale(image, width, height, hexBackgroundColour);
				case ImageMod.Crop:
					return Crop(image, width, height, anchor ?? AnchorPosition.Center);
				default:
					return Scale(image, width, height, hexBackgroundColour);
			}
		}
		public static Bitmap Get(Image image, int maxSideSize)
		{
			if (image.Width < maxSideSize & image.Height < maxSideSize)
				maxSideSize = image.Width > image.Height ? image.Width : image.Height;

			var scaleFactor = GetScaleFactor(image, maxSideSize);
			var width = Convert.ToInt32(scaleFactor * image.Width);
			var height = Convert.ToInt32(scaleFactor * image.Height);

			var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);

			bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			using (var graphics = Graphics.FromImage(bitmap))
			{
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.CompositingMode = CompositingMode.SourceCopy;

				var imageAttributes = new ImageAttributes();
				imageAttributes.SetWrapMode(WrapMode.TileFlipXY);

				var destRectangle = new Rectangle(0, 0, width, height);

				graphics.DrawImage(image, destRectangle, 0, 0, width, height, GraphicsUnit.Pixel, imageAttributes);
			}
			return bitmap;
		}
		public static Bitmap Get(Image image, int maxWidth, int maxHeight)
		{
			if (image.Width < maxWidth && image.Height < maxHeight)
			{
				maxWidth = image.Width;
				maxHeight = image.Height;
			}

			var widthScaleFactor = (float)maxWidth / image.Width;
			var heightScaleFactor = (float)maxHeight / image.Height;
			var scaleFactor = widthScaleFactor > heightScaleFactor ? heightScaleFactor : widthScaleFactor;
			var width = Convert.ToInt32(scaleFactor * image.Width);
			var height = Convert.ToInt32(scaleFactor * image.Height);

			var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);

			bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
			
			using (var graphics = Graphics.FromImage(bitmap))
			{
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.CompositingMode = CompositingMode.SourceCopy;

				var imageAttributes = new ImageAttributes();
				imageAttributes.SetWrapMode(WrapMode.TileFlipXY);

				var destRectangle = new Rectangle(0, 0, width, height);

				graphics.DrawImage(image, destRectangle, 0, 0, width, height, GraphicsUnit.Pixel, imageAttributes);
			}
			return bitmap;
		}

		public static Bitmap GetAndCrop(Image image, int targetWidth, int targetHeight, double widthRatio, double heightRatio, double leftRatio, double topRatio)
		{
			var bitmap = new Bitmap(targetWidth, targetHeight, PixelFormat.Format24bppRgb);
			bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			using (var graphics = Graphics.FromImage(bitmap))
			{
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.CompositingMode = CompositingMode.SourceCopy;

				graphics.DrawImage(image,
					new Rectangle(0, 0, targetWidth, targetHeight),
					new Rectangle(
						Convert.ToInt32(leftRatio * image.Width),
						Convert.ToInt32(topRatio * image.Height),
						Convert.ToInt32(widthRatio * image.Width),
						Convert.ToInt32(heightRatio * image.Height)),
					GraphicsUnit.Pixel);
			}
			return bitmap;
		}

		public static float GetScaleFactor(Image image, float maxDimension)
		{
			float scaleFactor;
			if (image.Width > image.Height)
			{
				scaleFactor = maxDimension / image.Width;
			}
			else
			{
				scaleFactor = maxDimension / image.Height;
			}
			return scaleFactor;
		}


		private static Rectangle GetDestinationRectangle(int width, int height, int sourceWidth, int sourceHeight)
		{
			var destX = 0;
			var destY = 0;

			float finalScalePercent;
			var widthPercent = (width / (float)sourceWidth);
			var heightPercent = (height / (float)sourceHeight);

			if (heightPercent < widthPercent)
			{
				destX = Convert.ToInt16((width - (sourceWidth * heightPercent)) / 2);
				finalScalePercent = heightPercent;
			}
			else
			{
				destY = Convert.ToInt16((height - (sourceHeight * widthPercent)) / 2);
				finalScalePercent = widthPercent;
			}

			var destWidth = (int)(sourceWidth * finalScalePercent);
			var destHeight = (int)(sourceHeight * finalScalePercent);

			return new Rectangle(destX - 1, destY - 1, destWidth + 2, destHeight + 2);
		}

		private static Color GetColour(string hexColour)
		{
			if (string.IsNullOrEmpty(hexColour) || hexColour.Length != 6)
				throw new ArgumentException("The string supplied should be in the hexidecimal colour format: e.g. 'AABB22' ");

			var red = int.Parse(hexColour.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
			var green = int.Parse(hexColour.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
			var blue = int.Parse(hexColour.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
			return Color.FromArgb(red, green, blue);
		}

		private static Bitmap Crop(Image image, int width, int height, AnchorPosition Anchor)
		{
			var sourceWidth = image.Width;
			var sourceHeight = image.Height;
			var sourceX = 0;
			var sourceY = 0;
			var destX = 0;
			var destY = 0;

			float nPercent;
			float nPercentW;
			float nPercentH;

			nPercentW = (width / (float)sourceWidth);
			nPercentH = (height / (float)sourceHeight);

			if (nPercentH < nPercentW)
			{
				nPercent = nPercentW;
				switch (Anchor)
				{
					case AnchorPosition.Top:
						destY = 0;
						break;
					case AnchorPosition.Bottom:
						destY = (int)(height - Math.Round(sourceHeight * nPercent));
						break;
					default:
						destY = (int)((height - Math.Round(sourceHeight * nPercent)) / 2);
						break;
				}
			}
			else
			{
				nPercent = nPercentH;
				switch (Anchor)
				{
					case AnchorPosition.Left:
						destX = 0;
						break;
					case AnchorPosition.Right:
						destX = (int)(width - Math.Round(sourceWidth * nPercent));
						break;
					default:
						destX = (int)((width - Math.Round(sourceWidth * nPercent)) / 2);
						break;
				}
			}

			var destWidth = (int)Math.Round(sourceWidth * nPercent);
			var destHeight = (int)Math.Round(sourceHeight * nPercent);

			var bmPhoto = new Bitmap(width, height, PixelFormat.Format24bppRgb);
			bmPhoto.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			using (var graphics = Graphics.FromImage(bmPhoto))
			{
				graphics.Clear(Utilities.BackgroundColour);
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.CompositingMode = CompositingMode.SourceCopy;

				var imageAttributes = new ImageAttributes();
				imageAttributes.SetWrapMode(WrapMode.TileFlipXY);

				graphics.DrawImage(image,
				                   new Rectangle(destX, destY, destWidth, destHeight),
				                   sourceX, sourceY, sourceWidth, sourceHeight,
				                   GraphicsUnit.Pixel, imageAttributes);
			}
			return bmPhoto;
		}

		private static Bitmap Scale(Image sourcePhoto, int width, int height, string hexBackgroundColour)
		{
			var destinationRectangle = GetDestinationRectangle(width, height, sourcePhoto.Width, sourcePhoto.Height);

			var bitmap = new Bitmap(width, height, PixelFormat.Format32bppRgb);
			bitmap.SetResolution(sourcePhoto.HorizontalResolution, sourcePhoto.VerticalResolution);
			bitmap.MakeTransparent();

			using (var graphics = Graphics.FromImage(bitmap))
			{
				var backgroundColour = Utilities.BackgroundColour;
				if (!string.IsNullOrEmpty(hexBackgroundColour))
				{
					backgroundColour = GetColour(hexBackgroundColour);
				}

				graphics.Clear(backgroundColour);

				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.CompositingMode = CompositingMode.SourceCopy;

				var imageAttributes = new ImageAttributes();
				imageAttributes.SetWrapMode(WrapMode.TileFlipXY);

				graphics.DrawImage(sourcePhoto, destinationRectangle, 0, 0, sourcePhoto.Width, sourcePhoto.Height,
				                   GraphicsUnit.Pixel, imageAttributes);
			}
			return bitmap;
		}
	}
}
