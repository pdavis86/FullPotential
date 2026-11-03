using System;

using FullPotential.Api.Registry.Elements;

namespace FullPotential.Standard.Effects.Elements
{
    public class Water : IElementType
    {
        public Guid TypeId { get; } = new Guid("a1bdf7d3-6f21-48d7-9fc8-15ee94435922");

        public Type Opposite => typeof(Lightning);
    }
}
