using System.Globalization;
using System.Windows.Data;

namespace CheckingCamera.Infastructure.Resources.Style
{
    public class FontSizeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is double width && values[1] is double height)
            {
                double parentSize = (width + height * 2) / 2;

                // Получаем процентное соотношение из параметра
                if (parameter is string percentageString && double.TryParse(percentageString, out double percentage))
                {
                    return parentSize * (percentage / 100);
                }

                // Если параметр не указан или не удалось его распарсить, используем значение по умолчанию
                return parentSize * 0.05; // 5% от среднего размера родительского элемента
            }
            return 12; // Значение по умолчанию
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
