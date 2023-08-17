using System;
using System.Threading.Tasks;
using Foundation;
using SafariServices;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class Browser
    {
        static async Task<bool> PlatformOpenAsync(Uri uri, BrowserLaunchOptions options)
        {
            switch (options.LaunchMode)
            {
                case BrowserLaunchMode.SystemPreferred:
                    var nativeUrl = new NSUrl(uri.AbsoluteUri);
#pragma warning disable CA1422
                    var sfViewController = new SFSafariViewController(nativeUrl, false);
#pragma warning restore CA1422
                    var vc = Platform.GetCurrentViewController();

                    if (options.PreferredToolbarColor.HasValue)
                        sfViewController.PreferredBarTintColor = options.PreferredToolbarColor.Value.ToPlatformColor();

                    if (options.PreferredControlColor.HasValue)
                        sfViewController.PreferredControlTintColor = options.PreferredControlColor.Value.ToPlatformColor();

                    if (sfViewController.PopoverPresentationController != null)
                        sfViewController.PopoverPresentationController.SourceView = vc.View;

                    if (options.HasFlag(BrowserLaunchFlags.PresentAsFormSheet))
                        sfViewController.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
                    else if (options.HasFlag(BrowserLaunchFlags.PresentAsPageSheet))
                        sfViewController.ModalPresentationStyle = UIModalPresentationStyle.PageSheet;

                    await vc.PresentViewControllerAsync(sfViewController, true);
                    break;
                case BrowserLaunchMode.External:
                    return await Launcher.PlatformOpenAsync(uri);
            }

            return true;
        }
    }
}
