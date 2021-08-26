using System;

namespace FullPotential.Assets.Core.Spells.Targeting
{
    public class Projectile : ISpellTargeting
    {
        public Guid TypeId => new Guid("6e41729e-bb21-44f8-8fb9-b9ad48c0e680");

        public string TypeName => nameof(Projectile);

        public bool HasShape => true;
    }
}
