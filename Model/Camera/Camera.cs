using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;

namespace CheckingCamera.Model.Camera
{
    public class Camera : ICamera
    {
        public int Index { get; }
        public string Name { get; }
        public string Path { get; }
        public int Exposure { get; set; } = -9;
        public int Brightness { get; set; } = 10;
        public int Contrast { get; set; } = 1;
        public int Focus { get; set; } = 0;


        public event EventHandler<Image<Bgr, byte>>? StreamImageChanged;

        // Можно добавить свойство, чтобы внешний код знал, открыта ли камера
        public bool IsOpened => _videoCapture?.IsOpened ?? false;

        private VideoCapture? _videoCapture;
        private Mat? _currentFrame;

        private readonly object frameLock = new object();
        private CancellationTokenSource? _cancellationTokenSource;

        private int frameWidth = 3264;
        private int frameHeight = 2448;

        // Храним последний полученный кадр, чтобы CapturePhoto мог вернуть его
        private Image<Bgr, byte>? _lastFrame;

        public Camera(int index, string name, string path)
        {
            Index = index;
            Name = name;
            Path = path;
            // Можно НЕ инициализировать камеру в конструкторе, а отложить до StartStreamVideo.
            // Но если хотите сразу, раскомментируйте:
            // InitializeCapture();
        }

        private void InitializeCapture()
        {
            if (_videoCapture != null && _videoCapture.IsOpened)
                return; // уже инициализирована

            _videoCapture = new VideoCapture(Index, VideoCapture.API.DShow);
            if (!_videoCapture.IsOpened)
            {
                throw new ArgumentException("Не удалось открыть устройство захвата видео с указанным индексом.");
            }

            // Применяем параметры (не все камеры это поддерживают)
            _videoCapture.Set(CapProp.Brightness, Brightness);
            _videoCapture.Set(CapProp.Contrast, Contrast);
            _videoCapture.Set(CapProp.FrameWidth, frameWidth);
            _videoCapture.Set(CapProp.FrameHeight, frameHeight);
            _videoCapture.Set(CapProp.Focus, Focus);

            _videoCapture.Set(CapProp.AutoExposure, 0);
            _videoCapture.Set(CapProp.Exposure, Exposure);

            // Делаем короткую паузу, чтобы камера «прогрелась»:
            Thread.Sleep(500);
        }

        /// <summary>
        /// Запускает стрим в фоновом потоке, с периодом fps.
        /// </summary>
        public void StartStreamVideo(int fps)
        {
            InitializeCapture();

            if (_videoCapture == null || !_videoCapture.IsOpened)
                throw new InvalidOperationException("Видео не инициализировано или устройство не открыто.");

            StopStreamVideo(); // Останавливаем предыдущий поток, если был.

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            Task.Run(async () =>
            {
                var delay = 1000 / fps;

                while (!token.IsCancellationRequested)
                {
                    var frame = CaptureFrameInternal();
                    if (frame != null)
                    {
                        // Сохраняем в _lastFrame, чтобы CapturePhoto() мог вернуть
                        lock (frameLock)
                        {
                            _lastFrame?.Dispose();
                            _lastFrame = frame.Clone();
                        }
                        // Поднимаем событие стрима
                        StreamImageChanged?.Invoke(this, frame);
                    }

                    try
                    {
                        await Task.Delay(delay, token);
                    }
                    catch (TaskCanceledException)
                    {
                        // Прерываем цикл, если задача отменена
                        break;
                    }
                }

            }, token);
        }

        /// <summary>
        /// Останавливаем фоновый поток чтения.
        /// </summary>
        public void StopStreamVideo()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }

        /// <summary>
        /// Возвращает последний захваченный кадр.
        /// </summary>
        public Image<Bgr, byte> CapturePhoto()
        {
            lock (frameLock)
            {
                if (_lastFrame == null)
                {
                    throw new InvalidOperationException("Нет доступного кадра. Убедитесь, что камера запущена и стрим идёт.");
                }

                return _lastFrame.Clone();
            }
        }

        /// <summary>
        /// Вспомогательный метод для чтения кадра из VideoCapture.
        /// </summary>
        private Image<Bgr, byte>? CaptureFrameInternal()
        {
            if (_videoCapture == null)
                return null;

            lock (frameLock)
            {
                _currentFrame = new Mat();
                if (_videoCapture.Read(_currentFrame) && !_currentFrame.IsEmpty)
                {
                    return _currentFrame.ToImage<Bgr, byte>();
                }
                else
                {
                    return null;
                }
            }
        }

        public void Dispose()
        {
            StopStreamVideo();
            _videoCapture?.Dispose();
            _currentFrame?.Dispose();
            _lastFrame?.Dispose();
        }

        public void UpdateCfg()
        {
            StopStreamVideo();
            if (_videoCapture != null)
            {
                _videoCapture.Stop();
                _videoCapture.Dispose();
            }
        }
    }
}
