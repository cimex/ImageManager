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
		private readonly IFileService _fileService;

		public ImageService(IFileService fileService, HttpContext context)
		{
			Context = context;
			_fileService = fileService;
		}

		public bool SaveForWeb(string sourceFileName, string relativeSourcePath, string relativeTargetPath)
		{
			var tempFileStream = _fileService.GetTempFile(relativeSourcePath + sourceFileName);
			if (tempFileStream == null)
				return false;

			using (var image = Image.FromStream(tempFileStream))
			{
				var scaleFactor = ResizeUtility.GetScaleFactor(image, Configs.MaxImageDimension);

				var defaultWidth = scaleFactor < 1 ? (int) (image.Width*scaleFactor) : image.Width;
				var defaultHeight = scaleFactor < 1 ? (int) (image.Height*scaleFactor) : image.Height;

				using (var thumbnailImage = CreateThumbnail(image, defaultWidth, defaultHeight))
				{
					var targetFilePath = relativeTargetPath + sourceFileName;

					_fileService.DeleteTempFile(targetFilePath);

					using (var thumbnailStream = new MemoryStream())
					{
						thumbnailImage.Save(thumbnailStream, ImageFormat.Png);
						thumbnailStream.Position = 0;
						_fileService.SaveFile(targetFilePath, thumbnailStream);
					}
				}
			}
			return true;
		}

		private static Bitmap CreateThumbnail(Image image, int defaultWidth, int defaultHeight)
		{
			var thumbBmp = new Bitmap(defaultWidth, defaultHeight);
			thumbBmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			using (var graphics = Graphics.FromImage(thumbBmp))
			{
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
				graphics.CompositingMode = CompositingMode.SourceCopy;

				var imageAttributes = new ImageAttributes();
				imageAttributes.SetWrapMode(WrapMode.TileFlipXY);

				var destRectangle = new Rectangle(0, 0, defaultWidth, defaultHeight);
				graphics.DrawImage(image, destRectangle, 0, 0, image.Width, image.Height,
				                   GraphicsUnit.Pixel, imageAttributes);
			}
			return thumbBmp;
		}

		public byte[] Get(string relativeFilePath, int width, int height, ImageMod imageMod, string hexBackgroundColour, AnchorPosition? anchor, OutputFormat outputFormat)
		{
			using (var bitmap = Get(relativeFilePath, width, height, imageMod, hexBackgroundColour, anchor))
			{
				return bitmap.GetBytes(outputFormat);
			}
		}
		public Bitmap Get(string relativeFilePath, int width, int height, ImageMod imageMod, string hexBackgroundColour, AnchorPosition? anchor)
		{
			using (var image = (relativeFilePath == "Default"
				? getDefault(width, height)
				: loadImage(relativeFilePath) ?? getDefault(width, height)))
			{
				return ResizeUtility.Get(image, width, height, imageMod, hexBackgroundColour, anchor);
			}
		}
		public Stream Get(Stream file, int width, int height, ImageMod imageMod, string hexBackgroundColour, AnchorPosition? anchor, OutputFormat outputFormat)
		{
			return ResizeUtility.Get(loadImage(file) ?? getDefault(width, height), width, height, imageMod, hexBackgroundColour, anchor).GetStream(outputFormat);
		}

		public byte[] Get(string relativeFilePath, int maxSideSize, OutputFormat outputFormat)
		{
			using (var bitmap = Get(relativeFilePath, maxSideSize))
			{
				return bitmap.GetBytes(outputFormat);
			}
		}
		public Bitmap Get(string relativeFilePath, int maxSideSize)
		{
			var image = (relativeFilePath == "Default"
				? getDefault(maxSideSize, maxSideSize)
				: loadImage(relativeFilePath)) ?? getDefault(maxSideSize, maxSideSize);

			return ResizeUtility.Get(image, maxSideSize);
		}
		public Stream Get(Stream file, int maxSideSize, OutputFormat outputFormat)
		{
			return ResizeUtility.Get(loadImage(file) ?? getDefault(maxSideSize, maxSideSize), maxSideSize).GetStream(outputFormat);
		}

		public byte[] Get(string relativeFilePath, int maxWidth, int maxHeight, OutputFormat outputFormat)
		{
			using (var bitmap = Get(relativeFilePath, maxWidth, maxHeight))
			{
				return bitmap.GetBytes(outputFormat);
			}
		}
		public Bitmap Get(string relativeFilePath, int maxWidth, int maxHeight)
		{
			var image = (relativeFilePath == "Default" ? getDefault(maxWidth, maxHeight) :
				loadImage(relativeFilePath)) ?? getDefault(maxWidth, maxHeight);

			return ResizeUtility.Get(image, maxWidth, maxHeight);
		}
		public Stream Get(Stream file, int maxWidth, int maxHeight, OutputFormat outputFormat)
		{
			return ResizeUtility.Get(Image.FromStream(file), maxWidth, maxHeight).GetStream(outputFormat);
		}

		public byte[] GetCached(string relativeFilePath, int width, int height, ImageMod imageMod, string hexBackgroundColour, AnchorPosition? anchor, OutputFormat outputFormat)
		{
			var key = string.Format("ImageManager-{0}-{1}-{2}-{3}-{4}", relativeFilePath, width, height, imageMod, outputFormat);
			if (Context.Cache[key] == null)
			{
				var image = Get(relativeFilePath, width, height, imageMod, hexBackgroundColour, anchor, outputFormat);
				if (image == null) throw new FileNotFoundException("The image requested does not exist.");
				Context.Cache.Insert(key, image, null, Cache.NoAbsoluteExpiration, Configs.CacheExpiration);
			}
			return (byte[])Context.Cache[key];
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
		public byte[] GetAndCrop(string relativeFilePath, int targetWidth, int targetHeight, double widthRatio, double heightRatio, double leftRatio, double topRatio, OutputFormat outputFormat)
		{
			return GetAndCrop(relativeFilePath, targetWidth, targetHeight, widthRatio, heightRatio, leftRatio, topRatio).GetBytes(outputFormat);
		}
		///<summary> 
		/// This returns a specified crop
		/// </summary>
		/// <param name="relativeFilePath">e.g. asdf/asdf.jpg</param>
		public Bitmap GetAndCrop(string relativeFilePath, int targetWidth, int targetHeight, double widthRatio, double heightRatio, double leftRatio, double topRatio)
		{
			var sourceImage = loadImage(relativeFilePath);

			if (sourceImage == null)
				return getDefault(targetWidth, targetHeight);

			return ResizeUtility.GetAndCrop(sourceImage, targetHeight, targetHeight, widthRatio, heightRatio, leftRatio, topRatio);
		}
		///<summary> 
		/// This returns a specified crop
		/// </summary>
		public Stream GetAndCrop(Stream file, int targetWidth, int targetHeight, double widthRatio, double heightRatio, double leftRatio, double topRatio, OutputFormat outputFormat)
		{
			return ResizeUtility.GetAndCrop(Image.FromStream(file), targetWidth, targetHeight, widthRatio, heightRatio, leftRatio, topRatio).GetStream(outputFormat);
		}


		private Image loadImage(string relativeFilePath)
		{
			var stream = _fileService.GetFile(relativeFilePath);
			return stream != null ? Image.FromStream(stream) : null;
		}

		private Image loadImage(Stream file)
		{
			file.Position = 0;
			return Image.FromStream(file);
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

	public enum OutputFormat
	{
		Png = 0,
		Jpeg = 1,
		Gif = 2,
		HighQualityJpeg = 3
	}
}