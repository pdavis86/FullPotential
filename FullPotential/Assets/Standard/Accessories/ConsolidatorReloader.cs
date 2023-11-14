using System;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Accessories
{
    public class ConsolidatorReloader : IAccessoryVisuals
    {
        public Guid TypeId => new Guid("575ed70f-f5de-4ffa-93fb-a6c1cc404f30");

        public string TypeName => nameof(ConsolidatorReloader);

        public string PrefabAddress => null;

        public AccessoryType Type => AccessoryType.Reloader;
    }
}
