// Copyright 2018 Ruben Buniatyan
// Licensed under the MIT License. For full terms, see LICENSE in the project root.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Foundation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MissinKit.Utilities
{
    /// <summary>
    /// Checks App Store for the current app updates.
    /// </summary>
    public class UpdateWatcher
    {
        #region Fields
        private const string LastCheckKey = "MKLastUpdateCheck";

        private readonly int _checkInterval;
        private readonly string _country;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the UpdateWatcher class with the specified check interval.
        /// </summary>
        /// <param name="checkInterval">Check interval expressed in days.</param>
        public UpdateWatcher(int checkInterval) : this(checkInterval, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the UpdateWatcher class with the specified check interval and country code.
        /// </summary>
        /// <param name="checkInterval">Check interval expressed in days.</param>
        /// <param name="country">The two-letter country code for the store to search.</param>
        public UpdateWatcher(int checkInterval, string country)
        {
            if (checkInterval < 0)
                throw new ArgumentOutOfRangeException(nameof(checkInterval), checkInterval, "Non-negative number required.");

            if (country != null && (string.IsNullOrWhiteSpace(country) || country.Length != 2))
                throw new ArgumentException("Country code must be in ISO 3166-1 alpha-2 format.", nameof(country));

            _checkInterval = checkInterval;
            _country = country?.ToLowerInvariant();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Checks App Store to determine whether a new update is available.
        /// </summary>
        /// <returns>
        /// <code>true</code> if a new update is available; otherwise, <code>false</code>.
        /// </returns>
        public virtual async Task<bool> CheckForUpdateAsync()
        {
            var timestamp = NSUserDefaults.StandardUserDefaults.IntForKey(LastCheckKey);
            var lastCheckDate = new DateTime(timestamp, DateTimeKind.Utc);

            if ((DateTime.UtcNow - lastCheckDate).TotalDays < _checkInterval)
                return false;

            var bundleId = Uri.EscapeDataString(NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleIdentifier").ToString());
            var url = $"https://itunes.apple.com/lookup?bundleId={bundleId}";

            if (_country != null)
                url += $"&country={Uri.EscapeDataString(_country)}";

            Stream stream = null;

            using (var httpClient = new HttpClient())
                try
                {
                    stream = await httpClient.GetStreamAsync(url).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

            if (stream == null)
                return false;

            using (stream)
            {
                using (var reader = new StreamReader(stream))
                using (var json = new JsonTextReader(reader))
                {
                    try
                    {
                        var serializer = JsonSerializer.CreateDefault();
                        var result = (JObject)serializer.Deserialize(json);
                        var r = result?["results"]?.FirstOrDefault();

                        if (r == null)
                            return false;

                        var version = r.Value<string>("version");
                        var storeVersion = new SemVer(version);

                        version = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString").ToString();

                        var currentVersion = new SemVer(version);

                        NSUserDefaults.StandardUserDefaults.SetInt((nint)DateTime.UtcNow.Ticks, LastCheckKey);

                        return storeVersion > currentVersion;
                    }
                    catch (JsonException ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }

            return false;
        }
        #endregion

        #region SemVer
        private class SemVer
        {
            private readonly int _major;
            private readonly int _minor;
            private readonly int _patch;
            private readonly string _version;

            public SemVer(string version)
            {
                _version = version ?? throw new ArgumentNullException(nameof(version));

                var v = version.Split('.');

                if (v.Length > 0)
                    int.TryParse(v[0], out _major);

                if (v.Length > 1)
                    int.TryParse(v[1], out _minor);

                if (v.Length > 2)
                    int.TryParse(v[2], out _patch);
            }

            public override bool Equals(object obj)
            {
                if (obj is null)
                    return false;

                if (ReferenceEquals(this, obj))
                    return true;

                return GetType() == obj.GetType() && Equals((SemVer)obj);
            }

            private bool Equals(SemVer v) => _major == v._major && _minor == v._minor && _patch == v._patch;

            public override int GetHashCode() => _version.GetHashCode();

            public override string ToString() => _version;

            public static bool operator ==(SemVer v1, SemVer v2) => v1?.Equals(v2) ?? v2 is null;

            public static bool operator !=(SemVer v1, SemVer v2) => !v1?.Equals(v2) ?? !(v2 is null);

            public static bool operator >(SemVer v1, SemVer v2)
            {
                if (v1 is null || v2 is null)
                    return false;

                if (v1._major > v2._major)
                    return true;

                return v1._major == v2._major && (v1._minor > v2._minor || v1._minor == v2._minor && v1._patch > v2._patch);
            }

            public static bool operator <(SemVer v1, SemVer v2)
            {
                if (v1 is null || v2 is null)
                    return false;

                if (v1._major < v2._major)
                    return true;

                return v1._major == v2._major && (v1._minor < v2._minor || v1._minor == v2._minor && v1._patch < v2._patch);
            }

            public static bool operator >=(SemVer v1, SemVer v2) => v1 == v2 || v1 > v2;

            public static bool operator <=(SemVer v1, SemVer v2) => v1 == v2 || v1 < v2;
        }
        #endregion
    }
}
