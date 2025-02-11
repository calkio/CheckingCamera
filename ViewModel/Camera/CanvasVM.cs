using CheckingCamera.Infastructure.Resources.WorkingImage;
using CheckingCamera.ViewModel.Base;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Windows;
using CheckingCamera.Model.Abstraction;

namespace CheckingCamera.ViewModel.Camera
{
    internal class CanvasVM : BaseVM, IZoomBorderViewModel
    {
        private MainVM _mvvm;

        private SegmentPhoto _segmentPhoto;
        public SegmentPhoto SegmentPhoto { get => _segmentPhoto; set => Set(ref _segmentPhoto, value); }

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

            SegmentPhoto = new SegmentPhoto(x, y, width, height);
        }

        public void DrawRectangle()
        {
            if (Image != null)
            {
                var clone = Image.Clone();
                clone.Draw(new System.Drawing.Rectangle(_segmentPhoto.X - 2, _segmentPhoto.Y - 2, _segmentPhoto.Width + 4, _segmentPhoto.Height + 4), new Bgr(System.Drawing.Color.Red), 2); // Рисуем прямоугольник на изображении

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
                _mvvm.CameraVM.SelectedCameraVM.InfoPixelCoordinate = $"X - {(int)pixel.X}, Y - {(int)pixel.Y}";
            }
            catch
            {
            }
        }
    }
}
