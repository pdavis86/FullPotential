using System;
using FullPotential.Api.Registry.Elements;

namespace FullPotential.Standard.Effects.Elements
{
    public class Earth : IElement
    {
        public Guid TypeId => new Guid("533bfd89-696d-497c-9e7f-d0629bfbc0d0");

        public string TypeName => nameof(Earth);

        public bool IsSideEffect => false;

        public string LingeringTypeName => "Immobilise";
    }
}
