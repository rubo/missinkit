// Copyright 2016 Ruben Buniatyan
// Licensed under the MIT License. For full terms, see LICENSE in the project root.

#pragma warning disable 1591

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

        public abstract object Value { get; }

        protected internal abstract void CopyTo(IntPtr ptr);

        #region Operators
        public static implicit operator VariadicArgument(char arg) => new CharArgument(arg);

        public static implicit operator VariadicArgument(decimal arg) => new DecimalArgument(arg);

        public static implicit operator VariadicArgument(double arg) => new DoubleArgument(arg);

        public static implicit operator VariadicArgument(float arg) => new SingleArgument(arg);

        public static implicit operator VariadicArgument(int arg) => new Int32Argument(arg);

        public static implicit operator VariadicArgument(long arg) => new Int64Argument(arg);

        public static implicit operator VariadicArgument(nfloat arg) => nfloat.Size == sizeof(double) ? new DoubleArgument(arg) : (VariadicArgument) new SingleArgument((float) arg);

        public static implicit operator VariadicArgument(nint arg) => nint.Size == sizeof(long) ? new Int64Argument(arg) : (VariadicArgument) new Int32Argument((int) arg);

        public static implicit operator VariadicArgument(string arg) => new StringArgument(arg);
        #endregion
    }
}
