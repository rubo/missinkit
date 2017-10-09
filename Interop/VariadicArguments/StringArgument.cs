// Copyright 2016 Ruben Buniatyan
// Licensed under the MIT License. For full terms, see LICENSE in the project root.

#pragma warning disable 1591

using System;
using System.Runtime.InteropServices;

namespace MissinKit.Interop.VariadicArguments
{
    public sealed class StringArgument : VariadicArgument
    {
        private bool _disposed;
        private IntPtr _handle;
        private readonly string _value;

        public StringArgument(string value) => _value = value;

        ~StringArgument() => Dispose(false);

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (_handle != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_handle);

                _handle = IntPtr.Zero;
            }

            _disposed = true;

            base.Dispose(disposing);
        }

        protected internal override void CopyTo(IntPtr ptr)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(StringArgument));

            _handle = Marshal.StringToHGlobalAuto(_value);

            Marshal.Copy(new [] { _handle }, 0, ptr, 1);
        }
    }
}
