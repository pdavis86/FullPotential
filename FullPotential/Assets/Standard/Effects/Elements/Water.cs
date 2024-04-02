using System;
using FullPotential.Api.Registry.Elements;

namespace FullPotential.Standard.Effects.Elements
{
    public class Water : IElement
    {
        public Guid TypeId => new Guid("a1bdf7d3-6f21-48d7-9fc8-15ee94435922");

        public Type Opposite => typeof(Lightning);
    }
}
