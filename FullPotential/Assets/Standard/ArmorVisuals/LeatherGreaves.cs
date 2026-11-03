using System;

using FullPotential.Api.Registry.Visuals;

using FullPotential.Standard.Armor;

namespace FullPotential.Standard.ArmorVisuals
{
    public class LeatherGreaves : IItemVisualsType
    {
        public Guid TypeId { get; } = new Guid("4eda8bc2-6929-4ad6-a5e1-3103b2cbcdac");

        public string PrefabAddress => "Standard/Prefabs/Armor/Legs.prefab";

        public string ApplicableToTypeIdString => Legs.TypeIdString;
    }
}
