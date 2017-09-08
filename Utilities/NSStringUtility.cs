// Copyright 2016 Ruben Buniatyan
// Licensed under the MIT License. For full terms, see LICENSE in the project root.

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

        /// <summary>
        /// Returns an <see cref="NSString"/> object initialized by using a given format string as a template
        /// into which the remaining argument values are substituted without any localization.
        /// </summary>
        /// <param name="format">A format string. This value must not be <code>null</code>.</param>
        /// <param name="args">A list of arguments to substitute into <code>format</code>.</param>
        /// <returns>
        /// An <see cref="NSString"/> object initialized by using <code>format</code> as a template
        /// into which the values in <code>args</code> are substituted according to the current locale.
        /// The returned object may be different from the original receiver.
        /// </returns>
        public static string LocalizedFormat(NSString format, params VariadicArgument[] args)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            if (args == null)
                throw new ArgumentNullException(nameof(args));

            // Falls back to return the format itself to prevent runtime error on x86_64 systems
            if (Runtime.Arch == Arch.SIMULATOR && IntPtr.Size == 8)
                return format; 

            using (var str = NSObject.Alloc(new Class(typeof(NSString))))
            using (var varargs = new VariadicArgumentList(args))
                return NSString.FromHandle(NSStringInitWithFormatArguments(str.Handle, InitWithFormatArgumentsHandle, format.Handle, varargs.Handle));
        }

        /// <summary>
        /// Returns a localized version of the string designated by the specified key and residing in the table in Localizable.strings.
        /// </summary>
        /// <param name="bundle"></param>
        /// <param name="key">The key for a string in the table.</param>
        /// <returns>
        /// A localized version of the string designated by key in the table.
        /// This method returns the following when key is <code>null</code> or not found in table:
        /// <list type="bullet">
        /// <item>
        /// <description>If <code>key</code> is <code>null</code>, returns an empty string.</description>
        /// </item>
        /// <item>
        /// <description>If <code>key</code> is not found, returns <code>key</code>.</description>
        /// </item>
        /// </list>
        /// </returns>
        public static NSString LocalizedNSString(this NSBundle bundle, string key)
        {
            return LocalizedNSString(bundle, key, null, null);
        }

        /// <summary>
        /// Returns a localized version of the string designated by the specified key and residing in the specified table.
        /// </summary>
        /// <param name="bundle"></param>
        /// <param name="key">The key for a string in the table identified by <code>table</code>.</param>
        /// <param name="value">The value to return if key is <code>null</code> or if a localized string for key can't be found in the table.</param>
        /// <param name="table">The receiver's string table to search.
        /// If <code>table</code> is <code>null</code> or is an empty string, the method attempts to use the table in Localizable.strings.</param>
        /// <returns>
        /// A localized version of the string designated by key in <code>table</code>.
        /// This method returns the following when key is <code>null</code> or not found in table:
        /// <list type="bullet">
        /// <item>
        /// <description>If <code>key</code> is <code>null</code> and <code>value</code> is <code>null</code>, returns an empty string.</description>
        /// </item>
        /// <item>
        /// <description>If <code>key</code> is <code>null</code> and <code>value</code> is non-<code>null</code>, returns <code>value</code>.</description>
        /// </item>
        /// <item>
        /// <description>If <code>key</code> is not found and <code>value</code> is <code>null</code> or an empty string, returns <code>key</code>.</description>
        /// </item>
        /// <item>
        /// <description>If <code>key</code> is not found and <code>value</code> is non-<code>null</code> and not empty, return <code>value</code>.</description>
        /// </item>
        /// </list>
        /// </returns>
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

        #region Objective-C Bindings
        [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
        private static extern IntPtr NSBundleLocalizedStringForKeyValueTable(IntPtr target, IntPtr selector, IntPtr key, IntPtr value, IntPtr table);

        [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
        private static extern IntPtr NSStringInitWithFormatArguments(IntPtr target, IntPtr selector, IntPtr key, IntPtr args);
        #endregion
    }
}
