using CheckingCamera.Model.Abstraction;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Drawing;

namespace CheckingCamera.Model
{
    internal class CropImage
    {
        public static Image<Bgr, Byte> CropImageBGR(Image<Bgr, Byte> image, SegmentPhoto segmentPhoto)
        {
            if (image == null)
            {
                throw new ArgumentNullException("Image Null");
            }

            Rectangle rectangle = new Rectangle(segmentPhoto.X, segmentPhoto.Y, segmentPhoto.Width, segmentPhoto.Height);
            Image<Bgr, Byte> croppedImage = image.GetSubRect(rectangle);
            Image<Bgr, Byte> resultImage = new Image<Bgr, Byte>(segmentPhoto.Width, segmentPhoto.Height);
            resultImage = croppedImage.Clone();

            return resultImage;
        }

        public static Image<Gray, Byte> CropImageGray(Image<Bgr, Byte> image, SegmentPhoto segmentPhoto)
        {
            if (image == null)
            {
                throw new ArgumentNullException("Image Null");
            }

            Rectangle rectangle = new Rectangle(segmentPhoto.X, segmentPhoto.Y, segmentPhoto.Width, segmentPhoto.Height);
            Image<Bgr, Byte> croppedImage = image.GetSubRect(rectangle);
            Image<Gray, Byte> resultImage = new Image<Gray, Byte>(segmentPhoto.Width, segmentPhoto.Height);
            resultImage = croppedImage.Clone().Convert<Gray, byte>();

            return resultImage;
        }
    }
}
