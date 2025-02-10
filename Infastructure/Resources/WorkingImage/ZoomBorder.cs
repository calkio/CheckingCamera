using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using CheckingCamera.ViewModel.Camera;

namespace CheckingCamera.Infastructure.Resources.WorkingImage
{
    internal class ZoomBorder : Border
    {
        private UIElement child = null;
        private Point origin;
        private Point start;
        private Point selectionStart;
        private SelectionAdorner selectionAdorner;

        private bool isSelecting = false;

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform).Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform).Children.First(tr => tr is ScaleTransform);
        }

        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (value != null && value != this.Child)
                    this.Initialize(value);
                base.Child = value;
            }
        }

        public void Initialize(UIElement element)
        {
            this.child = element;
            if (child != null)
            {
                TransformGroup group = new TransformGroup();
                ScaleTransform st = new ScaleTransform();
                group.Children.Add(st);
                TranslateTransform tt = new TranslateTransform();
                group.Children.Add(tt);
                child.RenderTransform = group;
                child.RenderTransformOrigin = new Point(0.0, 0.0);

                this.MouseWheel += child_MouseWheel;
                this.MouseLeftButtonDown += child_MouseLeftButtonDown;
                this.MouseLeftButtonUp += child_MouseLeftButtonUp;
                this.MouseLeftButtonUp += Image_MouseLeftButtonUp;
                this.MouseMove += child_MouseMove;
                this.MouseMove += Image_MouseMove;
                this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(child_PreviewMouseRightButtonDown);

                if (child is Image image)
                {
                    image.MouseMove += Image_MouseMove;
                }

                this.DataContextChanged += ZoomBorder_DataContextChanged;
                RestoreSelection();
            }
        }

        private void ZoomBorder_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RestoreSelection();
        }

        private void RestoreSelection()
        {
            var vm = this.DataContext as CanvasVM;
            if (vm != null && vm.SelectionStart.HasValue && vm.SelectionEnd.HasValue)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (selectionAdorner == null)
                {
                    selectionAdorner = new SelectionAdorner(this)
                    {
                        Start = vm.SelectionStart.Value,
                        End = vm.SelectionEnd.Value
                    };
                    adornerLayer.Add(selectionAdorner);
                }
                else
                {
                    selectionAdorner.Start = vm.SelectionStart.Value;
                    selectionAdorner.End = vm.SelectionEnd.Value;
                    selectionAdorner.Visibility = Visibility.Visible;
                }
                this.isSelecting = false; // Окончание режима выделения
                this.Cursor = Cursors.Arrow;
            }
        }

        public void Reset()
        {
            if (child != null)
            {
                var st = GetScaleTransform(child);
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;

                var tt = GetTranslateTransform(child);
                tt.X = 0.0;
                tt.Y = 0.0;
            }
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (child != null && sender is Image image)
            {
                if (isSelecting && selectionAdorner != null)
                {
                    selectionAdorner.End = e.GetPosition(this);
                    selectionAdorner.InvalidateVisual();
                }
            }
        }

        private void child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (child != null)
            {
                var st = GetScaleTransform(child);
                var tt = GetTranslateTransform(child);

                double zoom = e.Delta > 0 ? .2 : -.2;
                if (!(e.Delta > 0) && (st.ScaleX < .4 || st.ScaleY < .4))
                    return;

                Point relative = e.GetPosition(child);
                double absoluteX = relative.X * st.ScaleX + tt.X;
                double absoluteY = relative.Y * st.ScaleY + tt.Y;

                double zoomCorrected = zoom * st.ScaleX;

                st.ScaleX += zoomCorrected;
                st.ScaleY += zoomCorrected;

                tt.X = absoluteX - relative.X * st.ScaleX;
                tt.Y = absoluteY - relative.Y * st.ScaleY;
            }
        }

        private void Image_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (child != null && sender is Image image)
            {
                if (isSelecting)
                {
                    EndSelection(e.GetPosition(this));
                }
                else
                {
                    child.ReleaseMouseCapture();
                    this.Cursor = Cursors.Arrow;
                }
            }
        }

        private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                if (isSelecting)
                {
                    EndSelection(e.GetPosition(this));
                }
                else
                {
                    child.ReleaseMouseCapture();
                    this.Cursor = Cursors.Arrow;
                }
            }
        }

        private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && isSelecting == false)
                {
                    isSelecting = true;
                    selectionStart = e.GetPosition(this);

                    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
                    if (selectionAdorner == null)
                    {
                        selectionAdorner = new SelectionAdorner(this)
                        {
                            Start = selectionStart,
                            End = selectionStart
                        };
                        adornerLayer.Add(selectionAdorner);
                    }
                    else
                    {
                        selectionAdorner.Start = selectionStart;
                        selectionAdorner.End = selectionStart;
                        selectionAdorner.Visibility = Visibility.Visible;
                    }

                    this.Cursor = Cursors.Cross;
                }
                else if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && isSelecting == true)
                {
                    EndSelection(e.GetPosition(this));
                }
                else
                {
                    var tt = GetTranslateTransform(child);
                    start = e.GetPosition(this);
                    origin = new Point(tt.X, tt.Y);
                    this.Cursor = Cursors.Hand;
                    child.CaptureMouse();
                }
            }
        }

        private void EndSelection(Point endPoint)
        {
            isSelecting = false;

            var topLeft = new Point(
                Math.Min(selectionStart.X, endPoint.X),
                Math.Min(selectionStart.Y, endPoint.Y)
            );

            var bottomRight = new Point(
                Math.Max(selectionStart.X, endPoint.X),
                Math.Max(selectionStart.Y, endPoint.Y)
            );

            var vm = this.DataContext as IZoomBorderViewModel;
            if (vm != null)
            {
                vm.OnSelectionCompleted(ConvertToImageCoordinates(topLeft), ConvertToImageCoordinates(bottomRight));
            }

            this.Cursor = Cursors.Arrow;
        }

        void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Reset();
        }

        private void child_MouseMove(object sender, MouseEventArgs e)
        {
            if (child != null)
            {
                if (child.IsMouseCaptured)
                {
                    var tt = GetTranslateTransform(child);
                    Vector v = start - e.GetPosition(this);
                    tt.X = origin.X - v.X;
                    tt.Y = origin.Y - v.Y;
                }
            }
        }

        private Point ConvertToImageCoordinates(Point point)
        {
            if (child is Image image)
            {
                var st = GetScaleTransform(child);
                var tt = GetTranslateTransform(child);

                var source = (BitmapSource)image.Source;
                double originalWidth = source.PixelWidth;
                double originalHeight = source.PixelHeight;

                Point relativeToImage = new Point((point.X - tt.X) / st.ScaleX, (point.Y - tt.Y) / st.ScaleY);

                double pixelX = relativeToImage.X * originalWidth / image.ActualWidth;
                double pixelY = relativeToImage.Y * originalHeight / image.ActualHeight;

                return new Point(pixelX, pixelY);
            }
            return point;
        }
    }
}
