using System;
using System.Linq;
using System.Text;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Api.Utilities.Extensions;

namespace FullPotential.Api.Items.Types
{
    [Serializable]
    public class Weapon : ItemWithHealthBase, IHasChargeUpOrCooldown, IHasItemVisuals
    {
        public const float DefensiveWeaponDpsMultiplier = 0.1f;

        private const string AliasSegmentMelee = "MeleeWeapon";
        private const string AliasSegmentRanged = "RangedWeapon";

        private IItemVisuals _visuals;

        //Variables so they are serialized
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable NotAccessedField.Global
        public string WeaponVisualsTypeId;
        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore NotAccessedField.Global

        public int Ammo;

        public IWeapon WeaponType => (IWeapon)RegistryType;

        public bool IsRanged => WeaponType.AmmunitionTypeId != null;

        public bool IsMelee => WeaponType.AmmunitionTypeId == null;

        public bool IsDefensive => WeaponType.IsDefensive;

        public string VisualsTypeId => WeaponVisualsTypeId;

        public int ChargePercentage { get; set; }

        public IItemVisuals Visuals
        {
            get => _visuals;
            set
            {
                _visuals = value;
                WeaponVisualsTypeId = _visuals?.TypeId.ToString();
            }
        }

        public int GetAmmoMax()
        {
            const int ammoCap = 100;
            var returnValue = (int)(Attributes.Efficiency / 100f * ammoCap);
            return returnValue;
        }

        public float GetReloadTime()
        {
            var returnValue = GetHighInLowOutInRange(Attributes.Recovery, 0.5f, 5);
            return returnValue;
        }

        public float GetDelayBetweenShots()
        {
            //Roughly 2-6 bullets per second
            var returnValue = GetHighInLowOutInRange(Attributes.Speed, 0.07f, 0.4f);
            return returnValue;
        }

        public float GetAmmoPerSecond()
        {
            return 1 / GetDelayBetweenShots();
        }

        public int GetDefenseValue()
        {
            return IsTwoHanded
                ? Attributes.Strength * 2
                : Attributes.Strength;
        }

        public float GetMeleeDps(float multiplier)
        {
            var damage = GetCombatService().GetDamageValueFromAttack(null, this, 0);

            var windUp = GetChargeUpTime();
            var timeForTwoAttacks = windUp + GetCooldownTime() + windUp;

            return damage * 2 / timeForTwoAttacks * multiplier;
        }

        public float GetRangedDps()
        {
            var damage = GetCombatService().GetDamageValueFromAttack(null, this, 0);
            return GetDamagePerSecond(damage, GetAmmoMax(), GetAmmoPerSecond(), GetReloadTime());
        }

        public string GetAmmoTypeId()
        {
            return WeaponType.AmmunitionTypeId!.Value.ToString();
        }

        public override string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full, string itemName = null)
        {
            if (IsDefensive)
            {
                return GetDefensiveWeaponDescription(localizer, levelOfDetail, itemName);
            }

            if (IsMelee)
            {
                return GetMeleeWeaponDescription(localizer, levelOfDetail, itemName);
            }

            return GetRangedWeaponDescription(localizer, levelOfDetail, itemName);
        }

        public string GetDefensiveWeaponDescription(ILocalizer localizer, LevelOfDetail levelOfDetail, string itemName = null)
        {
            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {itemName.OrIfNullOrWhitespace(Name)}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {localizer.Translate(TranslationType.ItemType, GetType().Name)}" + "\n");
            }

            if (Effects != null && Effects.Count > 0)
            {
                var localisedEffects = Effects.Select(localizer.Translate);
                sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Effects))}: {string.Join(", ", localisedEffects)}\n");
            }

            AppendToDescription(
                sb,
                localizer,
                Attributes.Strength,
                nameof(Attributes.Strength),
                AliasSegmentDefensive,
                localizer.TranslateInt(GetDefenseValue()));

            AppendToDescription(
                sb,
                localizer,
                Attributes.Speed,
                nameof(Attributes.Speed),
                AliasSegmentMelee,
                localizer.TranslateFloat(GetChargeUpTime()),
                UnitsType.UnitPerTime);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Recovery,
                nameof(Attributes.Recovery),
                AliasSegmentMelee,
                localizer.TranslateFloat(GetCooldownTime()),
                UnitsType.UnitPerTime);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Strength,
                nameof(Attributes.Strength),
                AliasSegmentItem,
                localizer.TranslateFloat(GetMeleeDps(DefensiveWeaponDpsMultiplier)),
                UnitsType.UnitPerTime);

            return sb.ToString().Trim();
        }

        public string GetMeleeWeaponDescription(ILocalizer localizer, LevelOfDetail levelOfDetail, string itemName = null)
        {
            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {itemName.OrIfNullOrWhitespace(Name)}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {localizer.Translate(TranslationType.ItemType, GetType().Name)}" + "\n");
            }

            if (Effects != null && Effects.Count > 0)
            {
                var localisedEffects = Effects.Select(localizer.Translate);
                sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Effects))}: {string.Join(", ", localisedEffects)}\n");
            }

            AppendToDescription(
                sb,
                localizer,
                Attributes.Speed,
                nameof(Attributes.Speed),
                AliasSegmentMelee,
                localizer.TranslateFloat(GetChargeUpTime()),
                UnitsType.UnitPerTime);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Recovery,
                nameof(Attributes.Recovery),
                AliasSegmentMelee,
                localizer.TranslateFloat(GetCooldownTime()),
                UnitsType.UnitPerTime);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Strength,
                nameof(Attributes.Strength),
                AliasSegmentItem,
                localizer.TranslateFloat(GetMeleeDps(1)),
                UnitsType.UnitPerTime);

            return sb.ToString().Trim();
        }

        public string GetRangedWeaponDescription(ILocalizer localizer, LevelOfDetail levelOfDetail, string itemName = null)
        {
            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {itemName.OrIfNullOrWhitespace(Name)}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {localizer.Translate(TranslationType.ItemType, GetType().Name)}" + "\n");
            }

            if (Effects != null && Effects.Count > 0)
            {
                var localisedEffects = Effects.Select(localizer.Translate);
                sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Effects))}: {string.Join(", ", localisedEffects)}\n");
            }

            AppendToDescription(sb, localizer, Attributes.IsAutomatic, nameof(Attributes.IsAutomatic));
            AppendToDescription(sb, localizer, Attributes.ExtraAmmoPerShot, nameof(Attributes.ExtraAmmoPerShot));

            if (levelOfDetail >= LevelOfDetail.Intermediate)
            {
                AppendToDescription(
                    sb,
                    localizer,
                    Attributes.Efficiency,
                    nameof(Attributes.Efficiency),
                    AliasSegmentRanged,
                    localizer.TranslateInt(GetAmmoMax()));
            }

            AppendToDescription(
                sb,
                localizer,
                Attributes.Accuracy,
                nameof(Attributes.Accuracy),
                AliasSegmentItem,
                localizer.TranslateFloat(GetAccuracy()),
                UnitsType.Percent);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Speed,
                nameof(Attributes.Speed),
                AliasSegmentRanged,
                localizer.TranslateFloat(GetAmmoPerSecond()),
                UnitsType.UnitPerTime);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Recovery,
                nameof(Attributes.Recovery),
                AliasSegmentRanged,
                localizer.TranslateFloat(GetReloadTime()),
                UnitsType.Time);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Strength,
                nameof(Attributes.Strength),
                AliasSegmentItem,
                localizer.TranslateFloat(GetRangedDps()),
                UnitsType.UnitPerTime);

            return sb.ToString().Trim();
        }
    }
}
