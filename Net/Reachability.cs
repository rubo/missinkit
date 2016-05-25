﻿// Copyright 2016 Ruben Buniatyan
// This source is subject to the license agreement accompanying it.

using System;
using System.Net;
using SystemConfiguration;
using CoreFoundation;

namespace MissinKit.Net
{
    public class Reachability : IDisposable
    {
        #region Fields
        protected readonly NetworkReachability HostReachability;

        private bool _disposed;
        private bool _useLocalWifiStatus;
        #endregion

        #region Events
        /// <summary>
        /// Occurs when the reachability of the specified host is changed.
        /// </summary>
        public event EventHandler ReachabilityChanged;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the Reachability class with the specified IP address.
        /// </summary>
        /// <param name="hostAddress">The IP address of the host</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="hostAddress"/> is null.
        /// </exception>
        public Reachability(IPAddress hostAddress)
        {
            if (hostAddress == null)
                throw new ArgumentNullException(nameof(hostAddress));

            HostReachability = new NetworkReachability(hostAddress);

            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the Reachability class with the specified host name.
        /// </summary>
        /// <param name="hostName">The name of the host</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="hostName"/> is null.
        /// </exception>
        public Reachability(string hostName)
        {
            if (hostName == null)
                throw new ArgumentNullException(nameof(hostName));

            HostReachability = new NetworkReachability(hostName);

            Initialize();
        }

        private void Initialize()
        {
            HostReachability.Schedule(CFRunLoop.Current, CFRunLoop.ModeDefault);
            HostReachability.SetNotification(flags => RaiseReachabilityChangedEvent());
        }
        #endregion

        #region Destructors
        ~Reachability()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                HostReachability.Unschedule(CFRunLoop.Current, CFRunLoop.ModeDefault);

                if (disposing)
                    HostReachability.Dispose();

                _disposed = true;
            }
        }
        #endregion

        #region Static Properties
        /// <summary>
        /// Checks whether the default route is available. Should be used by applications that do not connect to a particular host.
        /// </summary>
        public static Reachability Internet { get; } = new Reachability(new IPAddress(0));

        /// <summary>
        /// Checks whether a local WiFi connection is available.
        /// </summary>
        public static Reachability LocalWifiNetwork { get; } = new Reachability(new IPAddress(0x0000FEA9)) { _useLocalWifiStatus = true };
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating whether a connection is required to reach the host.
        /// </summary>
        /// <remarks>
        /// WWAN may be available, but not active until a connection has been established.
        /// WiFi may require a connection for VPN on Demand.
        /// </remarks>
        public bool IsConnectionRequired
        {
            get
            {
                NetworkReachabilityFlags flags;

                return HostReachability.TryGetFlags(out flags) && (flags & NetworkReachabilityFlags.ConnectionRequired) != 0;
            }
        }

        /// <summary>
        /// Gets the reachability status of the host specified.
        /// </summary>
        public NetworkStatus Status
        {
            get
            {
                NetworkReachabilityFlags flags;

                return HostReachability.TryGetFlags(out flags)
                    ? _useLocalWifiStatus ? GetLocalWifiNetworkStatus(flags) : GetNetworkStatus(flags)
                    : NetworkStatus.NotReachable;
            }
        }
        #endregion

        protected static NetworkStatus GetLocalWifiNetworkStatus(NetworkReachabilityFlags flags)
        {
            return (flags & NetworkReachabilityFlags.Reachable) != 0 && (flags & NetworkReachabilityFlags.IsDirect) != 0
                ? NetworkStatus.ReachableViaWifi : NetworkStatus.NotReachable;
        }

        protected static NetworkStatus GetNetworkStatus(NetworkReachabilityFlags flags)
        {
            if ((flags & NetworkReachabilityFlags.Reachable) == 0)
                return NetworkStatus.NotReachable;

            if ((flags & NetworkReachabilityFlags.IsWWAN) != 0)
                return NetworkStatus.ReachableViaWwan;

            if ((flags & NetworkReachabilityFlags.ConnectionRequired) == 0 ||
                (((flags & NetworkReachabilityFlags.ConnectionOnDemand) != 0 ||
                  (flags & NetworkReachabilityFlags.ConnectionOnTraffic) != 0) &&
                 (flags & NetworkReachabilityFlags.InterventionRequired) == 0))
                return NetworkStatus.ReachableViaWifi;

            return NetworkStatus.NotReachable;
        }

        protected virtual void RaiseReachabilityChangedEvent()
        {
            var handler = ReachabilityChanged;

            handler?.Invoke(this, EventArgs.Empty);
        }
    }

    public enum NetworkStatus
    {
        NotReachable,
        ReachableViaWifi,
        ReachableViaWwan
    }
}
