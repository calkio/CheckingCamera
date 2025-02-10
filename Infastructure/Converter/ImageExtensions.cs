using Emgu.CV.Structure;
using Emgu.CV;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace CheckingCamera.Infastructure.Converter
{
    public static class ImageExtensions
    {
        public static BitmapSource ToBitmapSource(Image<Bgr, byte> image)
        {
            using (Bitmap bitmap = image.ToBitmap())
            {
                IntPtr ptr = bitmap.GetHbitmap();
                BitmapSource source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                source.Freeze();
                return source;
            }
        }
    }
}
