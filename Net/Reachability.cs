// Copyright 2016 Ruben Buniatyan
// Licensed under the MIT License. For full terms, see LICENSE in the project root.

using System;
using System.Diagnostics;
using System.Net;
using SystemConfiguration;
using CoreFoundation;

namespace MissinKit.Net
{
    public class Reachability
    {
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

            NetReachability = new NetworkReachability(hostAddress);

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

            NetReachability = new NetworkReachability(hostName);

            Initialize();
        }

        private void Initialize()
        {
            NetReachability.Schedule(CFRunLoop.Current, CFRunLoop.ModeDefault);
            NetReachability.SetNotification(flags => RaiseReachabilityChangedEvent());
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the default route reachability. Should be used by applications that do not connect to a particular host.
        /// </summary>
        public static Reachability Default { get; } = new Reachability(new IPAddress(0));

        /// <summary>
        /// Gets a value indicating whether a connection is required to reach the host.
        /// </summary>
        /// <remarks>
        /// WWAN may be available, but not active until a connection has been established.
        /// WLAN may require a connection for VPN on Demand.
        /// </remarks>
        public bool IsConnectionRequired => NetReachability.TryGetFlags(out var flags) && (flags & NetworkReachabilityFlags.ConnectionRequired) != 0;

        protected NetworkReachability NetReachability { get; }

        /// <summary>
        /// Gets the reachability status of the host specified.
        /// </summary>
        public ReachabilityStatus Status => NetReachability.TryGetFlags(out var flags) ? GetReachabilityStatus(flags) : ReachabilityStatus.Unreachable;
        #endregion

        protected static ReachabilityStatus GetReachabilityStatus(NetworkReachabilityFlags flags)
        {
            Debug.WriteLine("Reachability flags: {0}{1}{2}{3}{4}{5}{6}{7}{8}",
                (flags & NetworkReachabilityFlags.IsWWAN) != 0 ? 'W' : '-',
                (flags & NetworkReachabilityFlags.Reachable) != 0 ? 'R' : '-',
                (flags & NetworkReachabilityFlags.TransientConnection) != 0 ? 't' : '-',
                (flags & NetworkReachabilityFlags.ConnectionRequired) != 0 ? 'c' : '-',
                (flags & NetworkReachabilityFlags.ConnectionOnTraffic) != 0 ? 'C' : '-',
                (flags & NetworkReachabilityFlags.InterventionRequired) != 0 ? 'i' : '-',
                (flags & NetworkReachabilityFlags.ConnectionOnDemand) != 0 ? 'D' : '-',
                (flags & NetworkReachabilityFlags.IsLocalAddress) != 0 ? 'l' : '-',
                (flags & NetworkReachabilityFlags.IsDirect) != 0 ? 'd' : '-');

            if ((flags & NetworkReachabilityFlags.Reachable) == 0)
                return ReachabilityStatus.Unreachable;

            if ((flags & NetworkReachabilityFlags.IsWWAN) != 0)
                return ReachabilityStatus.ReachableViaWwan;

            if ((flags & NetworkReachabilityFlags.ConnectionRequired) == 0 || // if no connection is required
                                                                              // or the connection is on-demand or on-traffic
                ((flags & NetworkReachabilityFlags.ConnectionOnDemand) != 0 || (flags & NetworkReachabilityFlags.ConnectionOnTraffic) != 0) &&
                (flags & NetworkReachabilityFlags.InterventionRequired) == 0) // and no user intervention is required
                return ReachabilityStatus.ReachableViaWlan;

            return ReachabilityStatus.Unreachable;
        }

        protected virtual void RaiseReachabilityChangedEvent()
        {
            var handler = ReachabilityChanged;

            handler?.Invoke(this, EventArgs.Empty);
        }

        #region Events
        /// <summary>
        /// Occurs when the reachability of the specified host is changed.
        /// </summary>
        public event EventHandler ReachabilityChanged;
        #endregion
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
        /// The specified host name or address can be reached via a cellular connection, such as LTE or GPRS.
        /// </summary>
        ReachableViaWwan
    }
}
