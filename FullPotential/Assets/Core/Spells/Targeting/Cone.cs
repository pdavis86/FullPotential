using System;

namespace FullPotential.Assets.Core.Spells.Targeting
{
    public class Cone : ISpellTargeting
    {
        public Guid TypeId => new Guid("677d6fdc-428b-4bdc-8537-ae390fca9843");

        public string TypeName => nameof(Cone);

        public bool HasShape => false;
    }
}
