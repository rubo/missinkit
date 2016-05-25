// Copyright 2016 Ruben Buniatyan
// This source is subject to the license agreement accompanying it.

using System;
using System.Runtime.InteropServices;

namespace MissinKit.Interop.Varargs
{
    public sealed class Int32Argument : VariableArgument
    {
        private readonly int _value;

        public Int32Argument(int value)
        {
            _value = value;
        }

        protected internal override void CopyTo(IntPtr ptr)
        {
            Marshal.Copy(new [] { _value }, 0, ptr, 1);
        }

        public override int Size { get; } = sizeof(int);
    }
}
