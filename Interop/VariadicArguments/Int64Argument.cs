// Copyright 2016 Ruben Buniatyan
// Licensed under the MIT License. For full terms, see LICENSE in the project root.

#pragma warning disable 1591

using System;
using System.Runtime.InteropServices;

namespace MissinKit.Interop.VariadicArguments
{
    public sealed class Int64Argument : VariadicArgument
    {
        private readonly long _value;

        public Int64Argument(long value) => _value = value;

        protected internal override void CopyTo(IntPtr ptr) => Marshal.Copy(new[] { _value }, 0, ptr, 1);

        public override int Size { get; } = sizeof(long);

        public override object Value => _value;
    }
}
