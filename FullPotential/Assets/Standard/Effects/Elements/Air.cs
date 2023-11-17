using System;
using FullPotential.Api.Registry.Elements;

namespace FullPotential.Standard.Effects.Elements
{
    public class Air : IElement
    {
        public Guid TypeId => new Guid("957fb695-c894-496b-b8ec-0b89691a5481");

        //todo: zzz v0.5 Localize LingeringTypeNames
        public string LingeringTypeName => "Suffocate";

        public Type Opposite => typeof(Earth);
    }
}
