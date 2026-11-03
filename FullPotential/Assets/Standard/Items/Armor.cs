using System.Text;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Gameplay.Attributes;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Visuals;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Standard.Armor;

using UnityEngine;

namespace FullPotential.Standard.Items
{
    public class Armor : CombatItemBase, IHasItemVisuals
    {
        private readonly ITypeRegistry _typeRegistry;

        public IItemVisualsType Visuals { get; set; }

        public Armor(ITypeRegistry typeRegistry)
        {
            _typeRegistry = typeRegistry;
        }

        public override string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full)
        {
            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {Name}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {localizer.Translate(RegistryType)}" + "\n");
            }

            //todo: zzz v0.8 - implement armor triggers and traits
            //if (Effects != null && Effects.Count > 0)
            //{
            //    var localisedEffects = Effects.Select(localizer.GetTranslatedTypeName);
            //    sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Effects))}: {string.Join(", ", localisedEffects)}\n");
            //}

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetValueOrZero(Strength.Id),
                nameof(Strength),
                AliasSegmentDefensive,
                localizer.Translate(TranslationType.Attribute, nameof(Strength)));

            return sb.ToString().Trim();
        }

        public void InstantiateItemVisuals(IPlayerFighter playerFighter, SlotId slotId)
        {
            if (Visuals == null)
            {
                return;
            }

            Transform parentTransform = null;

            parentTransform = slotId.ToString() == Helm.TypeIdString
                ? playerFighter.BodyParts.Head
                : playerFighter.GraphicsTransform;

            _typeRegistry.LoadAddessable<GameObject>(
                Visuals.PrefabAddress,
                prefab =>
                    {
                        var newObj = Object.Instantiate(prefab, parentTransform);
                        playerFighter.Inventory.SetEquippedItemVisuals(slotId, newObj);
                    });
        }

        public override string GetTextForUiComponent(string id, IPlayerFighter playerFighter)
        {
            return null;
        }
    }
}
