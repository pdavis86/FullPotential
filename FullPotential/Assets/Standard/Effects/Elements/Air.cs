using FullPotential.Assets.Api.Registry;
using System;

namespace FullPotential.Assets.Standard.Effects.Elements
{
    public class Air : IElement
    {
        public Guid TypeId => new Guid("957fb695-c894-496b-b8ec-0b89691a5481");

        public string TypeName => nameof(Air);

        public bool IsSideEffect => false;

        public string LingeringTypeName => "Suffocate";
    }
}
