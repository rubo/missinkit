// Copyright 2016 Ruben Buniatyan
// Licensed under the MIT License. For full terms, see LICENSE in the project root.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MissinKit.Interop.VariadicArguments
{
    public sealed class VariadicArgumentList : IDisposable
    {
        #region Fields
        private IList<VariadicArgument> _args;
        private bool _disposed;
        #endregion

        public VariadicArgumentList(IList<VariadicArgument> args)
        {
            _args = args ?? throw new ArgumentNullException(nameof(args));

            Handle = Marshal.AllocHGlobal(_args.Sum(a => a.Size));

            var ptr = Handle;

            foreach (var arg in _args)
            {
                arg.CopyTo(ptr);

                ptr += arg.Size;
            }
        }

        #region Dispose
        ~VariadicArgumentList()
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
