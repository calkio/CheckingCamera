using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace CheckingCamera.Infastructure.Resources.WorkingImage
{
    public class SelectionAdorner : Adorner
    {
        private readonly Rectangle _rectangle;

        public Point Start { get; set; }
        public Point End { get; set; }

        public SelectionAdorner(UIElement adornedElement) : base(adornedElement)
        {
            _rectangle = new Rectangle
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 4, 2 }
            };
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            double x = Math.Min(Start.X, End.X) + 3;
            double y = Math.Min(Start.Y, End.Y) + 3;
            double width = Math.Abs(Start.X - End.X - 3);
            double height = Math.Abs(Start.Y - End.Y - 3);

            Rect rect = new Rect(x, y, width, height);
            drawingContext.DrawRectangle(null, new Pen(_rectangle.Stroke, _rectangle.StrokeThickness) { DashStyle = new DashStyle(_rectangle.StrokeDashArray, 0) }, rect);
        }
    }
}
