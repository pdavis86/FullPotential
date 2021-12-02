using System;

namespace FullPotential.Core.Spells.Targeting
{
    public class Touch : ISpellTargeting
    {
        public Guid TypeId => new Guid("144cc142-2e64-476f-b3a6-de57cc3abd05");

        public string TypeName => nameof(Touch);

        public bool HasShape => true;
    }
}
