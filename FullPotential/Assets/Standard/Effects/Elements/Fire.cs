using System;
using FullPotential.Api.Registry.Elements;

namespace FullPotential.Standard.Effects.Elements
{
    public class Fire : IElement
    {
        public Guid TypeId => new Guid("82657c2f-5402-48ff-a3a8-0c0c16346289");

        public string TypeName => nameof(Fire);

        public string LingeringTypeName => "Ignition";

        public Type Opposite => typeof(Ice);
    }
}
