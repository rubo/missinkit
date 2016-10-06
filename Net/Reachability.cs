// Copyright 2016 Ruben Buniatyan
// Licensed under the MIT License. For full terms, see LICENSE in the project root.

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
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating whether a connection is required to reach the host.
        /// </summary>
        /// <remarks>
        /// WWAN may be available, but not active until a connection has been established.
        /// WLAN may require a connection for VPN on Demand.
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
        public ReachabilityStatus Status
        {
            get
            {
                NetworkReachabilityFlags flags;

                return HostReachability.TryGetFlags(out flags) ? GetReachabilityStatus(flags) : ReachabilityStatus.Unreachable;
            }
        }
        #endregion

        protected static ReachabilityStatus GetReachabilityStatus(NetworkReachabilityFlags flags)
        {
            if ((flags & NetworkReachabilityFlags.Reachable) == 0)
                return ReachabilityStatus.Unreachable;

            if ((flags & NetworkReachabilityFlags.IsWWAN) != 0)
                return ReachabilityStatus.ReachableViaWwan;

            if ((flags & NetworkReachabilityFlags.ConnectionRequired) == 0 ||
                (((flags & NetworkReachabilityFlags.ConnectionOnDemand) != 0 ||
                  (flags & NetworkReachabilityFlags.ConnectionOnTraffic) != 0) &&
                 (flags & NetworkReachabilityFlags.InterventionRequired) == 0))
                return ReachabilityStatus.ReachableViaWlan;

            return ReachabilityStatus.Unreachable;
        }

        protected virtual void RaiseReachabilityChangedEvent()
        {
            var handler = ReachabilityChanged;

            handler?.Invoke(this, EventArgs.Empty);
        }
    }

    public enum ReachabilityStatus
    {
        /// <summary>
        /// The specified host name or address cannot be reached.
        /// </summary>
        Unreachable,
        /// <summary>
        /// The specified host name or address can be reached via a wireless LAN, such as Wi-Fi.
        /// </summary>
        ReachableViaWlan,
        /// <summary>
        /// The specified host name or address can be reached via a cellular connection, such as EDGE or GPRS.
        /// </summary>
        ReachableViaWwan
    }
}
