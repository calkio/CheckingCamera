using CheckingCamera.ViewModel.Base;
using CheckingCamera.ViewModel.Camera.Base;

namespace CheckingCamera.ViewModel
{
    internal class MainVM : BaseVM
    {
        private BaseVM _selectedContent;
        public BaseVM SelectedContent { get => _selectedContent; set => Set(ref _selectedContent, value); }


        private CameraVM _cameraVM;
        public CameraVM CameraVM { get => _cameraVM; set => Set(ref _cameraVM, value); }

        public MainVM()
        {
            _cameraVM = new CameraVM(this);
            _selectedContent = _cameraVM;
        }
    }
}
