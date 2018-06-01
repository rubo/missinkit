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
        private static readonly IntPtr InitWithFormatLocaleArgumentsHandle = Selector.GetHandle("initWithFormat:locale:arguments:");
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

            // Fallback to prevent runtime error on x86_64 systems
            if (Runtime.Arch == Arch.SIMULATOR && IntPtr.Size == 8)
                return format;

            using (var str = NSObject.Alloc(new Class(typeof(NSString))))
            using (var varargs = new VariadicArgumentList(args))
                return NSString.FromHandle(NSStringInitWithFormatLocaleArguments(
                    str.Handle, InitWithFormatLocaleArgumentsHandle, format.Handle, NSLocale.CurrentLocale.Handle, varargs.Handle));
        }

        #region Objective-C Bindings
        [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
        private static extern IntPtr NSStringInitWithFormatLocaleArguments(IntPtr target, IntPtr selector, IntPtr format, IntPtr locale, IntPtr args);
        #endregion
    }
}
