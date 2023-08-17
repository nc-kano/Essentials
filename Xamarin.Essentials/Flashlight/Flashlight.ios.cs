using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AVFoundation;

namespace Xamarin.Essentials
{
    [SuppressMessage("Interoperability", "CA1416:Walidacja zgodności z platformą", Justification = "Interoperability")]
    public static partial class Flashlight
    {
        static Task PlatformTurnOnAsync()
        {
            Toggle(true);

            return Task.CompletedTask;
        }

        static Task PlatformTurnOffAsync()
        {
            Toggle(false);

            return Task.CompletedTask;
        }

        static void Toggle(bool on)
        {
            var captureDevice = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
            if (captureDevice == null || !(captureDevice.HasFlash || captureDevice.HasTorch))
                throw new FeatureNotSupportedException();

            captureDevice.LockForConfiguration(out var error);

            if (error == null)
            {
                if (on)
                {
                    if (captureDevice.HasTorch)
                        captureDevice.SetTorchModeLevel(AVCaptureDevice.MaxAvailableTorchLevel, out var torchErr);
                    if (captureDevice.HasFlash)
#pragma warning disable CA1422
                        captureDevice.FlashMode = AVCaptureFlashMode.On;
#pragma warning restore CA1422
                }
                else
                {
                    if (captureDevice.HasTorch)
                        captureDevice.TorchMode = AVCaptureTorchMode.Off;
                    if (captureDevice.HasFlash)
#pragma warning disable CA1422
                        captureDevice.FlashMode = AVCaptureFlashMode.Off;
#pragma warning restore CA1422
                }
            }

            captureDevice.UnlockForConfiguration();
            captureDevice.Dispose();
            captureDevice = null;
        }
    }
}
