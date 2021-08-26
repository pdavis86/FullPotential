using System;

namespace FullPotential.Assets.Core.Spells.Targeting
{
    public class Beam : ISpellTargeting
    {
        public Guid TypeId => new Guid("3c528d09-bd4f-4211-af17-d8721975fef1");

        public string TypeName => nameof(Beam);

        public bool HasShape => false;
    }
}
