// Copyright 2016 Ruben Buniatyan
// This source is subject to the license agreement accompanying it.

using System;
using System.Runtime.InteropServices;
using Foundation;
using MissinKit.Interop.VariadicArguments;
using ObjCRuntime;

namespace MissinKit.Utilities
{
    public static class NSStringUtility
    {
        #region Fields
        private static readonly IntPtr InitWithFormatArgumentsHandle = Selector.GetHandle("initWithFormat:arguments:");
        private static readonly IntPtr LocalizedStringForKeyValueTableHandle = Selector.GetHandle("localizedStringForKey:value:table:");
        #endregion

        public static string LocalizedFormat(NSString format, params VariadicArgument[] args)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            if (args == null)
                throw new ArgumentNullException(nameof(args));

            using (var str = NSObject.Alloc(new Class(typeof(NSString))))
            using (var varargs = new VariadicArgumentList(args))
                return NSString.FromHandle(NSStringInitWithFormatArguments(str.Handle, InitWithFormatArgumentsHandle, format.Handle, varargs.Handle));
        }

        public static NSString LocalizedNSString(this NSBundle bundle, string key)
        {
            return LocalizedNSString(bundle, key, null, null);
        }

        public static NSString LocalizedNSString(this NSBundle bundle, string key, string value, string table)
        {
            if (bundle == null)
                throw new ArgumentNullException(nameof(bundle));

            var keyHandle = key == null ? IntPtr.Zero : NSString.CreateNative(key);
            var valueHandle = value == null ? IntPtr.Zero : NSString.CreateNative(value);
            var tableHandle = table == null ? IntPtr.Zero : NSString.CreateNative(table);
            var localizedHandle = NSBundleLocalizedStringForKeyValueTable(bundle.Handle, LocalizedStringForKeyValueTableHandle,
                keyHandle, valueHandle, tableHandle);

            if (keyHandle != IntPtr.Zero)
                NSString.ReleaseNative(keyHandle);

            if (valueHandle != IntPtr.Zero)
                NSString.ReleaseNative(valueHandle);

            if (tableHandle != IntPtr.Zero)
                NSString.ReleaseNative(tableHandle);

            return Runtime.GetNSObject<NSString>(localizedHandle);
        }

        public static NSString ToNSString(this string str)
        {
            return (NSString) str;
        }

        #region Objective-C Bindings
        [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
        private static extern IntPtr NSBundleLocalizedStringForKeyValueTable(IntPtr target, IntPtr selector, IntPtr key, IntPtr value, IntPtr table);

        [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
        private static extern IntPtr NSStringInitWithFormatArguments(IntPtr target, IntPtr selector, IntPtr key, IntPtr args);
        #endregion
    }
}
