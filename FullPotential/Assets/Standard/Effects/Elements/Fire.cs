using System;
using FullPotential.Api.Registry.Elements;

namespace FullPotential.Standard.Effects.Elements
{
    public class Fire : IElementType
    {
        private static readonly Guid Id = new Guid("82657c2f-5402-48ff-a3a8-0c0c16346289");

        public Guid TypeId => Id;

        public Type Opposite => typeof(Ice);
    }
}
