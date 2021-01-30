using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Crafting.Results
{
    public class Spell : CraftableBase
    {
        public string Targeting { get; set; }
        public string Shape { get; set; }



        public abstract class BuffEffects
        {
            public const string Regen = "Regen";
            public const string Haste = "Haste";
            public const string Courage = "Courage";
            public const string Focus = "Focus";
            public const string Strengthen = "Strengthen";
            public const string LifeTap = "LifeTap";
            public const string ManaTap = "ManaTap";

            public static readonly List<string> All = new List<string>
            {
                Regen,
                Haste,
                Courage,
                Focus,
                Strengthen
                //Not needed for loot generation: LifeTap,
                //Not needed for loot generation: ManaTap
            };
        }

        public abstract class DebuffEffects
        {
            public const string Poison = "Poison";
            public const string Slow = "Slow";
            public const string Fear = "Fear";
            public const string Distract = "Distract";
            public const string Weaken = "Weaken";
            public const string LifeDrain = "LifeDrain";
            public const string ManaDrain = "ManaDrain";

            public static readonly List<string> All = new List<string>
            {
                Poison,
                Slow,
                Fear,
                Distract,
                Weaken,
                LifeDrain,
                ManaDrain
            };
        }

        public static readonly Dictionary<string, string> BuffOpposites = new Dictionary<string, string> {
            { BuffEffects.Regen, DebuffEffects.Poison },
            { BuffEffects.Haste, DebuffEffects.Slow },
            { BuffEffects.Courage, DebuffEffects.Fear },
            { BuffEffects.Focus, DebuffEffects.Distract },
            { BuffEffects.Strengthen, DebuffEffects.Weaken },
            { BuffEffects.LifeTap, DebuffEffects.LifeDrain },
            { BuffEffects.ManaTap, DebuffEffects.ManaDrain }
        };

        public abstract class SupportEffects
        {
            public const string Heal = "Heal";
            public const string Leap = "Leap";
            public const string Blink = "Blink";
            public const string Soften = "Soften";
            public const string Absorb = "Absorb";
            public const string Deflect = "Deflect";

            public static readonly List<string> All = new List<string>
            {
                Heal,
                Leap,
                Blink,
                Soften,
                Absorb,
                Deflect
            };
        }

        public abstract class ElementalEffects
        {
            //todo: is this necessary?: public const string Force = "Force"; 
            public const string Fire = "Fire";
            public const string Lightning = "Lightning";
            public const string Ice = "Ice";
            public const string Earth = "Earth";
            public const string Water = "Water";
            public const string Air = "Air";

            public static readonly List<string> All = new List<string>
            {
                //Not needed for loot creation: Force,
                Fire,
                Lightning,
                Ice,
                Earth,
                Water,
                Air
            };
        }

        public abstract class LingeringOptions
        {
            public const string Ignition = "Ignition";
            public const string Shock = "Shock";
            public const string Freeze = "Freeze";
            public const string Immobilise = "Immobilise";

            public static readonly List<string> All = new List<string>
            {
                Ignition,
                Shock,
                Freeze,
                Immobilise
            };
        }

        public static readonly Dictionary<string, string> LingeringPairing = new Dictionary<string, string>
        {
            { ElementalEffects.Fire, LingeringOptions.Ignition },
            { ElementalEffects.Lightning, LingeringOptions.Shock },
            { ElementalEffects.Ice, LingeringOptions.Freeze },
            { ElementalEffects.Earth, LingeringOptions.Immobilise }
        };

        public abstract class TargetingOptions
        {
            public const string Self = "Self";
            public const string Touch = "Touch";
            public const string Projectile = "Projectile";
            public const string Beam = "Beam";
            public const string Cone = "Cone";

            public static readonly List<string> All = new List<string>
            {
                Self,
                Touch,
                Projectile,
                Beam,
                Cone
            };
        }

        public abstract class ShapeOptions
        {
            public const string Zone = "Zone";
            public const string Wall = "Wall";

            public static readonly List<string> All = new List<string>
            {
                Zone,
                Wall
            };
        }

        public static readonly List<string> LootEffectsAndOptions = 
            BuffEffects.All //Excluded from "All" property: .Except(new[] { BuffEffects.LifeTap, BuffEffects.ManaTap })
            .Union(DebuffEffects.All)
            .Union(SupportEffects.All)
            .Union(ElementalEffects.All)
            //Do not include as it needs pairing: .Union(_lingeringEffects)
            .Union(TargetingOptions.All)
            .Union(ShapeOptions.All)
            .ToList();

    }
}
