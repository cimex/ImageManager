using System;
using System.Collections.Specialized;
using NUnit.Framework;
using System.Web;
using Rhino.Mocks;
using ImageManager;

namespace ImageManagerTest
{
	[TestFixture]
	public class ImageHandlerTest
	{
		/*public void should_get_image()
		{

			IHttpHandler imageHandler = new ImageHandler();
			var context = MockRepository.GenerateMock<HttpContext>();
			var request = MockRepository.GenerateMock<HttpRequest>();
			var queryString = new NameValueCollection();
			queryString.Add("FileName", "Maribou.jpg");
			queryString.Add("ContentType", "Bird");
			queryString.Add("Width", "300");
			queryString.Add("Height", "200");
			queryString.Add("ImageMod", Enum.GetName(typeof(ImageMod), ImageMod.Scale));

			context.Expect(x => x.Request).Return(request);
			request.Expect(x => x.QueryString).Return(queryString);
			
			imageHandler.ProcessRequest(context);
		}*/
	}
}

