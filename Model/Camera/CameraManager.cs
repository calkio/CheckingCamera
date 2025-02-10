
using DirectShowLib;

namespace CheckingCamera.Model.Camera
{
    public class CameraManager : ICameraManager
    {
        private IEnumerable<ICamera> _cameras;

        public CameraManager()
        {
            _cameras = InitCameras();
        }

        private IEnumerable<ICamera> InitCameras()
        {
            var devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            var cameras = new List<ICamera>();
            for (var i = 0; i < devices.Length; i++)
            {
                var device = devices[i];
                var camera = new Camera(i, device.Name, device.DevicePath);
                cameras.Add(camera);
            }

            return cameras;
        }

        public IEnumerable<ICamera> GetAllCameras()
        {
            return _cameras;
        }
    }
}
