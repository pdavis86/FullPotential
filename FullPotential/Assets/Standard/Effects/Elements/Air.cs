using System;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Elements;

namespace FullPotential.Standard.Effects.Elements
{
    public class Air : IElement
    {
        public Guid TypeId => new Guid("957fb695-c894-496b-b8ec-0b89691a5481");

        public string TypeName => nameof(Air);

        public Affect Affect => Affect.Elemental;

        public string LingeringTypeName => "Suffocate";

        public Type Opposite => typeof(Earth);
    }
}
