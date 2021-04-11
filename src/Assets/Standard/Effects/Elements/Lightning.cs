using Assets.ApiScripts.Crafting;
using System;

namespace Assets.Standard.Effects.Elements
{
    public class Lightning : IElement
    {
        public Guid TypeId => new Guid("80f913d3-0969-42e1-8f32-8ac54c1b3203");

        public string TypeName => nameof(Lightning);

        public bool IsSideEffect => false;

        public string LingeringTypeName => "Shock";

        //opposite of water
    }
}
