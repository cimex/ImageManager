using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace ImageManager
{
	public static class Utilities
	{
		public static readonly Color BackgroundColour = Color.Fuchsia;

		public static byte[] GetBytes(this Bitmap bitmap, OutputFormat outputFormat)
		{
			using (var memStream = new MemoryStream())
			{
				bitmap.MakeTransparent(BackgroundColour);
				switch (outputFormat)
				{
					case OutputFormat.Jpeg:
						bitmap.Save(memStream, ImageFormat.Jpeg);
						break;
					case OutputFormat.Gif:
						bitmap.Save(memStream, ImageFormat.Gif);
						break;
					case OutputFormat.Png:
						bitmap.Save(memStream, ImageFormat.Png);
						break;
					case OutputFormat.HighQualityJpeg:
						var p = new EncoderParameters(1);
						p.Param[0] = new EncoderParameter(Encoder.Quality, (long)95);
						bitmap.Save(memStream, GetImageCodeInfo("image/jpeg"), p);
						break;
				}
				return memStream.ToArray();
			}
		}

		public static ImageCodecInfo GetImageCodeInfo(string mimeType)
		{
			ImageCodecInfo[] info = ImageCodecInfo.GetImageEncoders();
			return info.FirstOrDefault(ici => ici.MimeType.Equals(mimeType, StringComparison.OrdinalIgnoreCase));
		}

		public static string GetContentType(OutputFormat outputFormat)
		{
			switch (outputFormat)
			{
				case OutputFormat.Gif:
					return "image/gif";
				case OutputFormat.Png:
					return "image/png";
				case OutputFormat.Jpeg:
				case OutputFormat.HighQualityJpeg:
					return "image/jpeg";
				default:
					return null;
			}
		}
	}
}
