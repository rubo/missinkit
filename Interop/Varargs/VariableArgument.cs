// Copyright 2016 Ruben Buniatyan
// This source is subject to the license agreement accompanying it.

using System;

namespace MissinKit.Interop.Varargs
{
    public abstract class VariableArgument : IDisposable
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
        public static implicit operator VariableArgument(double arg)
        {
            return new DoubleArgument(arg);
        }

        public static implicit operator VariableArgument(int arg)
        {
            return nint.Size == sizeof(long)
                ? new Int64Argument(arg)
                : (VariableArgument) new Int32Argument(arg);
        }

        public static implicit operator VariableArgument(long arg)
        {
            return new Int64Argument(arg);
        }

        public static implicit operator VariableArgument(nfloat arg)
        {
            return new DoubleArgument(arg);
        }

        public static implicit operator VariableArgument(nint arg)
        {
            return nint.Size == sizeof(long)
                ? new Int64Argument(arg)
                : (VariableArgument) new Int32Argument((int) arg);
        }

        public static implicit operator VariableArgument(string arg)
        {
            return new StringArgument(arg);
        }
        #endregion
    }
}
