using CheckingCamera.Infastructure.Command;
using CheckingCamera.ViewModel.Base;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using CheckingCamera.Model;
using CheckingCamera.Model.Camera;

namespace CheckingCamera.ViewModel.Camera
{
    internal class SelectedCameraVM : BaseVM
    {
        private readonly MainVM _mvvm;
        private readonly ICameraManager _cameraManager;
        private readonly CanvasVM _canvasVM;
        private string _pathFolderSaveImageStream;
        private int _countImageInStram;

        private bool _isStreamingCam;
        public bool IsStreamingCam
        {
            get => _isStreamingCam;
            private set
            {
                if (Set(ref _isStreamingCam, value))
                {
                    // Обновление состояния команд при изменении
                    _startStreamCommand.RaiseCanExecuteChanged();
                    _stopStreamCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private string _infoPixel;
        public string InfoPixel { get => _infoPixel; set => Set(ref _infoPixel, value); }


        private string _infoPixelCoordinate;
        public string InfoPixelCoordinate { get => _infoPixelCoordinate; set => Set(ref _infoPixelCoordinate, value); }


        private int _exposure;
        public int Exposure 
        { 
            get => SelectedCamera.Exposure; 
            set 
            {
                Set(ref _exposure, value);
                SelectedCamera.Exposure = value;
                TitleExposure = $"Exposure: {value}";
            }
        }
        private string _titleExposure;
        public string TitleExposure { get => _titleExposure; set => Set(ref _titleExposure, value); }


        private int _brightness;
        public int Brightness
        {
            get => SelectedCamera.Brightness;
            set
            {
                Set(ref _brightness, value);
                SelectedCamera.Brightness = value;
                TitleBrightness = $"Brightness: {value}";
            }
        }
        private string _titleBrightness;
        public string TitleBrightness { get => _titleBrightness; set => Set(ref _titleBrightness, value); }


        private int _contrast;
        public int Contrast
        {
            get => SelectedCamera.Contrast;
            set
            {
                Set(ref _contrast, value);
                SelectedCamera.Contrast = value;
                TitleContrast = $"Contrast: {value}";
            }
        }
        private string _titleContrast;
        public string TitleContrast { get => _titleContrast; set => Set(ref _titleContrast, value); }


        public string _beterSharpness;
        public string BeterSharpness { get => _beterSharpness; set => Set(ref _beterSharpness, value); }

        public string _currentSharpness;
        public string CurrentSharpness { get => _currentSharpness; set => Set(ref _currentSharpness, value); }


        private ICamera _selectedCamera;
        public ICamera SelectedCamera
        {
            get => _selectedCamera;
            set
            {
                if (Set(ref _selectedCamera, value))
                {
                    // Можно автоматически останавливать предыдущую камеру
                    // или подписываться/отписываться от событий и т.п.
                }
            }
        }


        private ObservableCollection<ICamera> _allCameras;
        public ObservableCollection<ICamera> AllCameras
        {
            get => _allCameras;
            set => Set(ref _allCameras, value);
        }


        // Команды
        private readonly AsyncRelayCommand _startStreamCommand;
        public ICommand StartStreamCommand => _startStreamCommand;


        private readonly AsyncRelayCommand _stopStreamCommand;
        public ICommand StopStreamCommand => _stopStreamCommand;


        private readonly AsyncRelayCommand _saveImageCommand;
        public ICommand SaveImageCommand => _saveImageCommand;


        private readonly AsyncRelayCommand _saveImageStreamStartCommand;
        public ICommand SaveImageStreamStartCommand => _saveImageStreamStartCommand;


        private readonly AsyncRelayCommand _saveImageStreamEndCommand;
        public ICommand SaveImageStreamEndCommand => _saveImageStreamEndCommand;


        private readonly AsyncRelayCommand _sharpeningAlgorithmCommand;
        public ICommand SharpeningAlgorithmCommand => _sharpeningAlgorithmCommand;


        public SelectedCameraVM(MainVM mvvm, ICameraManager cameraManager, CanvasVM canvasVM)
        {
            _mvvm = mvvm;
            _cameraManager = cameraManager;
            _canvasVM = canvasVM;

            _allCameras = new ObservableCollection<ICamera>(_cameraManager.GetAllCameras());

            if (_allCameras.Count > 0)
            {
                SelectedCamera = _allCameras[0];
            }

            _titleExposure = $"Exposure: {Exposure}";
            _titleBrightness = $"Brightness: {Brightness}";
            _titleContrast = $"Contrast: {Contrast}";

            _beterSharpness = $"Лучшая резкость: {BeterSharpness}";
            _currentSharpness = $"Текущая резкость: {CurrentSharpness}";

            // Инициализация команд
            _startStreamCommand = new AsyncRelayCommand(StartStreamImplAsync, CanStartStream);
            _stopStreamCommand = new AsyncRelayCommand(StopStreamImplAsync, CanStopStream);
            _saveImageCommand = new AsyncRelayCommand(SaveImageImplAsync, CanSaveImage);
            _saveImageStreamStartCommand = new AsyncRelayCommand(SaveImageStreamStartImplAsync, CanSaveImageStreamStart);
            _saveImageStreamEndCommand = new AsyncRelayCommand(SaveImageStreamEndImplAsync, CanSaveImageStreamEnd);
            _sharpeningAlgorithmCommand = new AsyncRelayCommand(SharpeningAlgorithmImplAsync, CanSharpeningAlgorithm);
        }

       
        private async Task StartStreamImplAsync()
        {
            if (SelectedCamera != null && !IsStreamingCam)
            {
                try
                {
                    // Подпишемся на событие, чтобы отображать непрерывно
                    SelectedCamera.StreamImageChanged += OnCameraFrameChanged;

                    SelectedCamera.StartStreamVideo(30);

                    // Подождём немного, чтобы камера "раскачалась"
                    await Task.Delay(1000);

                    IsStreamingCam = true;
                    Console.WriteLine("Стрим видео запущен.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при запуске стрима: {ex.Message}");
                }
            }
        }
        private void OnCameraFrameChanged(object sender, Image<Bgr, byte> frame)
        {
            // Событие приходит из фонового потока, нужно перебросить в UI-поток (WPF)
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // Обновляем CanvasVM.Image, чтобы видеть "живую картинку"
                _canvasVM.Image = frame;
                if (_canvasVM.SegmentPhoto != null)
                {
                    CurrentSharpness = $"Текущая резкость: {Math.Round(SharpnessHelper.ComputeSharpness(frame, _canvasVM.SegmentPhoto), 1)}";
                    _canvasVM.DrawRectangle();
                }
            });
        }
        private bool CanStartStream() => !IsStreamingCam;


        private async Task StopStreamImplAsync()
        {
            if (SelectedCamera != null && IsStreamingCam)
            {
                try
                {
                    // Отписываемся от события, чтобы не получать кадры
                    SelectedCamera.StreamImageChanged -= OnCameraFrameChanged;

                    SelectedCamera.Dispose();

                    // Небольшая задержка
                    await Task.Delay(500);

                    IsStreamingCam = false;
                    Console.WriteLine("Стрим видео остановлен.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при остановке стрима: {ex.Message}");
                }
            }
        }
        private bool CanStopStream() => IsStreamingCam;


        private async Task SaveImageImplAsync()
        {
            try
            {
                string pathFolder = SelectedPathFolder();
                string time = DateTime.Now.ToString().Split(" ")[1].Replace(':', '-');
                string nameFile = @$"{pathFolder}\{time}.png";
                _canvasVM.Image.Save(nameFile);
            }
            catch
            {
            }
        }
        private bool CanSaveImage()
        {
            return true;
        }


        private async Task SaveImageStreamStartImplAsync()
        {
            _pathFolderSaveImageStream = SelectedPathFolder();

            SelectedCamera.StreamImageChanged += OnCameraSaveImageChanged;
        }
        private void OnCameraSaveImageChanged(object sender, Image<Bgr, byte> frame)
        {
            if (!string.IsNullOrEmpty(_pathFolderSaveImageStream))
            {
                _countImageInStram++;
                frame.Save($@"{_pathFolderSaveImageStream}\{_countImageInStram}.png");
            }
        }
        private bool CanSaveImageStreamStart() => true;


        private async Task SaveImageStreamEndImplAsync()
        {
            SelectedCamera.StreamImageChanged -= OnCameraSaveImageChanged;
            _countImageInStram = 0;
        }
        private bool CanSaveImageStreamEnd() => true;


        private async Task SharpeningAlgorithmImplAsync()
        {
            if (_canvasVM.SegmentPhoto != null)
            {
                var beterSharpest = SharpnessHelper.FindBeterSharpest(_pathFolderSaveImageStream, _canvasVM.SegmentPhoto);
                string? beterImage = beterSharpest.bestFile;
                _canvasVM.Image = new Image<Bgr, byte>(beterImage);
                BeterSharpness = $"Лучшая резкость: {Math.Round(beterSharpest.bestSharpness, 1)}";
            }
        }
        private bool CanSharpeningAlgorithm() => true;


        private string SelectedPathFolder()
        {
            var folderBrowserDialog = new VistaFolderBrowserDialog
            {
                Description = "Выберите папку",
                UseDescriptionForTitle = true // Использует описание как заголовок
            };
            if (folderBrowserDialog.ShowDialog() == true)
            {
                return folderBrowserDialog.SelectedPath;
            }
            throw new AggregateException();
        }
    }
}
