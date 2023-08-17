using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Photos;

namespace Xamarin.Essentials
{
    [SuppressMessage("Interoperability", "CA1422:Validate platform compatibility", Justification = "Interoperability")]
    public static partial class Permissions
    {
        [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Interoperability")]
        public partial class Photos : BasePlatformPermission
        {
            protected override Func<IEnumerable<string>> RequiredInfoPlistKeys => () =>
            {
                if (!Permissions.IsKeyDeclaredInInfoPlist("NSPhotoLibraryAddUsageDescription"))
                    Debug.WriteLine("You may need to set `NSPhotoLibraryAddUsageDescription` in your Info.plist file to use the Photo permission.");

                return new string[] { "NSPhotoLibraryUsageDescription" };
            };

            public override Task<PermissionStatus> CheckStatusAsync()
            {
                EnsureDeclared();

                return Task.FromResult(GetPhotoPermissionStatus());
            }

            public override Task<PermissionStatus> RequestAsync()
            {
                EnsureDeclared();

                var status = GetPhotoPermissionStatus();
                if (status == PermissionStatus.Granted)
                    return Task.FromResult(status);

                EnsureMainThread();

                return RequestPhotoPermission();
            }

            static PermissionStatus GetPhotoPermissionStatus()
            {
                var status = PHPhotoLibrary.AuthorizationStatus;
                return status switch
                {
                    PHAuthorizationStatus.Authorized => PermissionStatus.Granted,
                    PHAuthorizationStatus.Denied => PermissionStatus.Denied,
                    PHAuthorizationStatus.Restricted => PermissionStatus.Restricted,
                    _ => PermissionStatus.Unknown,
                };
            }

            static Task<PermissionStatus> RequestPhotoPermission()
            {
                var tcs = new TaskCompletionSource<PermissionStatus>();

                PHPhotoLibrary.RequestAuthorization(s =>
                {
                    switch (s)
                    {
                        case PHAuthorizationStatus.Authorized:
                            tcs.TrySetResult(PermissionStatus.Granted);
                            break;
                        case PHAuthorizationStatus.Denied:
                            tcs.TrySetResult(PermissionStatus.Denied);
                            break;
                        case PHAuthorizationStatus.Restricted:
                            tcs.TrySetResult(PermissionStatus.Restricted);
                            break;
                        default:
                            tcs.TrySetResult(PermissionStatus.Unknown);
                            break;
                    }
                });

                return tcs.Task;
            }
        }
    }
}
