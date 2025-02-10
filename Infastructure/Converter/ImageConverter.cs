using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using Emgu.CV;
using Emgu.CV.Structure;

namespace CheckingCamera.Infastructure.Converter
{
    internal class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Image<Bgr, Byte> image)
            {
                // Создаем WriteableBitmap под размер изображения
                var writeableBitmap = new WriteableBitmap(
                    image.Width,
                    image.Height,
                    96,     // DPI по горизонтали (условно)
                    96,     // DPI по вертикали (условно)
                    PixelFormats.Bgr24, // формат пикселей в Image<Bgr, Byte>
                    null
                );

                // Копируем «сырые» данные пикселей из Emgu в WriteableBitmap
                writeableBitmap.WritePixels(
                    new Int32Rect(0, 0, image.Width, image.Height),
                    image.Mat.DataPointer,
                    image.Mat.Step * image.Height, // общий размер выделенной под пиксели памяти
                    image.Mat.Step                 // "stride" — число байт на одну строку
                );

                return writeableBitmap;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
