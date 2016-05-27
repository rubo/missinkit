// Copyright 2016 Ruben Buniatyan
// Licensed under the MIT License. For full terms, see LICENSE in the project root.

using System;
using System.Runtime.InteropServices;

namespace MissinKit.Interop.VariadicArguments
{
    public sealed class StringArgument : VariadicArgument
    {
        private bool _disposed;
        private IntPtr _handle;
        private readonly string _value;

        public StringArgument(string value)
        {
            _value = value;
        }

        ~StringArgument()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            Marshal.FreeHGlobal(_handle);

            _disposed = true;

            base.Dispose(disposing);
        }

        protected internal override void CopyTo(IntPtr ptr)
        {
            _handle = Marshal.StringToHGlobalAuto(_value);

            Marshal.Copy(new [] { _handle }, 0, ptr, 1);
        }
    }
}
