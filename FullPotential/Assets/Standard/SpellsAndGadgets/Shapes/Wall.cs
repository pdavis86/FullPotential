using System;
using FullPotential.Api.Registry.Consumers;

namespace FullPotential.Standard.SpellsAndGadgets.Shapes
{
    public class Wall : IShape
    {
        public Guid TypeId => new Guid("4aad2866-5903-4b79-bda2-e3dcab920d9e");

        public string TypeName => nameof(Wall);

        public string PrefabAddress => "Standard/Prefabs/SpellOrGadget/Wall.prefab";
    }
}
