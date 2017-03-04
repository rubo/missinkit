// Copyright 2017 Ruben Buniatyan
// Licensed under the MIT License. For full terms, see LICENSE in the project root.

namespace MissinKit.Utilities
{
    public static class MachineEpsilon
    {
        /// <summary>
        /// Represents the smallest positive <see cref="double"/> value on ARM systems that is greater than zero. This field is constant.
        /// </summary>
        /// <seealso cref="double.Epsilon"/>
        public const double Double = 2.2250738585072014E-308;

        /// <summary>
        /// Represents the smallest positive <see cref="float"/> value on ARM systems that is greater than zero. This field is constant.
        /// </summary>
        /// <seealso cref="float.Epsilon"/>
        public const float Single = 1.175494351E-38F;
    }
}
