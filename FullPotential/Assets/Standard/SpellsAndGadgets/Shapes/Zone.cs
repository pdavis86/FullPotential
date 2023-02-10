using System;
using FullPotential.Api.Registry.SpellsAndGadgets;

namespace FullPotential.Standard.SpellsAndGadgets.Shapes
{
    public class Zone : IShape
    {
        public Guid TypeId => new Guid("142aeb3b-84b1-43c6-ae91-388b0901fa52");

        public string TypeName => nameof(Zone);

        public string PrefabAddress => "Standard/Prefabs/SpellOrGadget/Zone.prefab";
    }
}
