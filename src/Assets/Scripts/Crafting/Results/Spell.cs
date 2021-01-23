using System.Collections.Generic;

namespace Assets.Scripts.Crafting.Results
{
    public class Spell : CraftableBase
    {
        public const string BuffRegen = "Regen";
        public const string BuffHaste = "Haste";
        public const string BuffCourage = "Courage";
        public const string BuffFocus = "Focus";
        public const string BuffStrengthen = "Strengthen";
        public const string BuffLifeTap = "LifeTap";
        public const string BuffManaTap = "ManaTap";

        public const string DebuffPoison = "Poison";
        public const string DebuffSlow = "Slow";
        public const string DebuffFear = "Fear";
        public const string DebuffDistract = "Distract";
        public const string DebuffWeaken = "Weaken";
        public const string DebuffLifeDrain = "LifeDrain";
        public const string DebuffManaDrain = "ManaDrain";

        public const string SupportHeal = "Heal";
        public const string SupportLeap = "Leap";
        public const string SupportBlink = "Blink";
        public const string SupportSoften = "Soften";
        public const string SupportAbsorb = "Absorb";
        public const string SupportDeflect = "Deflect";

        public const string DamageForce = "Force"; //todo: is this necessary?
        public const string DamageFire = "Fire";
        public const string DamageLightning = "Lightning";
        public const string DamageIce = "Ice";
        public const string DamageEarth = "Earth";
        public const string DamageWater = "Water";
        public const string DamageAir = "Air";

        public const string LingeringIgnition = "Ignition";
        public const string LingeringShock = "Shock";
        public const string LingeringFreeze = "Freeze";
        public const string LingeringImmobilise = "Immobilise";

        public const string TargetingSelf = "Self";
        public const string TargetingTouch = "Touch";
        public const string TargetingProjectile = "Projectile";
        public const string TargetingBeam = "Beam";
        public const string TargetingCone = "Cone";

        public const string ShapeZone = "Zone";
        public const string ShapeWall = "Wall";

        public static readonly Dictionary<string, string> BuffOpposites = new Dictionary<string, string> {
            { BuffRegen, DebuffPoison },
            { BuffHaste, DebuffSlow },
            { BuffCourage, DebuffFear },
            { BuffFocus, DebuffDistract },
            { BuffStrengthen, DebuffWeaken },
            { BuffLifeTap, DebuffLifeDrain },
            { BuffManaTap, DebuffManaDrain }
        };

        public string Targeting { get; set; }
        public string Shape { get; set; }
    }
}
