using System;
using System.Drawing;

namespace Business
{
    public class MaintainAspectRatioResize
    {

        public int MaxHeight = 65;
        public int MaxWidth = 140;

        public Image Resize(Bitmap imageToResize)
        {
            return Scale(imageToResize, false);
        }

        public Image ResizeAndCrop(Bitmap imageToResizeAndCrop)
        {

            int width, height;

            if (imageToResizeAndCrop.Height > MaxHeight || imageToResizeAndCrop.Width > MaxWidth)
            {
                var percent = DeterminePercentageForResizeAndCrop(imageToResizeAndCrop.Height, imageToResizeAndCrop.Width);
                float floatWidth = imageToResizeAndCrop.Width;

                if (imageToResizeAndCrop.Width > MaxWidth)
                    floatWidth = imageToResizeAndCrop.Width * percent;

                float floatHeight = imageToResizeAndCrop.Height;
                if (imageToResizeAndCrop.Height > MaxHeight)
                    floatHeight = imageToResizeAndCrop.Height * percent;

                width = Convert.ToInt32(floatWidth);
                height = Convert.ToInt32(floatHeight);
            }
            else
            {
                width = imageToResizeAndCrop.Width;
                height = imageToResizeAndCrop.Height;
            }

            //Image thumb = imageToResizeAndCrop.GetThumbnailImage(width, height, ThumbnailCallback, IntPtr.Zero);
            var thumb = new Bitmap(imageToResizeAndCrop, new Size(width, height));

            var cropWidth = width > MaxWidth ? MaxWidth : width;
            var cropHeight = height > MaxHeight ? MaxHeight : height;

            var cropArea = new Rectangle(0, 0, cropWidth, cropHeight);
            var bmpImage = new Bitmap(thumb);
            var bmpCrop = bmpImage.Clone(cropArea, bmpImage.PixelFormat);

            imageToResizeAndCrop.Dispose();

            return bmpCrop;

            //return Scale(imageToResizeAndCrop, true);
        }

        private Image Scale(Image imageToScale, bool isCrop)
        {

        	var width = imageToScale.Width;
        	var height = imageToScale.Height;

            if (imageToScale.Height > MaxHeight || imageToScale.Width > MaxWidth)
            {
            	var percent = determinePercentage(height, width, isCrop);
                var floatWidth = imageToScale.Width * percent;
                var floatHeight = imageToScale.Height * percent;
                width = Convert.ToInt32(floatWidth);
                height = Convert.ToInt32(floatHeight);

            }

            var thumb = imageToScale.GetThumbnailImage(width, height, ThumbnailCallback, IntPtr.Zero);

            Image bmpCrop;
            if (isCrop)
            {
                var cropArea = new Rectangle(0, 0, MaxWidth, MaxHeight);
                var bmpImage = new Bitmap(thumb);
                bmpCrop = bmpImage.Clone(cropArea, bmpImage.PixelFormat);
            }
            else
            {
                bmpCrop = thumb;
            }

            imageToScale.Dispose();

            return bmpCrop;
        }

        private float determinePercentage(int height, int width, bool isCrop)
        {
            float percentageHeight = 1;
            float percentageWidth = 1;

            if (height > MaxHeight)
                percentageHeight = MaxHeight / (float)height;

            if (width > MaxWidth)
                percentageWidth = MaxWidth / (float)width;

            float percent;

            if (isCrop)
                percent = percentageHeight < percentageWidth ? percentageWidth : percentageHeight;
            else
                percent = percentageHeight > percentageWidth ? percentageWidth : percentageHeight;

            if (percent > 1 || percent == 0)
                throw new Exception("Percent cannot be greater than 1 or equal to zero");
            return percent;
        }

        private float DeterminePercentageForResizeAndCrop(int height, int width)
        {
            return determinePercentage(height, width, true);
        }


        public bool ThumbnailCallback()
        {
            return false;
        }


    }
}