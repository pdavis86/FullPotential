using System;

using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Accessories.AutoAmmoBuyer
{
    public class AutoAmmoBuyer : IAccessoryType
    {
        public const string TypeIdString = "0d6f6511-352d-4303-9c25-b7b21c34ec59";

        public Guid TypeId { get; } = new Guid(TypeIdString);

        public string SlotSpritePrefabAddress => "Standard/UI/Equipment/AutoAmmoBuyer.png";

        public int SlotCount => 1;
    }
}
