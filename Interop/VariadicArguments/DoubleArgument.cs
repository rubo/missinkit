// Copyright 2016 Ruben Buniatyan
// This source is subject to the license agreement accompanying it.

using System;
using System.Runtime.InteropServices;

namespace MissinKit.Interop.VariadicArguments
{
    public sealed class DoubleArgument : VariadicArgument
    {
        private readonly double _value;

        public DoubleArgument(double value)
        {
            _value = value;
        }

        protected internal override void CopyTo(IntPtr ptr)
        {
            Marshal.Copy(new[] { _value }, 0, ptr, 1);
        }

        public override int Size { get; } = sizeof(double);
    }
}
