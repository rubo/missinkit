// Copyright 2016 Ruben Buniatyan
// This source is subject to the license agreement accompanying it.

using System;

namespace MissinKit.Interop.VariadicArguments
{
    public abstract class VariadicArgument : IDisposable
    {
        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
        #endregion

        public virtual int Size { get; } = IntPtr.Size;

        protected internal abstract void CopyTo(IntPtr ptr);

        #region Operators
        public static implicit operator VariadicArgument(double arg)
        {
            return new DoubleArgument(arg);
        }

        public static implicit operator VariadicArgument(int arg)
        {
            return nint.Size == sizeof(long)
                ? new Int64Argument(arg)
                : (VariadicArgument) new Int32Argument(arg);
        }

        public static implicit operator VariadicArgument(long arg)
        {
            return new Int64Argument(arg);
        }

        public static implicit operator VariadicArgument(nfloat arg)
        {
            return new DoubleArgument(arg);
        }

        public static implicit operator VariadicArgument(nint arg)
        {
            return nint.Size == sizeof(long)
                ? new Int64Argument(arg)
                : (VariadicArgument) new Int32Argument((int) arg);
        }

        public static implicit operator VariadicArgument(string arg)
        {
            return new StringArgument(arg);
        }
        #endregion
    }
}
