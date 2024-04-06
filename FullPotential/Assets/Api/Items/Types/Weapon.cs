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
    public class Weapon : ItemWithHealthBase, IHasVisuals
    {
        private const string AliasSegmentMelee = "MeleeWeapon";
        private const string AliasSegmentRanged = "RangedWeapon";

        private IVisuals _visuals;

        //Variables so they are serialized
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable NotAccessedField.Global
        public string WeaponVisualsTypeId;
        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore NotAccessedField.Global

        public int Ammo;

        public IWeapon WeaponType => (IWeapon) RegistryType;

        public bool IsRanged => WeaponType.AmmunitionTypeId != null;

        public bool IsMelee => WeaponType.AmmunitionTypeId == null;

        public bool IsDefensive => WeaponType.IsDefensive;

        public string VisualsTypeId => WeaponVisualsTypeId;

        public IVisuals Visuals
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
            //Debug.Log("GetAmmoMax: " + returnValue);
            return returnValue;
        }

        public float GetMeleeRecoveryTime()
        {
            var returnValue = GetHighInLowOutInRange(Attributes.Recovery, 0.5f, 5);
            //Debug.Log("GetMeleeRecoveryTime: " + returnValue);
            return returnValue;
        }

        public float GetReloadTime()
        {
            var returnValue = GetHighInLowOutInRange(Attributes.Recovery, 0.5f, 5);
            //Debug.Log("GetReloadTime: " + returnValue);
            return returnValue;
        }

        public float GetMeleeWindUpTime()
        {
            var returnValue = GetHighInLowOutInRange(Attributes.Speed, 0.05f, 0.5f);
            //Debug.Log("GetMeleeWindUpTime: " + returnValue);
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

        public float GetMeleeDps()
        {
            var damage = GetCombatService().GetDamageValueFromAttack(this, 0, false);

            var windUp = GetMeleeWindUpTime();
            var timeForTwoAttacks = windUp + GetMeleeRecoveryTime() + windUp;

            return damage * 2 / timeForTwoAttacks;
        }

        public float GetRangedDps()
        {
            var damage = GetCombatService().GetDamageValueFromAttack(this, 0, false);
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

            //todo: what does Speed do for a defensive weapon?
            //AppendToDescription(sb, localizer, Attributes.Speed, nameof(Attributes.Speed));

            //todo: what does Recovery do for a defensive weapon?
            //AppendToDescription(sb, localizer, Attributes.Recovery, nameof(Attributes.Recovery));

            AppendToDescription(
                sb,
                localizer,
                Attributes.Strength,
                nameof(Attributes.Strength),
                AliasSegmentDefensive,
                localizer.TranslateInt(GetDefenseValue()));

            //todo: Implement melee with a shield
            //AppendToDescription(
            //    sb,
            //    localizer,
            //    Attributes.Strength,
            //    nameof(Attributes.Strength),
            //    AliasSegmentItem,
            //    localizer.TranslateFloat(GetMeleeDps()),
            //    UnitsType.UnitPerTime);

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
                Attributes.Efficiency,
                nameof(Attributes.Efficiency),
                AliasSegmentMelee,
                localizer.TranslateFloat(GetMeleeWindUpTime()),
                UnitsType.UnitPerTime);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Recovery,
                nameof(Attributes.Recovery),
                AliasSegmentMelee,
                localizer.TranslateFloat(GetMeleeRecoveryTime()),
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
