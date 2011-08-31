using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using System.Web.Caching;

namespace ImageManager
{
	public class ImageService : IImageService
	{
		public HttpContext Context { get; set; }

		public ImageService(HttpContext context)
		{
			Context = context;
		}

		public bool SaveForWeb(string sourceFileName, string relativeSourcePath, string relativeTargetPath)
		{
			var sourceFilePath = Context.Server.MapPath(relativeSourcePath + sourceFileName);
			var image = Image.FromFile(sourceFilePath);
			var scaleFactor = getScaleFactor(image, Configs.MaxImageDimension);

			var defaultWidth = scaleFactor < 1 ? (int) (image.Width*scaleFactor) : image.Width;
			var defaultHeight = scaleFactor < 1 ? (int) (image.Height*scaleFactor) : image.Height;
			
			var thumbnailImage  = createThumbnail(image, defaultWidth, defaultHeight);
			var targetFilePath = Context.Server.MapPath(relativeTargetPath + sourceFileName);

			if (File.Exists(targetFilePath))
			{
				File.Delete(targetFilePath);
			}

			thumbnailImage.Save(targetFilePath, ImageFormat.Png);
			
			image.Dispose();
			thumbnailImage.Dispose();
			return true;
		}

		private Bitmap createThumbnail(Image image, int defaultWidth, int defaultHeight)
		{
			var thumbBmp = new Bitmap(defaultWidth, defaultHeight);
			thumbBmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);
			var graphics = Graphics.FromImage(thumbBmp);
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

			var imageAttributes = new ImageAttributes();
			imageAttributes.SetWrapMode(WrapMode.TileFlipXY);

			var destRectangle = new Rectangle(0, 0, defaultWidth, defaultHeight);
			graphics.DrawImage(image, destRectangle, 0, 0, image.Width, image.Height,
				GraphicsUnit.Pixel, imageAttributes);
			return thumbBmp;
		}

		public Bitmap Get(string relativeFilePath, int width, int height, ImageMod imageMod, string hexBackgroundColour, AnchorPosition? anchor)
		{
			Image image;
			image = (relativeFilePath == "Default" ? getDefault(width, height) : loadImage(relativeFilePath)) ?? getDefault(width, height);
			if (image == null) return getDefault(width, height);

			switch (imageMod)
			{
				case ImageMod.Scale: return scale(image, width, height, hexBackgroundColour);
				case ImageMod.Crop: return crop(image, width, height, anchor ?? AnchorPosition.Center);
				default: return scale(image, width, height, hexBackgroundColour);
			}
		}

		public Bitmap Get(string relativeFilePath, int maxSideSize)
		{
			Func<int, Image> defaultImage = maxSize => getDefault(maxSize, maxSize);
			
			var image = (relativeFilePath == "Default" ? defaultImage(maxSideSize) : 
				loadImage(relativeFilePath)) ?? defaultImage(maxSideSize);

			if(image.Width < maxSideSize & image.Height < maxSideSize)
			{
				maxSideSize = image.Width > image.Height ? image.Width : image.Height;
			}

			var scaleFactor = getScaleFactor(image, maxSideSize);
			var width = Convert.ToInt32(scaleFactor*image.Width);
			var height = Convert.ToInt32(scaleFactor*image.Height);

			var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);

			bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
			var graphics = Graphics.FromImage(bitmap);
			
			graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			graphics.CompositingQuality = CompositingQuality.HighQuality;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.CompositingMode = CompositingMode.SourceCopy;

			graphics.DrawImage(image,0, 0, width, height);

			graphics.Dispose();
			return bitmap;
		}

		private Image loadImage(string relativeFilePath)
		{
			var physicalPath = Context.Server.MapPath(relativeFilePath);
			return !File.Exists(physicalPath) ? null : Image.FromFile(physicalPath);
		}

		private Bitmap getDefault(int width, int height)
	{
		var defaultImage = new Bitmap(width, height);
		using (var g = Graphics.FromImage(defaultImage))
		{
            g.Clear(Color.Gray);
		}
		return defaultImage;
	}

		public Bitmap GetCached(string relativeFilePath, int width, int height, ImageMod imageMod, string hexBackgroundColour, AnchorPosition? anchor)
		{
			var key = "ImageManager-"+ relativeFilePath + width + height + Enum.GetName(typeof(ImageMod), imageMod);
			if(Context.Cache[key] == null)
			{
				var image = Get(relativeFilePath, width, height, imageMod, hexBackgroundColour, anchor);
				if(image == null) throw new FileNotFoundException("The image requested does not exist.");
				Context.Cache.Insert(key, image, null, Cache.NoAbsoluteExpiration, Configs.CacheExpiration);
			}
			return Context.Cache[key] as Bitmap;
		}

		public void Delete(string fullFilePath)
		{
			if (File.Exists(fullFilePath))
			{
				File.Delete(fullFilePath);
			}
		}

		///<summary> 
		/// This returns a specified crop
		/// </summary>
		/// <param name="relativeFilePath">e.g. asdf/asdf.jpg</param>
		public Bitmap GetAndCrop(string relativeFilePath, int targetWidth, int targetHeight, double widthRatio, double heightRatio, double leftRatio, double topRatio)
		{
			var sourceImage = loadImage(relativeFilePath);

			if (sourceImage == null) return getDefault(targetWidth, targetHeight);

			//target
			var bitmap = new Bitmap(targetWidth, targetHeight, PixelFormat.Format24bppRgb);
			bitmap.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);

			var graphics = Graphics.FromImage(bitmap);

			graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			graphics.CompositingQuality = CompositingQuality.HighQuality;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.CompositingMode = CompositingMode.SourceCopy;

			graphics.DrawImage(sourceImage,
				
				new Rectangle(0, 0,
					targetWidth, 
					targetHeight)
					,
				new Rectangle(
					Convert.ToInt32(leftRatio * sourceImage.Width),
					Convert.ToInt32(topRatio * sourceImage.Height), 
					Convert.ToInt32(widthRatio * sourceImage.Width),
					Convert.ToInt32(heightRatio * sourceImage.Height)),

				GraphicsUnit.Pixel);

			graphics.Dispose();
			return bitmap;
		}


		private Bitmap crop(Image image, int width, int height, AnchorPosition Anchor)
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
						destY = (int)
							(height - (sourceHeight * nPercent));
						break;
					default:
						destY = (int)
							((height - (sourceHeight * nPercent)) / 2);
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
						destX = (int)
						  (width - (sourceWidth * nPercent));
						break;
					default:
						destX = (int)
						  ((width - (sourceWidth * nPercent)) / 2);
						break;
				}
			}

			var destWidth = (int)(sourceWidth * nPercent);
			var destHeight = (int)(sourceHeight * nPercent);

			var bmPhoto = new Bitmap(width, height, PixelFormat.Format24bppRgb);
			bmPhoto.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			var grPhoto = Graphics.FromImage(bmPhoto);

			grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;
			grPhoto.CompositingQuality = CompositingQuality.HighQuality;
			grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
			grPhoto.CompositingMode = CompositingMode.SourceCopy;


			grPhoto.DrawImage(image,
				new Rectangle(destX, destY, destWidth, destHeight),
				new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
				GraphicsUnit.Pixel);

			grPhoto.Dispose();
			return bmPhoto;
		}

		private Bitmap scale(Image sourcePhoto, int Width, int Height, string hexBackgroundColour)
		{
			var destinationRectangle = GetDestinationRectangle(Width, Height, sourcePhoto.Width, sourcePhoto.Height);

			var bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppRgb);
			bitmap.SetResolution(sourcePhoto.HorizontalResolution, sourcePhoto.VerticalResolution);

			var grPhoto = Graphics.FromImage(bitmap);

			var backgroundColour = Color.Fuchsia;
			if(!string.IsNullOrEmpty(hexBackgroundColour))
			{
				backgroundColour = getColour(hexBackgroundColour);
			}

			grPhoto.Clear(backgroundColour);

			grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;
			grPhoto.CompositingQuality = CompositingQuality.HighQuality;
			grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

			var imageAttributes = new ImageAttributes();
			imageAttributes.SetWrapMode(WrapMode.TileFlipXY);
			

			grPhoto.DrawImage(sourcePhoto, destinationRectangle, 0, 0, sourcePhoto.Width, sourcePhoto.Height, 
				GraphicsUnit.Pixel, imageAttributes);

			grPhoto.Dispose();
			return bitmap;
		}

		public Rectangle GetDestinationRectangle(int width, int height, int sourceWidth, int sourceHeight)
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

			return new Rectangle(destX, destY, destWidth, destHeight);
		}

		private Color getColour(string hexColour)
		{
			if(string.IsNullOrEmpty(hexColour) || hexColour.Length != 6) 
				throw new ArgumentException("The string supplied should be in the hexidecimal colour format: e.g. 'AABB22' ");
			
			var red = int.Parse(hexColour.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
			var green = int.Parse(hexColour.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
			var blue = int.Parse(hexColour.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
			return Color.FromArgb(red, green, blue);
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

	public enum AnchorPosition
	{
		Center,
		Top,
		Bottom,
		Left,
		Right
	}

	public enum ImageMod
	{
		Raw = 0,
		Scale = 1,
		Crop = 3,
		SpecifiedCrop = 4
	}
}