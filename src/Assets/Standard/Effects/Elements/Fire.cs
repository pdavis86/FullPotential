using Assets.ApiScripts.Crafting;
using System;

namespace Assets.Standard.Effects.Elements
{
    public class Fire : IElement
    {
        public Guid TypeId => new Guid("82657c2f-5402-48ff-a3a8-0c0c16346289");

        public string TypeName => nameof(Fire);

        public bool IsSideEffect => false;

        public string LingeringTypeName => "Ignition";
    }
}
