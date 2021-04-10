using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ApiScripts.Crafting
{
    public interface IEffectBuff : IEffect
    {
        //todo .Except(new[] { Spell.BuffEffects.LifeTap, Spell.BuffEffects.ManaTap })
        bool DebuffAutomaticallyAppliesToTarget { get; }




        //public abstract class BuffEffects
        //{
        //    public const string Regen = "Regen";
        //    public const string Haste = "Haste";
        //    public const string Courage = "Courage";
        //    public const string Focus = "Focus";
        //    public const string Strengthen = "Strengthen";
        //    public const string LifeTap = "LifeTap";
        //    public const string ManaTap = "ManaTap";

        //    public static readonly List<string> All = new List<string>
        //    {
        //        Regen,
        //        Haste,
        //        Courage,
        //        Focus,
        //        Strengthen
        //        //Not needed for loot generation: LifeTap,
        //        //Not needed for loot generation: ManaTap
        //    };
        //}

        //public abstract class DebuffEffects
        //{
        //    public const string Poison = "Poison";
        //    public const string Slow = "Slow";
        //    public const string Fear = "Fear";
        //    public const string Distract = "Distract";
        //    public const string Weaken = "Weaken";
        //    public const string LifeDrain = "LifeDrain";
        //    public const string ManaDrain = "ManaDrain";

        //    public static readonly List<string> All = new List<string>
        //    {
        //        Poison,
        //        Slow,
        //        Fear,
        //        Distract,
        //        Weaken,
        //        LifeDrain,
        //        ManaDrain
        //    };
        //}

        //public static readonly Dictionary<string, string> BuffOpposites = new Dictionary<string, string>
        //{
        //    { BuffEffects.Regen, DebuffEffects.Poison },
        //    { BuffEffects.Haste, DebuffEffects.Slow },
        //    { BuffEffects.Courage, DebuffEffects.Fear },
        //    { BuffEffects.Focus, DebuffEffects.Distract },
        //    { BuffEffects.Strengthen, DebuffEffects.Weaken },
        //    { BuffEffects.LifeTap, DebuffEffects.LifeDrain },
        //    { BuffEffects.ManaTap, DebuffEffects.ManaDrain }
        //};


    }
}
