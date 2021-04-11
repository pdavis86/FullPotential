namespace Assets.Core.Crafting.SpellTargeting
{
    public class Self : ISpellTargeting
    {
        public string TypeName => nameof(Self);
        public bool HasShape => true;
    }
}
