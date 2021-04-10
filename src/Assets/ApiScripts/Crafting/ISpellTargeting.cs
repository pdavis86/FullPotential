using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ApiScripts.Crafting
{
    public interface ISpellTargeting : IEffect
    {
        bool HasShape { get; }


        // != Spell.TargetingOptions.Beam && targeting != Spell.TargetingOptions.Cone do not have a shape

        //public abstract class TargetingOptions
        //{
        //    public const string Self = "Self";
        //    public const string Touch = "Touch";
        //    public const string Projectile = "Projectile";
        //    public const string Beam = "Beam";
        //    public const string Cone = "Cone";

        //    public static readonly List<string> All = new List<string>
        //    {
        //        Self,
        //        Touch,
        //        Projectile,
        //        Beam,
        //        Cone
        //    };
        //}

    }
}
