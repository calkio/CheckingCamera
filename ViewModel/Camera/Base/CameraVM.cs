using Camera.API;
using CheckingCamera.Infastructure.Command;
using CheckingCamera.ViewModel.Base;
using System.Windows.Input;
using WARD.Core.Configuration.DTO;
using WARD.Core.Entity.Image;

namespace CheckingCamera.ViewModel.Camera.Base
{
    internal class CameraVM : BaseVM
    {
        private MainVM _mvvm;

        private CanvasVM _canvasVM;
        public CanvasVM CanvasVM { get => _canvasVM; set => Set(ref _canvasVM, value); }


        private SelectedCameraVM _selectedCameraVM;
        public SelectedCameraVM SelectedCameraVM { get => _selectedCameraVM; set => Set(ref _selectedCameraVM, value); }


        public CameraVM(MainVM mvvm)
        {
            _mvvm = mvvm;

            _canvasVM = new CanvasVM(mvvm);
            _selectedCameraVM = new SelectedCameraVM(mvvm, new CameraManager(), _canvasVM);
        }
    }
}
