using System.Diagnostics.CodeAnalysis;
using Foundation;
using UIKit;

namespace Xamarin.Essentials
{
    [SuppressMessage("Interoperability", "CA1416:Walidacja zgodności z platformą", Justification = "Interoperability")]
    public static partial class DeviceDisplay
    {
        static NSObject observer;

        static bool PlatformKeepScreenOn
        {
            get => UIApplication.SharedApplication.IdleTimerDisabled;
            set => UIApplication.SharedApplication.IdleTimerDisabled = value;
        }

        static DisplayInfo GetMainDisplayInfo()
        {
            var bounds = UIScreen.MainScreen.Bounds;
            var scale = UIScreen.MainScreen.Scale;

            return new DisplayInfo(
                width: bounds.Width * scale,
                height: bounds.Height * scale,
                density: scale,
                orientation: CalculateOrientation(),
                rotation: CalculateRotation(),
                rate: Platform.HasOSVersion(10, 3) ? UIScreen.MainScreen.MaximumFramesPerSecond : 0);
        }

        static void StartScreenMetricsListeners()
        {
            var notificationCenter = NSNotificationCenter.DefaultCenter;
#pragma warning disable CA1422
            var notification = UIApplication.DidChangeStatusBarOrientationNotification;
#pragma warning restore CA1422
            observer = notificationCenter.AddObserver(notification, OnScreenMetricsChanged);
        }

        static void StopScreenMetricsListeners()
        {
            observer?.Dispose();
            observer = null;
        }

        static void OnScreenMetricsChanged(NSNotification obj)
        {
            var metrics = GetMainDisplayInfo();
            OnMainDisplayInfoChanged(metrics);
        }

        static DisplayOrientation CalculateOrientation()
        {
#pragma warning disable CA1422
            var orientation = UIApplication.SharedApplication.StatusBarOrientation;
#pragma warning restore CA1422

            if (orientation.IsLandscape())
                return DisplayOrientation.Landscape;

            return DisplayOrientation.Portrait;
        }

        static DisplayRotation CalculateRotation()
        {
#pragma warning disable CA1422
            var orientation = UIApplication.SharedApplication.StatusBarOrientation;
#pragma warning restore CA1422

            switch (orientation)
            {
                case UIInterfaceOrientation.Portrait:
                    return DisplayRotation.Rotation0;
                case UIInterfaceOrientation.PortraitUpsideDown:
                    return DisplayRotation.Rotation180;
                case UIInterfaceOrientation.LandscapeLeft:
                    return DisplayRotation.Rotation270;
                case UIInterfaceOrientation.LandscapeRight:
                    return DisplayRotation.Rotation90;
            }

            return DisplayRotation.Unknown;
        }
    }
}
