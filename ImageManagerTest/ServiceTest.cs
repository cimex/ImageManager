using System.Collections;
using System.Web;
using ImageManager;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using System.IO;
using System;
using Web.Mocks;
using System.Diagnostics;

namespace ImageManagerTest
{
    [TestFixture]
    public class ServiceTest
    {

    	private const string imageDirectory =
			@"C:\Users\richard.armstrong\Desktop\My Dropbox\My Dropbox\Photos\Weird Animals\";

		private string targetDirectory = @"Images/";
        private string contentType = "Brief";
		private string rawfilePath = @"Uploads\";
		private string fileName = "axolotl.jpg";
		private HttpContext context = (new MockHttpContext(true)).Context;


        [Test]
        public void should_save_image()
        {
            IImageService service = new ImageService(context);
            var isSaved = service.SaveForWeb(fileName, imageDirectory, targetDirectory);
            Assert.That(isSaved); 
        }


        [Test]
        public void should_save_image_and_create_new_directory()
        {
            IImageService service = new ImageService(context);
            var isSaved = service.SaveForWeb(fileName, rawfilePath, targetDirectory);
            Assert.That(isSaved);

            Assert.That(Directory.Exists(targetDirectory + contentType));
        }


        [Test]
        public void should_get_image()
        {
			context.Application.Set("Path",imageDirectory);
            IImageService service = new ImageService(context);
            var width = 100;
            var height = 300;

            var image = service.Get(fileName, width, height, ImageMod.Scale, null, null);
			image.Save("imageTransparencyText.bmp");
            Assert.That(image.Width, Is.EqualTo(width));
            Assert.That(image.Height, Is.EqualTo(height));
        }


		[Test]
		public void should_delete_image()
		{
			IImageService service = new ImageService(context);
			service.Delete(fileName);
			var image = service.Get(fileName, 100, 100, ImageMod.Scale, "FFAADD", null);
			Assert.That(image, Is.Null);
		}



		[Test]
		public void should_get_cached_image()
		{
			IImageService service = new ImageService(context);

			var width = 600;
			var height = 30;
			Assert.That(context.Cache.Count == 0);
			var image = service.GetCached(fileName, width, height, ImageMod.Scale, "FFAADD", null, OutputFormat.Png);
			Assert.That(context.Cache.Count > 0);

			foreach (DictionaryEntry cacheItem in context.Cache)
			{
				Debug.WriteLine(cacheItem.Key);
			}

			image = service.GetCached(fileName, width, height, ImageMod.Scale, "FFAADD", null, OutputFormat.Png);
			Assert.That(image, Is.Not.Null);
		}


		[Test]
		public void should_get_cropped_image()
		{
			IImageService service = new ImageService(context);
			var width = 100;
			var height = 200;

			var image = service.Get(fileName, width, height, ImageMod.Scale, "FFAADD", null);
			image.Save(targetDirectory +  width + "x" + "cropped2" + height + fileName);
			Assert.That(image.Width, Is.EqualTo(width));
			Assert.That(image.Height, Is.EqualTo(height));
		}

		//[Test]
		//public void should_get_cropped_Image_by_anchor_point()
		//{
		//    var service = new ImageService(context);
		//    var width = 50;
		//    var height = 50;
		//    var top = 10;
		//    var left = 30;
		//    var scale = 40;

		//    var image = service.GetAndCrop(fileName, width, height, scale, top, left);
		//    Assert.That(image.Width, Is.EqualTo(width));
		//    Assert.That(image.Height, Is.EqualTo(height));
		//}
    }
}
