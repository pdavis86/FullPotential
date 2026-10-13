using FullPotential.Api.Items.Base;

namespace FullPotential.Standard.Items.Base
{
    // todo: delete StandardItemBase?
    public abstract class StandardItemBase : CombatItemBase
    {
        //public override string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full)
        //{
        //    var sb = new StringBuilder();

        //    if (levelOfDetail == LevelOfDetail.Full)
        //    {
        //        sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {Name}" + "\n");
        //        sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {localizer.Translate(RegistryType)}" + "\n");
        //    }

        //    if (EffectTypes != null && EffectTypes.Count > 0)
        //    {
        //        var localisedEffects = EffectTypes.Select(localizer.Translate);
        //        sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(EffectTypes))}: {string.Join(", ", localisedEffects)}\n");
        //    }

        //    AppendToDescription(sb, localizer, Properties.GetBoolValue(nameof(Weapon.IsAutomatic)), nameof(Weapon.IsAutomatic));
        //    AppendToDescription(sb, localizer, Properties.GetIntValue(nameof(Weapon.ExtraAmmoPerShot)), nameof(Weapon.ExtraAmmoPerShot));

        //    AppendToDescription(sb, localizer, Attributes.GetIntValue(Strength.TypeId), nameof(Strength));
        //    AppendToDescription(sb, localizer, Attributes.GetIntValue(Efficiency.TypeId), nameof(Efficiency));
        //    AppendToDescription(sb, localizer, Attributes.GetIntValue(Range.TypeId), nameof(Range));
        //    AppendToDescription(sb, localizer, Attributes.GetIntValue(Accuracy.TypeId), nameof(Accuracy));
        //    AppendToDescription(sb, localizer, Attributes.GetIntValue(Speed.TypeId), nameof(Speed));
        //    AppendToDescription(sb, localizer, Attributes.GetIntValue(Recovery.TypeId), nameof(Recovery));
        //    AppendToDescription(sb, localizer, Attributes.GetIntValue(Duration.TypeId), nameof(Duration));
        //    AppendToDescription(sb, localizer, Attributes.GetIntValue(Luck.TypeId), nameof(Luck));

        //    return sb.ToString().Trim();
        //}
    }
}
