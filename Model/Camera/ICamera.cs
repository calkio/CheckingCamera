using Emgu.CV.Structure;
using Emgu.CV;

namespace CheckingCamera.Model.Camera
{
    public interface ICamera
    {
        int Index { get; }
        string Name { get; }
        string Path { get; }
        int Exposure { get; set; }
        int Brightness { get; set; }
        int Contrast { get; set; }
        int Focus { get; set; }

        event EventHandler<Image<Bgr, byte>>? StreamImageChanged;

        void StartStreamVideo(int fps);
        void StopStreamVideo();
        void UpdateCfg();
        Image<Bgr, byte> CapturePhoto();
        bool IsOpened { get; }
    }
}
