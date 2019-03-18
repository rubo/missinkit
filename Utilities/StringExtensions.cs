// Copyright 2017 Ruben Buniatyan
// Licensed under the MIT License. For full terms, see LICENSE in the project root.

#pragma warning disable 1591

using System;
using Foundation;
using MissinKit.Interop.VariadicArguments;

namespace MissinKit.Utilities
{
    public static class StringExtensions
    {
        private const string NSLocalizedString = "__NSLocalizedString";

        /// <summary>
        /// Returns a localized string for the current key into which the remaining argument values are substituted.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="args">A list of arguments to substitute into the localized string for the current key.</param>
        /// <returns>
        /// A localized string for the current key in the default table of the main bundle
        /// into which the remaining argument values in <code>args</code> are substituted.
        /// </returns>
        public static string Localize(this string key, params object[] args) => Localize(key, null, L10n.BundleForCurrentLocale, null, args);

        /// <summary>
        /// Returns a localized string for the current key into which the remaining argument values are substituted.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="table">The name of the table containing the key-value pairs.</param>
        /// <param name="bundle">The bundle containing the strings file. This value must not be <code>null</code></param>
        /// <param name="value">The value to return if a localized string for key can't be found in the table.</param>
        /// <param name="args">A list of arguments to substitute into the localized string for the current key.</param>
        /// <returns>
        /// A localized string for the current key in <code>table</code> of <code>bundle</code>
        /// into which the remaining argument values in <code>args</code> are substituted.
        /// </returns>
        public static string Localize(this string key, string table, NSBundle bundle, string value, params object[] args)
        {
            if (bundle == null)
                throw new ArgumentNullException(nameof(bundle));

            var str = bundle.GetLocalizedString(key, value, table);

            if (args == null)
                return str;

            if (str.Class.Name != NSLocalizedString)
                return string.Format(str, args);

            var varargs = new VariadicArgument[args.Length];

            for (int i = 0, count = args.Length; i < count; i++)
            {
                var arg = args[i];
                var type = arg?.GetType() ?? typeof(string);

                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.SByte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                        varargs[i] = (int)arg;
                        break;

                    case TypeCode.Decimal:
                    case TypeCode.Double:
                    case TypeCode.Single:
                        varargs[i] = (double)arg;
                        break;

                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        varargs[i] = (long)arg;
                        break;

                    case TypeCode.String:
                        varargs[i] = arg?.ToString();
                        break;

                    case TypeCode.Object:
                        if (type == typeof(nfloat))
                            varargs[i] = (nfloat)arg;
                        else if (type == typeof(nint))
                            varargs[i] = (nint)arg;
                        else
                            varargs[i] = arg.ToString();
                        break;

                    default:
                        varargs[i] = arg.ToString();
                        break;
                }
            }

            return NSStringUtility.LocalizedFormat(str, varargs);
        }
    }
}
