using System;
using FullPotential.Api.Registry.Elements;

namespace FullPotential.Standard.Effects.Elements
{
    public class Air : IElementType
    {
        private static readonly Guid Id = new Guid("957fb695-c894-496b-b8ec-0b89691a5481");

        public Guid TypeId => Id;

        public Type Opposite => typeof(Earth);
    }
}
