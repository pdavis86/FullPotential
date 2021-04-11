namespace Assets.Core.Crafting.SpellTargeting
{
    public class Touch : ISpellTargeting
    {
        public string TypeName => nameof(Touch);
        public bool HasShape => true;
    }
}
