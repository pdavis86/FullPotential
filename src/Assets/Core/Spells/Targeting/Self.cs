using System;

namespace Assets.Core.Spells.Targeting
{
    public class Self : ISpellTargeting
    {
        public Guid TypeId => new Guid("b7cd2ff2-e054-4955-bde9-38b3b6b9a1bf");

        public string TypeName => nameof(Self);

        public bool HasShape => true;
    }
}
