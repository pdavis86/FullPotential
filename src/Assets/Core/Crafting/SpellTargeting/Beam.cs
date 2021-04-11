namespace Assets.Core.Crafting.SpellTargeting
{
    public class Beam : ISpellTargeting
    {
        public string TypeName => nameof(Beam);
        public bool HasShape => false;
    }
}
