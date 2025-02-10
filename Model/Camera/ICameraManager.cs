namespace CheckingCamera.Model.Camera
{
    public interface ICameraManager
    {
        public IEnumerable<ICamera> GetAllCameras();
    }
}
