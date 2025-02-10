using CheckingCamera.Infastructure.Resources.WorkingImage;
using CheckingCamera.ViewModel.Base;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Collections.ObjectModel;
using WARD.Core.Entity.Image;
using System.Windows;

namespace CheckingCamera.ViewModel.Camera
{
    internal class CanvasVM : BaseVM, IZoomBorderViewModel
    {
        private MainVM _mvvm;
        int _countSegment = 0;


        private Image<Bgr, Byte> _image;
        public Image<Bgr, Byte> Image { get => _image; set => Set(ref _image, value); }


        private System.Windows.Point? _selectionStart;
        public System.Windows.Point? SelectionStart
        {
            get => _selectionStart;
            set
            {
                _selectionStart = value;
                OnPropertyChanged();
            }
        }

        private System.Windows.Point? _selectionEnd;
        public System.Windows.Point? SelectionEnd
        {
            get => _selectionEnd;
            set
            {
                _selectionEnd = value;
                OnPropertyChanged();
            }
        }


        private ObservableCollection<SegmentPhoto> _segments = new ObservableCollection<SegmentPhoto>()
        {
            new SegmentPhoto(1,1,1,1),
            new SegmentPhoto(1,1,1,1),
            new SegmentPhoto(1,1,1,1),
            new SegmentPhoto(1,1,1,1)
        };
        public ObservableCollection<SegmentPhoto> Segments { get => _segments; set => Set(ref _segments, value); }


        public SegmentPhoto _segment = new SegmentPhoto(0, 0, 0, 0);
        public SegmentPhoto Segment { get => _segment; set => Set(ref _segment, value); }


        public CanvasVM(MainVM mvvm)
        {
            _mvvm = mvvm;
        }

        public void OnSelectionCompleted(System.Windows.Point topLeft, System.Windows.Point bottomRight)
        {
            int x = (int)topLeft.X;
            int y = (int)topLeft.Y;
            int width = Math.Abs((int)topLeft.X - (int)bottomRight.X);
            int height = Math.Abs((int)topLeft.Y - (int)bottomRight.Y);
            Segment = new SegmentPhoto(x, y, width, height);

            Segments[_countSegment % 4] = Segment;
            _countSegment++;

            DrawRectangle();
        }

        public void DrawRectangle()
        {
            if (Image != null)
            {
                var clone = Image.Clone();
                clone.Draw(new System.Drawing.Rectangle(_segment.X, _segment.Y, _segment.Width, _segment.Height), new Bgr(System.Drawing.Color.Red), 2); // Рисуем прямоугольник на изображении

                Image = clone;
            }
        }

        public void OnMouseMove(Point pixel)
        {
            try
            {
                var channelB = _image.Data[(int)pixel.Y, (int)pixel.X, 0];
                var channelG = _image.Data[(int)pixel.Y, (int)pixel.X, 1];
                var channelR = _image.Data[(int)pixel.Y, (int)pixel.X, 2];
                string newInfo = $"B - {channelB}, G - {channelG}, R - {channelR}";
                _mvvm.CameraVM.SelectedCameraVM.InfoPixel = newInfo;
            }
            catch
            {
            }
        }
    }
}
