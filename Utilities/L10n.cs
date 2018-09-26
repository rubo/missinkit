// Copyright 2017 Ruben Buniatyan
// Licensed under the MIT License. For full terms, see LICENSE in the project root.

#pragma warning disable 1591

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;

namespace MissinKit.Utilities
{
    public static class L10n
    {
        #region Fields
        private static readonly IntPtr CurrentLocaleSelectorHandle = Selector.GetHandle("currentLocale");
        private static readonly CurrentLocaleDelegate CurrentLocaleTrampoline = GetCurrentLocale;

        private static IntPtr _genuineImp = IntPtr.Zero;
        private static NSLocale _locale;
        #endregion

        /// <summary>
        /// Overrides <see cref="NSLocale.CurrentLocale"/> with a locale for the identifier specified.
        /// </summary>
        /// <param name="localeIdentifier">Locale identifier</param>
        /// <remarks>
        /// If the current locale is already overridden, it must be restored by
        /// invoking the <see cref="RestoreCurrentLocale"/> method before overriding again.
        /// </remarks>
        public static void OverrideCurrentLocale(string localeIdentifier)
        {
            if (localeIdentifier == null)
                throw new ArgumentNullException(nameof(localeIdentifier));

            if (localeIdentifier.Equals(_locale?.Identifier, StringComparison.OrdinalIgnoreCase))
                return;

            if (IsOverridden)
            {
                Debug.WriteLine($"{nameof(L10n)}: The current locale is already overridden. To override again, restore it.");

                return;
            }

            _locale = NSLocale.FromLocaleIdentifier(localeIdentifier);

            var path = NSBundle.MainBundle.PathForResource(_locale.LanguageCode, "lproj")
                ?? NSBundle.MainBundle.PathForResource("Base", "lproj");

            BundleForCurrentLocale = string.IsNullOrEmpty(path) ? NSBundle.MainBundle : NSBundle.FromPath(path);

            var method = class_getClassMethod(NSLocale.CurrentLocale.ClassHandle, CurrentLocaleSelectorHandle);

            if (_genuineImp == IntPtr.Zero)
                _genuineImp = method_getImplementation(method);

            var block = new BlockLiteral();
            block.SetupBlock(CurrentLocaleTrampoline, null);

            var imp = imp_implementationWithBlock(ref block);

            method_setImplementation(method, imp);

            block.CleanupBlock();

            IsOverridden = true;

            NSNotificationCenter.DefaultCenter.PostNotificationName(NSLocale.CurrentLocaleDidChangeNotification, null);
        }

        /// <summary>
        /// Restores <see cref="NSLocale.CurrentLocale"/> to its genuine value.
        /// </summary>
        public static void RestoreCurrentLocale()
        {
            if (!IsOverridden)
                return;

            var method = class_getClassMethod(NSLocale.CurrentLocale.ClassHandle, CurrentLocaleSelectorHandle);

            method_setImplementation(method, _genuineImp);

            _locale.Dispose();
            _locale = null;

            BundleForCurrentLocale = NSBundle.MainBundle;

            IsOverridden = false;

            NSNotificationCenter.DefaultCenter.PostNotificationName(NSLocale.CurrentLocaleDidChangeNotification, null);
        }

        /// <summary>
        /// Returns an <see cref="NSBundle"/> object corresponding to the bundle directory for the overridden locale.
        /// If the current locale is not overridden, returns <see cref="NSBundle.MainBundle"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="NSBundle"/> object corresponding to the bundle directory for the overridden locale.
        /// </returns>
        public static NSBundle BundleForCurrentLocale { get; private set; } = NSBundle.MainBundle;

        /// <summary>
        /// Gets a value indicating whether the current locale is overridden.
        /// </summary>
        public static bool IsOverridden { get; private set; }

        #region Swizzling Methods
        private delegate IntPtr CurrentLocaleDelegate();

        [MonoPInvokeCallback(typeof(CurrentLocaleDelegate))]
        private static IntPtr GetCurrentLocale() => _locale.Handle;
        #endregion

        #region Objective-C Bindings
        [DllImport(Constants.ObjectiveCLibrary)]
        private static extern IntPtr class_getClassMethod(IntPtr cls, IntPtr sel);

        [DllImport(Constants.ObjectiveCLibrary)]
        private static extern IntPtr imp_implementationWithBlock(ref BlockLiteral block);

        [DllImport(Constants.ObjectiveCLibrary)]
        private static extern IntPtr method_getImplementation(IntPtr method);

        [DllImport(Constants.ObjectiveCLibrary)]
        private static extern IntPtr method_setImplementation(IntPtr method, IntPtr imp);
        #endregion
    }
}
