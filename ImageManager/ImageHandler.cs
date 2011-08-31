using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;

namespace ImageManager
{
	public class ImageHandler : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			this.context = context;
			var service = new ImageService(context);

			Bitmap bitmap;
			switch (imageMod)
			{
				case ImageMod.Raw:
					bitmap = service.Get(fileName, maxSize);
					break;
				case ImageMod.SpecifiedCrop:
					bitmap = service.GetAndCrop(fileName, width, height, widthRatio ,heightRatio, leftRatio, topRatio);
					break;
				default:
					if (cacheEnabled)
					{
						bitmap = service.GetCached(fileName, width, height, imageMod, hexBackGroundColour, anchor);
					}
					else
					{
						bitmap = service.Get(fileName, width, height, imageMod, hexBackGroundColour, anchor) ?? getDefaultImage();
					}
					break;
			}

			writeOutput(bitmap);
		}



		protected bool cacheEnabled
		{
			get { return parseBoolKeyValue("CacheEnabled"); }
		}

		private void writeOutput(Bitmap bitmap)
		{
			var memStream = new MemoryStream();

			context.Response.ContentType = "image/png";
			bitmap.MakeTransparent(Color.Fuchsia);
			bitmap.Save(memStream, ImageFormat.Png);

			setClientCaching();
			memStream.WriteTo(context.Response.OutputStream);
		}

		private void setClientCaching()
		{
			var cacheType = disableClientCache ? HttpCacheability.NoCache : HttpCacheability.Public;
			context.Response.Cache.SetCacheability(cacheType);
			context.Response.Cache.SetExpires(DateTime.Now.AddMinutes(5));
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
				return (ImageMod) Enum.Parse(typeof (ImageMod), value);
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
			get
			{
				return parseIntegerKeyValue("Height");
			}
		}
		protected int width
		{
			get
			{
				return parseIntegerKeyValue("Width");
			}
		}

		protected int maxSize
		{
			get { return parseIntegerKeyValue("MaxSize"); }
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



		protected string fileName
		{
			get
			{
				var value = parseStringKeyValue("FileName", false);
				return string.IsNullOrEmpty(value) ? "Default" : value;
			}
		}	

		#region parsers
		private bool parseBoolKeyValue(string key)
		{
			var value = QueryString[key];
			if (string.IsNullOrEmpty(value)) return false;
			bool result;
			if (bool.TryParse(value, out result)) return result;
			throw new ArgumentException(string.Format(nonNumericFormatValueMessage, key));
		}
		private int parseIntegerKeyValue(string key)
		{
			var value = parseStringKeyValue(key, true);
			int result;
			if (int.TryParse(value, out result)) return result;
			throw new ArgumentException(string.Format(nonNumericFormatValueMessage, key));
		}

		private double parseDoubleKeyValue(string key)
		{
			var value = parseStringKeyValue(key, true);
			double result;
			if (double.TryParse(value, out result)) return result;
			throw new ArgumentException(string.Format(nonNumericFormatValueMessage, key));
		}

		private string parseStringKeyValue(string key, bool isMandatory)
		{
			var value = QueryString[key];
			if (string.IsNullOrEmpty(value) & isMandatory) 
				throw new ArgumentException(string.Format(missingQueryStringKeyValuePairMessage, key));
			return value;
		}
		#endregion

		#region error messages
		private const string missingQueryStringKeyValuePairMessage = "You must provide the {0} for the image.";
		private const string nonNumericFormatValueMessage = "The value for {0} is not an integer.";
		#endregion

		protected HttpContext context { get; set; }
		protected NameValueCollection QueryString
		{
			get { return context.Request.QueryString; }
		}
		private Bitmap getDefaultImage()
		{
			throw new NotImplementedException("No default image has been specified.");
		}
		public bool IsReusable
		{
			get { return true; }
		}
	}
}