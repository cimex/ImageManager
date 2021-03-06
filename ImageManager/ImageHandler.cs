using System;
using System.Collections.Specialized;
using System.Web;

namespace ImageManager
{
	public class ImageHandler : IHttpHandler
	{
		private IImageService _imageService;

		public virtual void ProcessRequest(HttpContext context)
		{
			this.context = context;
			_imageService = new ImageService(new FileService(context), context);


			setContentType();
			setClientCaching();
			
			switch (imageMod)
			{
				case ImageMod.Raw:
					context.Response.BinaryWrite(string.IsNullOrEmpty(QueryString["MaxSize"])
													? _imageService.Get(fileName, maxWidth, maxHeight, outputFormat)
													: _imageService.Get(fileName, maxSize, outputFormat));
					break;
				case ImageMod.SpecifiedCrop:
					context.Response.BinaryWrite(_imageService.GetAndCrop(fileName, width, height, widthRatio, heightRatio, leftRatio, topRatio, outputFormat));
					break;
				default:
					context.Response.BinaryWrite(cacheEnabled
					                             	? _imageService.GetCached(fileName, width, height, imageMod, hexBackGroundColour, anchor, outputFormat)
					                             	: _imageService.Get(fileName, width, height, imageMod, hexBackGroundColour, anchor, outputFormat));
					break;
			}
		}

		protected void setClientCaching()
		{
			var cacheType = disableClientCache ? HttpCacheability.NoCache : HttpCacheability.Public;
			context.Response.Cache.SetCacheability(cacheType);
			context.Response.Cache.SetExpires(DateTime.Now.AddMinutes(5));
		}

		protected void setContentType()
		{
			context.Response.ContentType = Utilities.GetContentType(outputFormat);
		}

		protected bool cacheEnabled
		{
			get { return parseBoolKeyValue("CacheEnabled"); }
		}

		protected bool disableClientCache
		{
			get
			{
				return parseBoolKeyValue("disableClientCache");
			}
		}
		protected ImageMod imageMod
		{
			get
			{
				var value = parseStringKeyValue("ImageMod", false);
				if (string.IsNullOrEmpty(value)) return ImageMod.Scale;
				return (ImageMod)Enum.Parse(typeof(ImageMod), value);
			}
		}

		protected AnchorPosition? anchor
		{
			get
			{
				var value = parseStringKeyValue("anchor", false);
				if (string.IsNullOrEmpty(value)) return AnchorPosition.Center;
				return (AnchorPosition)Enum.Parse(typeof(AnchorPosition), value);
			}
		}

		protected string hexBackGroundColour
		{
			get { return parseStringKeyValue("backgroundColour", false); }
		}

		protected int height
		{
			get { return parseIntegerKeyValue("Height"); }
		}
		protected int width
		{
			get { return parseIntegerKeyValue("Width"); }
		}

		protected int maxSize
		{
			get { return parseIntegerKeyValue("MaxSize"); }
		}
		protected int maxWidth
		{
			get { return parseIntegerKeyValue("MaxWidth"); }
		}
		protected int maxHeight
		{
			get { return parseIntegerKeyValue("MaxHeight"); }
		}

		protected double topRatio
		{
			get { return parseDoubleKeyValue("topRatio"); }
		}
		protected double leftRatio
		{
			get { return parseDoubleKeyValue("leftRatio"); }
		}

		protected double widthRatio
		{
			get { return parseDoubleKeyValue("widthRatio"); }
		}

		protected double heightRatio
		{
			get { return parseDoubleKeyValue("heightRatio"); }
		}

		protected OutputFormat outputFormat
		{
			get
			{
				var value = parseStringKeyValue("OutputFormat", false);
				if (string.IsNullOrEmpty(value)) return OutputFormat.Png;
				return (OutputFormat)Enum.Parse(typeof(OutputFormat), value);
			}
		}

		protected string fileName
		{
			get
			{
				var value = parseStringKeyValue("FileName", false);
				return string.IsNullOrEmpty(value) ? "Default" : value;
			}
		}

		
		#region parsers
		protected bool parseBoolKeyValue(string key)
		{
			var value = QueryString[key];
			if (string.IsNullOrEmpty(value)) return false;
			bool result;
			if (bool.TryParse(value, out result)) return result;
			throw new ArgumentException(string.Format(nonNumericFormatValueMessage, key));
		}

		protected int parseIntegerKeyValue(string key)
		{
			var value = parseStringKeyValue(key, true);
			int result;
			if (int.TryParse(value, out result)) return result;
			throw new ArgumentException(string.Format(nonNumericFormatValueMessage, key));
		}

		protected double parseDoubleKeyValue(string key)
		{
			var value = parseStringKeyValue(key, true);
			double result;
			if (double.TryParse(value, out result)) return result;
			throw new ArgumentException(string.Format(nonNumericFormatValueMessage, key));
		}

		protected string parseStringKeyValue(string key, bool isMandatory)
		{
			var value = QueryString[key];
			if (string.IsNullOrEmpty(value) & isMandatory)
				throw new ArgumentException(string.Format(missingQueryStringKeyValuePairMessage, key));
			return value;
		}

		protected Guid parseGuidKeyValue(string key)
		{
			var value = parseStringKeyValue(key, true);
			Guid result;
			try
			{
				result = new Guid(value);
			}
			catch (Exception)
			{
				throw new ArgumentException(string.Format(nonGuidFormatValueMessage, key));
			}
			return result;
		}

		#endregion

		#region error messages
		private const string missingQueryStringKeyValuePairMessage = "You must provide the {0} for the image.";
		private const string nonNumericFormatValueMessage = "The value for {0} is not an integer.";
		private const string nonGuidFormatValueMessage = "The value for {0} is not a guid.";
		#endregion

		protected HttpContext context { get; set; }
		protected NameValueCollection QueryString
		{
			get { return context.Request.QueryString; }
		}
		public bool IsReusable
		{
			get { return true; }
		}
	}
}