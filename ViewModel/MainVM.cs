using CheckingCamera.ViewModel.Base;
using CheckingCamera.ViewModel.Camera.Base;

namespace CheckingCamera.ViewModel
{
    internal class MainVM : BaseVM
    {
        private BaseVM _selectedContent;
        public BaseVM SelectedContent { get => _selectedContent; set => Set(ref _selectedContent, value); }

        public MainVM()
        {
            _selectedContent = new CameraVM(this);
        }
    }
}
