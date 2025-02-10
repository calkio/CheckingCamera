using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using System.IO;

namespace CheckingCamera.Model
{
    internal class SharpnessHelper
    {
        /// <summary>
        /// Возвращает путь к файлу в folderPath, у которого "резкость" (Laplacian variance) максимальна.
        /// Если файлов нет, возвращает null.
        /// </summary>
        public static string FindSharpestImage(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("Указанная папка не существует: " + folderPath);
                return null;
            }

            // Собираем все .png-файлы
            string[] files = Directory.GetFiles(folderPath, "*.png", SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
            {
                Console.WriteLine("В папке нет PNG-файлов.");
                return null;
            }

            string bestFile = null;
            double bestSharpness = double.MinValue;

            foreach (string file in files)
            {
                try
                {
                    // Загружаем изображение как цветное (можно и как ч/б, если нужно)
                    using (Mat image = CvInvoke.Imread(file, ImreadModes.Color))
                    {
                        double sharpness = ComputeSharpness(image);

                        if (sharpness > bestSharpness)
                        {
                            bestSharpness = sharpness;
                            bestFile = file;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Не удалось загрузить или обработать файл {file}: {ex.Message}");
                }
            }

            Console.WriteLine($"Самое резкое изображение: {bestFile}, резкость: {bestSharpness}");
            return bestFile;
        }

        /// <summary>
        /// Вычисляет "резкость" (через дисперсию Лапласиана) для переданного кадра/изображения.
        /// </summary>
        private static double ComputeSharpness(Mat frame)
        {
            Mat gray = new Mat();
            CvInvoke.CvtColor(frame, gray, ColorConversion.Bgr2Gray);

            Mat laplacian = new Mat();
            CvInvoke.Laplacian(gray, laplacian, DepthType.Cv64F);

            MCvScalar mean = new MCvScalar();
            MCvScalar stdDev = new MCvScalar();

            CvInvoke.MeanStdDev(laplacian, ref mean, ref stdDev);

            double variance = stdDev.V0 * stdDev.V0;
            return variance;
        }
    }
}
