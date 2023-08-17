using System;
using System.Diagnostics.CodeAnalysis;
#if __ANDROID_26__
using Android;
using Android.OS;
#endif

namespace Xamarin.Essentials
{
    [SuppressMessage("Interoperability", "CA1422:Validate platform compatibility", Justification = "Interoperability")]
    public static partial class Vibration
    {
        internal static bool IsSupported => true;

        static void PlatformVibrate(TimeSpan duration)
        {
            Permissions.EnsureDeclared<Permissions.Vibrate>();

            var time = (long)duration.TotalMilliseconds;
#if __ANDROID_26__
            if (Platform.HasApiLevelO)
            {
                Platform.Vibrator.Vibrate(VibrationEffect.CreateOneShot(time, VibrationEffect.DefaultAmplitude));
                return;
            }
#endif

#pragma warning disable CS0618 // Type or member is obsolete
            Platform.Vibrator.Vibrate(time);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        static void PlatformCancel()
        {
            Permissions.EnsureDeclared<Permissions.Vibrate>();

            Platform.Vibrator.Cancel();
        }
    }
}
