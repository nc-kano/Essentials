﻿#if __IOS__
using CoreTelephony;
#endif
using System;
using System.Collections.Generic;

namespace Xamarin.Essentials
{
    public static partial class Connectivity
    {
#if __IOS__
#pragma warning disable CA1416
        static readonly Lazy<CTCellularData> cellularData = new Lazy<CTCellularData>(() => new CTCellularData());
#pragma warning restore CA1416

#pragma warning disable CA1416
        internal static CTCellularData CellularData => cellularData.Value;
#pragma warning restore CA1416
#endif

        static ReachabilityListener listener;

        static void StartListeners()
        {
            listener = new ReachabilityListener();
            listener.ReachabilityChanged += OnConnectivityChanged;
        }

        static void StopListeners()
        {
            if (listener == null)
                return;

            listener.ReachabilityChanged -= OnConnectivityChanged;
            listener.Dispose();
            listener = null;
        }

        static NetworkAccess PlatformNetworkAccess
        {
            get
            {
                var restricted = false;
#if __IOS__
#pragma warning disable CA1416
                restricted = CellularData.RestrictedState == CTCellularDataRestrictedState.Restricted;
#pragma warning restore CA1416
#endif
                var internetStatus = Reachability.InternetConnectionStatus();
                if ((internetStatus == NetworkStatus.ReachableViaCarrierDataNetwork && !restricted) || internetStatus == NetworkStatus.ReachableViaWiFiNetwork)
                    return NetworkAccess.Internet;

                var remoteHostStatus = Reachability.RemoteHostStatus();
                if ((remoteHostStatus == NetworkStatus.ReachableViaCarrierDataNetwork && !restricted) || remoteHostStatus == NetworkStatus.ReachableViaWiFiNetwork)
                    return NetworkAccess.Internet;

                return NetworkAccess.None;
            }
        }

        static IEnumerable<ConnectionProfile> PlatformConnectionProfiles
        {
            get
            {
                var statuses = Reachability.GetActiveConnectionType();
                foreach (var status in statuses)
                {
                    switch (status)
                    {
                        case NetworkStatus.ReachableViaCarrierDataNetwork:
                            yield return ConnectionProfile.Cellular;
                            break;
                        case NetworkStatus.ReachableViaWiFiNetwork:
                            yield return ConnectionProfile.WiFi;
                            break;
                        default:
                            yield return ConnectionProfile.Unknown;
                            break;
                    }
                }
            }
        }
    }
}
