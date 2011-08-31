using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.Text;

namespace ImageManager
{
	/// <summary>
	/// Summary description for ImageFunctions
	/// </summary>
	public class ImageFunctions
	{
		public bool CreateThumbNailImage(string SourceFilePath, string OutputFilePath, int width, int height)
		{
			// create an image object, using the filename we just retrieved
			var image = Image.FromFile(SourceFilePath);

			// If the height is greater than the width then remove bottom part of image 
			// of if greater width than height.
			int resizeheight;
			int resizewidth;

			if (image.Width != image.Height)
			{
				if (image.Width > image.Height)
				{
					resizeheight = image.Height;
					resizewidth = image.Height;
				}
				else
				{
					resizeheight = image.Width;
					resizewidth = image.Width;
				}

				var location = new Point(0, 0);
				var size = new Size(resizewidth, resizeheight);
				var rectangle = new Rectangle(location, size);

				image = cropImage(image, rectangle);
			}

			var thumbnailImage = image.GetThumbnailImage(width, height, ThumbnailCallback, IntPtr.Zero);
			thumbnailImage.Save(OutputFilePath);
			return true;
		}

		///  <summary>
		/// Required, but not used
		/// </summary>
		/// <returns>true</returns>
		public static bool ThumbnailCallback()
		{
			return true;
		}

		private Image cropImage(Image img, Rectangle cropArea)
		{
			var bmpImage = new Bitmap(img);
			var bmpCrop = bmpImage.Clone(cropArea,
			                             bmpImage.PixelFormat);
			return bmpCrop;
		}

		private Image resizeImage(Image imgToResize, Size size)
		{
			int sourceWidth = imgToResize.Width;
			int sourceHeight = imgToResize.Height;

			float nPercent = 0;
			float nPercentW = 0;
			float nPercentH = 0;

			nPercentW = ((float)size.Width / (float)sourceWidth);
			nPercentH = ((float)size.Height / (float)sourceHeight);

			if (nPercentH < nPercentW)
				nPercent = nPercentH;
			else
				nPercent = nPercentW;

			int destWidth = (int)(sourceWidth * nPercent);
			int destHeight = (int)(sourceHeight * nPercent);

			Bitmap b = new Bitmap(destWidth, destHeight);
			Graphics g = Graphics.FromImage((System.Drawing.Image)b);
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;

			g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
			g.Dispose();

			return (System.Drawing.Image)b;
		}


		public static bool DeleteImage(string filename, string directory)
		{
			var savesuccess = false;
			try
			{
				var fileDirectory = directory + "\\" + filename;
				if (File.Exists(fileDirectory))
				{
					File.Delete(fileDirectory);
					savesuccess = true;
				}
			}
			catch (Exception)
			{
				savesuccess = false;
			}

			return savesuccess;
		}

		///// <summary>
		///// Return a random background image from the background directory
		///// </summary>
		///// <returns></returns>
		//public static string SelectRandomBackgroundImage()
		//{
		//    string backgrounddirectory = ConfigurationManager.AppSettings["BackgroundImagesDirectory"].ToString();
		//    string folder = HttpContext.Current.Server.MapPath(backgrounddirectory);

		//    string[] filters = { "*.jpg", "*.png", "*.gif" };
		//    var images = new ArrayList();

		//    foreach (var filter in filters)
		//    {
		//        images.AddRange(Directory.GetFiles(folder, filter));
		//    }

		//    switch (images.Count)
		//    {
		//        case 0:
		//            return "";
		//        case 1:
		//            return BuildBackgroundImageStyle(backgrounddirectory.Replace("~", "") + images[0].ToString().Substring(images[0].ToString().LastIndexOf("\\") + 1));
		//        default:
		//            {
		//                var RandomClass = new Random();
		//                var RandomNumber = RandomClass.Next(0, images.Count);
		//                return BuildBackgroundImageStyle(backgrounddirectory.Replace("~", "") + images[RandomNumber].ToString().Substring(images[RandomNumber].ToString().LastIndexOf("\\") + 1));
		//            }
		//    }

		//}

		//private static string BuildBackgroundImageStyle(string filepath)
		//{
		//    var sb = new StringBuilder();
		//    sb.Append("background:url('");
		//    sb.Append(filepath);
		//    sb.Append("') ");

		//    // Check if the image is any of the 'special background images with special properties'
		//    // Get list of special images
		//    string[] specialimages = ConfigurationManager.AppSettings["BackgroundSpecialImages"].ToString().Split(',');
		//    string[] specialimagesstyle = ConfigurationManager.AppSettings["BackgroundSpecialImagesStyle"].ToString().Split(',');

		//    var filename = filepath.Substring(filepath.LastIndexOf("/") + 1);
		//    for (var i = 0; i < specialimages.Length; i++)
		//    {
		//        if (filename.ToLower() == specialimages[i].ToLower())
		//        {
		//            sb.Append(specialimagesstyle[i]);
		//        }
		//    }

		//    return sb.ToString();

		//}

	}
}