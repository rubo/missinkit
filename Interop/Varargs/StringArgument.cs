// Copyright 2016 Ruben Buniatyan
// This source is subject to the license agreement accompanying it.

using System;
using System.Runtime.InteropServices;

namespace MissinKit.Interop.Varargs
{
    public sealed class StringArgument : VariableArgument
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
