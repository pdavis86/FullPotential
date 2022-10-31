using System.Linq;
using System.Text;
using FullPotential.Api.Gameplay.Enums;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry.Base;
using FullPotential.Api.Utilities.Extensions;

namespace FullPotential.Api.Registry.SpellsAndGadgets
{
    [System.Serializable]
    public abstract class SpellOrGadgetItemBase : ItemBase, ISpellOrGadget
    {
        public string TargetingTypeName;
        public string ShapeTypeName;

        public ResourceConsumptionType ResourceConsumptionType { get; protected set; }

        public int ChargePercentage { get; set; }

        private ITargeting _targeting;
        public ITargeting Targeting
        {
            get
            {
                return _targeting;
            }
            set
            {
                _targeting = value;
                TargetingTypeName = _targeting?.TypeName;
            }
        }

        private IShape _shape;
        public IShape Shape
        {
            get
            {
                return _shape;
            }
            set
            {
                _shape = value;
                ShapeTypeName = _shape?.TypeName;
            }
        }

        public int GetResourceCost()
        {
            var returnValue = GetValueInRangeHighLow(Attributes.Efficiency, 5, 50);
            //Debug.Log("GetResourceCost: " + returnValue);
            return (int)returnValue;
        }

        public float GetContinuousRange()
        {
            var returnValue = Attributes.Range / 100f * 10;
            //Debug.Log("GetContinuousRange: " + returnValue);
            return returnValue;
        }

        public float GetProjectileSpeed()
        {
            var castSpeed = Attributes.Speed / 50f;
            var returnValue = castSpeed < 0.5
                ? 0.5f
                : castSpeed;
            //Debug.Log("GetProjectileSpeed: " + returnValue);
            return returnValue;
        }

        public float GetChargeTime()
        {
            var returnValue = GetValueInRangeHighLow(Attributes.Speed, 0, 2);
            //Debug.Log("GetSogChargeTime: " + returnValue);
            return returnValue;
        }

        public float GetCooldownTime()
        {
            var returnValue = GetValueInRangeHighLow(Attributes.Recovery, 0, 2);
            //Debug.Log("GetSogChargeTime: " + returnValue);
            return returnValue;
        }

        public override string GetDescription(ILocalizer localizer, bool showExtendedDetails = true, string itemName = null)
        {
            var sb = new StringBuilder();

            if (showExtendedDetails)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {itemName.OrIfNullOrWhitespace(Name)}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {GetType().Name}" + "\n");
            }

            if (Targeting != null) { sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Targeting))}: {localizer.GetTranslatedTypeName(Targeting)}\n"); }
            if (Shape != null) { sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Shape))}: {localizer.GetTranslatedTypeName(Shape)}\n"); }

            if (Effects != null && Effects.Count > 0)
            {
                var localisedEffects = Effects.Select(localizer.GetTranslatedTypeName);
                sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Effects))}: {string.Join(", ", localisedEffects)}\n");
            }

            AppendToDescription(sb, localizer, Attributes.IsSoulbound, nameof(Attributes.IsSoulbound));
            AppendToDescription(sb, localizer, Attributes.Strength, nameof(Attributes.Strength));
            AppendToDescription(sb, localizer, Attributes.Efficiency, nameof(Attributes.Efficiency));
            AppendToDescription(sb, localizer, Attributes.Range, nameof(Attributes.Range));
            AppendToDescription(sb, localizer, Attributes.Accuracy, nameof(Attributes.Accuracy));

            AppendToDescription(
                sb,
                localizer,
                Attributes.Speed,
                nameof(Attributes.Speed),
                nameof(Spell),
                RoundFloatForDisplay(GetChargeTime()),
                UnitsType.Time);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Recovery,
                nameof(Attributes.Recovery),
                nameof(Spell),
                RoundFloatForDisplay(GetCooldownTime()),
                UnitsType.Time);

            AppendToDescription(sb, localizer, Attributes.Duration, nameof(Attributes.Duration));

            return sb.ToString();
        }
    }
}
