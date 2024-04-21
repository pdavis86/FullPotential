using System;
using System.Linq;
using System.Text;
using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;

namespace FullPotential.Api.Items.Types
{
    [Serializable]
    public class Weapon : CombatItemBase, IHasItemVisuals, IHasCharge
    {
        public const float DefensiveWeaponDpsMultiplier = 0.1f;

        private const string AliasSegmentMelee = "MeleeWeapon";
        private const string AliasSegmentRanged = "RangedWeapon";

        private static ICombatService _combatService;
        private IEffect _hurtEffect;

        private IItemVisuals _visuals;

        //Variables so they are serialized
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable NotAccessedField.Global
        public string WeaponVisualsTypeId;
        public int Ammo;
        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore NotAccessedField.Global

        private int _baseDamage = -1;
        private float _meleeDps = -1;
        private float _defensiveDps = -1;
        private float _rangedDps = -1;

        public IWeapon WeaponType => (IWeapon)RegistryType;

        public bool IsRanged => WeaponType.AmmunitionTypeIdString != null;

        public bool IsMelee => WeaponType.AmmunitionTypeIdString == null;

        public bool IsDefensive => WeaponType.IsDefensive;

        public string VisualsTypeId => WeaponVisualsTypeId;

        public bool IsChargePercentageUsed => !IsRanged;

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
            var returnValue = MathsHelper.GetHighInLowOutInRange(Attributes.Recovery, 0.5f, 5);
            return returnValue;
        }

        public float GetDelayBetweenShots()
        {
            //Roughly 2-6 bullets per second
            var returnValue = MathsHelper.GetHighInLowOutInRange(Attributes.Speed, 0.07f, 0.4f);
            return returnValue;
        }

        public float GetAmmoPerSecond()
        {
            return 1 / GetDelayBetweenShots();
        }

        public float GetMeleeDps()
        {
            if (_meleeDps >= 0)
            {
                return _meleeDps;
            }

            var windUp = GetChargeUpTime();
            var timeForTwoAttacks = windUp + GetCooldownTime() + windUp;

            _meleeDps = GetBaseDamage() * 2 / timeForTwoAttacks;

            return _meleeDps;
        }

        public float GetDefensiveDps()
        {
            if (_defensiveDps >= 0)
            {
                return _defensiveDps;
            }

            var windUp = GetChargeUpTime();
            var timeForTwoAttacks = windUp + GetCooldownTime() + windUp;

            _defensiveDps = GetBaseDamage() * 2 / timeForTwoAttacks * DefensiveWeaponDpsMultiplier;

            return _defensiveDps;
        }

        public float GetRangedDps()
        {
            if (_rangedDps >= 0)
            {
                return _rangedDps;
            }

            _rangedDps = GetDamagePerSecond(GetBaseDamage(), GetAmmoMax(), GetAmmoPerSecond(), GetReloadTime());

            return _rangedDps;
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
                localizer.TranslateInt(Attributes.Strength));

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
                localizer.TranslateFloat(GetDefensiveDps()),
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
                localizer.TranslateFloat(GetMeleeDps()),
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
                localizer.TranslateFloat(Attributes.Accuracy),
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

        private int GetBaseDamage()
        {
            if (_baseDamage >= 0)
            {
                return _baseDamage;
            }

            _combatService ??= DependenciesContext.Dependencies.GetService<ICombatService>();

            var baseDamage = 0;

            if (!Effects.Any())
            {
                _hurtEffect ??= DependenciesContext.Dependencies.GetService<ITypeRegistry>().GetRegisteredByTypeId<IEffect>(EffectTypeIds.HurtId);
                baseDamage = (int)_combatService.GetEffectBaseChange(null, this, _hurtEffect, false);
            }
            else
            {
                foreach (var effect in Effects)
                {
                    baseDamage += (int)_combatService.GetEffectBaseChange(null, this, effect, false);
                }
            }

            return _baseDamage = baseDamage;
        }
    }
}
