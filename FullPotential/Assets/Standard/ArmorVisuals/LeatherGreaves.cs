using System;
using FullPotential.Api.Registry.Gear;
using FullPotential.Standard.Armor;

namespace FullPotential.Standard.ArmorVisuals
{
    public class LeatherGreaves : IArmorVisuals
    {
        public Guid TypeId => new Guid("4eda8bc2-6929-4ad6-a5e1-3103b2cbcdac");

        public string TypeName => nameof(LeatherGreaves);

        public string PrefabAddress => "Standard/Prefabs/Armor/Legs.prefab";

        public Guid ApplicableToTypeId => new Guid(Legs.TypeIdString);
    }
}
