namespace FullPotential.Api.Registry.SpellsAndGadgets
{
    public interface ISpellOrGadget
    {
        ITargeting Targeting { get; }
        IShape Shape { get; }
    }
}
