// Copyright 2017 Ruben Buniatyan
// Licensed under the MIT License. For full terms, see LICENSE in the project root.

#pragma warning disable 1591

using System;
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

        public static void OverrideCurrentLocale(string localeId)
        {
            if (string.CompareOrdinal(localeId, _locale?.Identifier) == 0)
                return;

            _locale = NSLocale.FromLocaleIdentifier(localeId);

            var path = NSBundle.MainBundle.PathForResource(_locale.LanguageCode, "lproj");

            BundleForCurrentLocale = string.IsNullOrEmpty(path) ? NSBundle.MainBundle : NSBundle.FromPath(path);

            if (IsOverridden)
                return;

            var method = class_getClassMethod(NSLocale.CurrentLocale.ClassHandle, CurrentLocaleSelectorHandle);

            if (_genuineImp == IntPtr.Zero)
                _genuineImp = method_getImplementation(method);

            var block = new BlockLiteral();
            block.SetupBlock(CurrentLocaleTrampoline, null);

            var imp = imp_implementationWithBlock(ref block);

            method_setImplementation(method, imp);

            block.CleanupBlock();

            IsOverridden = true;
        }

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
        }

        public static NSBundle BundleForCurrentLocale { get; private set; } = NSBundle.MainBundle;

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
