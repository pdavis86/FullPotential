namespace Assets.ApiScripts.Crafting
{
    public interface IEffect : IRegisterable
    {
        //todo .Except(new[] { Spell.BuffEffects.LifeTap, Spell.BuffEffects.ManaTap })

        /// <summary>
        /// Set this to true if this status effect is a side effect of another effect
        /// </summary>
        bool IsSideEffect { get; }
    }
}
