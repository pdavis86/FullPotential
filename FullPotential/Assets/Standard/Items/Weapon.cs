using System.Collections.Generic;
using System.Linq;
using System.Text;

using FullPotential.Api.Gameplay;
using FullPotential.Api.Gameplay.Attributes;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Api.Ui;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities.Extensions;

using UnityEngine;

namespace FullPotential.Standard.Items
{
    public class Weapon : CombatItemBase, IHasItemVisuals, IHasCharge
    {
        private readonly ITypeRegistry _typeRegistry;

        private const string AliasSegmentMelee = "MeleeWeapon";
        private const string AliasSegmentRanged = "RangedWeapon";

        public IWeaponType WeaponType => (IWeaponType)RegistryType;

        public bool IsRanged => WeaponType.AmmunitionTypeIdString != null;

        public bool IsMelee => WeaponType.AmmunitionTypeIdString == null;

        public bool IsDefensive => WeaponType.IsDefensive;

        public bool IsChargePercentageUsed => !IsRanged;

        public int ChargePercentage { get; set; }

        public int Ammo
        {
            get => Properties.GetIntValue(nameof(Ammo));
            set => Properties[nameof(Ammo)] = value.ToString();
        }

        public bool IsAutomatic
        {
            get => Properties.GetBoolValue(nameof(IsAutomatic));
            set => Properties[nameof(IsAutomatic)] = value.ToString();
        }

        public int ExtraAmmoPerShot
        {
            get => Properties.GetIntValue(nameof(ExtraAmmoPerShot));
            set => Properties[nameof(ExtraAmmoPerShot)] = value.ToString();
        }

        public IItemVisualsType Visuals { get; set; }

        public Weapon(ITypeRegistry typeRegistry)
        {
            _typeRegistry = typeRegistry;
        }

        public override string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full)
        {
            if (IsDefensive)
            {
                return GetDefensiveWeaponDescription(localizer, levelOfDetail, Name);
            }

            if (IsMelee)
            {
                return GetMeleeWeaponDescription(localizer, levelOfDetail, Name);
            }

            return GetRangedWeaponDescription(localizer, levelOfDetail, Name);
        }

        public string GetDefensiveWeaponDescription(ILocalizer localizer, LevelOfDetail levelOfDetail, string itemName = null)
        {
            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {itemName.OrIfNullOrWhitespace(Name)}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {localizer.Translate(RegistryType)}" + "\n");
            }

            if (EffectTypes != null && EffectTypes.Count > 0)
            {
                var localisedEffects = EffectTypes.Select(localizer.Translate);
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(EffectTypes))}: {string.Join(", ", localisedEffects)}\n");
            }

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetIntValue(Strength.TypeIdString),
                nameof(Strength),
                AliasSegmentDefensive,
                localizer.Translate(TranslationType.Attribute, nameof(Strength)));

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetIntValue(Speed.TypeIdString),
                nameof(Speed),
                AliasSegmentMelee,
                localizer.Translate(GetChargeUpTime()),
                UnitsType.UnitPerTime);

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetIntValue(Recovery.TypeIdString),
                nameof(Recovery),
                AliasSegmentMelee,
                localizer.Translate(GetCooldownTime()),
                UnitsType.UnitPerTime);

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetIntValue(Strength.TypeIdString),
                nameof(Strength),
                AliasSegmentItem,
                localizer.Translate(GetDefensiveDps()),
                UnitsType.UnitPerTime);

            return sb.ToString().Trim();
        }

        public string GetMeleeWeaponDescription(ILocalizer localizer, LevelOfDetail levelOfDetail, string itemName = null)
        {
            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {itemName.OrIfNullOrWhitespace(Name)}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {localizer.Translate(RegistryType)}" + "\n");
            }

            if (EffectTypes != null && EffectTypes.Count > 0)
            {
                var localisedEffects = EffectTypes.Select(localizer.Translate);
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(EffectTypes))}: {string.Join(", ", localisedEffects)}\n");
            }

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetIntValue(Speed.TypeIdString),
                nameof(Speed),
                AliasSegmentMelee,
                localizer.Translate(GetChargeUpTime()),
                UnitsType.UnitPerTime);

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetIntValue(Recovery.TypeIdString),
                nameof(Recovery),
                AliasSegmentMelee,
                localizer.Translate(GetCooldownTime()),
                UnitsType.UnitPerTime);

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetIntValue(Strength.TypeIdString),
                nameof(Strength),
                AliasSegmentItem,
                localizer.Translate(GetMeleeDps()),
                UnitsType.UnitPerTime);

            return sb.ToString().Trim();
        }

        public string GetRangedWeaponDescription(ILocalizer localizer, LevelOfDetail levelOfDetail, string itemName = null)
        {
            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {itemName.OrIfNullOrWhitespace(Name)}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {localizer.Translate(RegistryType)}" + "\n");
            }

            if (EffectTypes != null && EffectTypes.Count > 0)
            {
                var localisedEffects = EffectTypes.Select(localizer.Translate);
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(EffectTypes))}: {string.Join(", ", localisedEffects)}\n");
            }

            AppendToDescription(sb, localizer, IsAutomatic, nameof(IsAutomatic));
            AppendToDescription(sb, localizer, ExtraAmmoPerShot, nameof(ExtraAmmoPerShot));

            if (levelOfDetail >= LevelOfDetail.Intermediate)
            {
                AppendToDescription(
                    sb,
                    localizer,
                    Attributes.GetIntValue(Efficiency.TypeIdString),
                    nameof(Efficiency),
                    AliasSegmentRanged,
                    localizer.Translate(GetAmmoMax()));
            }

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetIntValue(Accuracy.TypeIdString),
                nameof(Accuracy),
                AliasSegmentItem,
                Attributes.GetStringValueOrNull(Accuracy.TypeIdString),
                UnitsType.Percent);

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetIntValue(Speed.TypeIdString),
                nameof(Speed),
                AliasSegmentRanged,
                localizer.Translate(GetAmmoPerSecond()),
                UnitsType.UnitPerTime);

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetIntValue(Recovery.TypeIdString),
                nameof(Recovery),
                AliasSegmentRanged,
                localizer.Translate(GetReloadTime()),
                UnitsType.Time);

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetIntValue(Strength.TypeIdString),
                nameof(Strength),
                AliasSegmentItem,
                localizer.Translate(GetRangedDps()),
                UnitsType.UnitPerTime);

            return sb.ToString().Trim();
        }

        public void UpdateAmmo(int newValue)
        {
            Ammo = newValue;
            IsDirty = true;
        }

        public override IEnumerable<string> ValidateCraftingAttempt(List<ItemBase> components)
        {
            var errors = new List<string>();

            if (components.Count > 8)
            {
                errors.Add("crafting.error.toomanycomponents");
            }
            if (components.Count > 4 && !IsTwoHanded)
            {
                errors.Add("crafting.error.toomanyforonehanded");
            }

            return errors;
        }

        public override string GetTextForUiComponent(string id, IPlayerFighter playerFighter)
        {
            if (!IsRanged)
            {
                return null;
            }

            var ammoTypeId = WeaponType.AmmunitionTypeIdString;
            var availableAmmo = playerFighter.Inventory.GetItemStackTotal(ammoTypeId);

            return $"{Ammo}/{GetAmmoMax()} ({availableAmmo})";
        }

        public void InstantiateItemVisuals(IPlayerFighter playerFighter, string slotId)
        {
            _typeRegistry.LoadAddessable<GameObject>(
               Visuals.PrefabAddress,
               prefab =>
               {
                   var newObj = Object.Instantiate(prefab, playerFighter.InFrontOfPlayerGameObject.transform);

                   newObj.transform.localPosition = slotId == HandSlotIds.LeftHand
                       ? playerFighter.Positions.LeftHand.localPosition
                       : playerFighter.Positions.RightHand.localPosition;

                   newObj.transform.localEulerAngles = new Vector3(0, 90);

                   if (playerFighter.IsLocalClient())
                   {
                       newObj.SetGameLayerRecursive(playerFighter.InFrontOfPlayerGameObject.layer);
                   }

                   playerFighter.Inventory.SetEquippedItemVisuals(slotId, newObj);
               });
        }
    }
}
