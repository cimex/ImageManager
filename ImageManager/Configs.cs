using System;
using System.Configuration;
using System.Web;

namespace ImageManager
{
	public static class Configs
	{
		public static TimeSpan CacheExpiration
		{
			get
			{
				var config = ConfigurationManager.AppSettings["ImageManager-CacheExpiration"];
				if (config == null) throw new ConfigurationErrorsException(
					"Please add the 'ImageManager-CacheExpiration' in minutes to the appSettings section within the Web.config.");
				int value;
				if(int.TryParse(config, out value)) return new TimeSpan(0,0,value,0);
				throw new ArgumentException("The 'ImageManager-CacheExpiration' configuration setting was not a numeric format");
			}
		}

		public static string TargetDirectory
		{
			get
			{
				var config = ConfigurationManager.AppSettings["ImageManager-TargetDirectory"];
				if (config == null) throw new ConfigurationErrorsException(
					"Please add the 'ImageManager-TargetDirectory' to the appSettings section within the Web.config.");
				return config;
			}
		}

		public static int MaxImageDimension
		{
			get
			{
				var config = ConfigurationManager.AppSettings["ImageManager-MaxImageDimension"];
				if (config == null) throw new ConfigurationErrorsException(
					"Please add the 'ImageManager-MaxImageDimension' in pixels to the appSettings section within the Web.config.");
				int value;
				if (int.TryParse(config, out value)) return value;
				throw new ArgumentException("The 'ImageManager-MaxImageDimension' configuration setting was not a numeric format");
			}
		}

	}
}