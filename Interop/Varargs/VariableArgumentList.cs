// Copyright 2016 Ruben Buniatyan
// This source is subject to the license agreement accompanying it.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MissinKit.Interop.Varargs
{
    public sealed class VariableArgumentList : IDisposable
    {
        #region Fields
        private IList<VariableArgument> _args;
        private bool _disposed;
        #endregion

        public VariableArgumentList(IList<VariableArgument> args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            _args = args;

            Handle = Marshal.AllocHGlobal(_args.Sum(a => a.Size));

            var ptr = Handle;

            foreach (var arg in _args)
            {
                arg.CopyTo(ptr);

                ptr += arg.Size;
            }
        }

        #region Dispose
        ~VariableArgumentList()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                foreach (var arg in _args)
                    arg.Dispose();

                _args = null;
            }

            Marshal.FreeHGlobal(Handle);

            Handle = IntPtr.Zero;

            _disposed = true;
        }
        #endregion

        public IntPtr Handle { get; private set; }
    }
}
