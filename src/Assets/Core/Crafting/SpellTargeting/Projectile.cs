namespace Assets.Core.Crafting.SpellTargeting
{
    public class Projectile : ISpellTargeting
    {
        public string TypeName => nameof(Projectile);
        public bool HasShape => true;
    }
}
