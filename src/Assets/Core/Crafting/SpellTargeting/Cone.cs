namespace Assets.Core.Crafting.SpellTargeting
{
    public class Cone : ISpellTargeting
    {
        public string TypeName => nameof(Cone);
        public bool HasShape => false;
    }
}
