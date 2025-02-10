using System.Windows;

namespace CheckingCamera.Infastructure.Resources.WorkingImage
{
    internal interface IZoomBorderViewModel
    {
        void OnSelectionCompleted(Point topLeft, Point bottomRight);
    }
}
