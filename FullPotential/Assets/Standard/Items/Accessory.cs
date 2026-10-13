using System;
using System.Text;

using FullPotential.Api.Gameplay;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Standard.Accessories;

using UnityEngine;

namespace FullPotential.Standard.Items
{
    // todo: remove [Serializable] from almost everywhere
    public class Accessory : CombatItemBase, IHasItemVisuals
    {
        private readonly ITypeRegistry _typeRegistry;

        public IItemVisuals Visuals { get; set; }

        public Accessory(ITypeRegistry typeRegistry)
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

            //todo: zzz v0.8 - implement accessory triggers and traits
            //if (Effects != null && Effects.Count > 0)
            //{
            //    var localisedEffects = Effects.Select(localizer.GetTranslatedTypeName);
            //    sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Effects))}: {string.Join(", ", localisedEffects)}\n");
            //}

            //todo: zzz v0.8 - implement accessory triggers and traits
            //sb.Append($"WiP ({localizer.Translate(TranslationType.Attribute, nameof(Attributes.Strength))}: {Attributes.Strength})\n");
            //sb.Append($"WiP ({localizer.Translate(TranslationType.Attribute, nameof(Attributes.Efficiency))}: {Attributes.Efficiency})\n");
            //sb.Append($"WiP ({localizer.Translate(TranslationType.Attribute, nameof(Attributes.Speed))}: {Attributes.Speed})\n");
            //sb.Append($"WiP ({localizer.Translate(TranslationType.Attribute, nameof(Attributes.Recovery))}: {Attributes.Recovery})\n");

            return sb.ToString().Trim();
        }

        public void InstantiateItemVisuals(IPlayerFighter playerFighter, string slotId)
        {
            if (Visuals == null)
            {
                return;
            }

            Transform parentTransform = null;
            var showsOnPlayerCamera = false;
            Action<Transform> manipulateTransform = null;

            //todo: remove special cases
            switch (slotId)
            {
                case Amulet.TypeIdString + ";1":
                    const float amuletForwardMultiplier = 0.2f;
                    parentTransform = playerFighter.GraphicsTransform;
                    manipulateTransform = t => t.position += t.forward * amuletForwardMultiplier;
                    break;

                case Belt.TypeIdString + ";1":
                    parentTransform = playerFighter.GraphicsTransform;
                    break;

                case Ring.TypeIdString + ";1":
                    parentTransform = playerFighter.BodyParts.LeftArm;
                    showsOnPlayerCamera = true;
                    break;

                case Ring.TypeIdString + ";2":
                    parentTransform = playerFighter.BodyParts.RightArm;
                    showsOnPlayerCamera = true;
                    break;
            }

            var thisClient = playerFighter.IsLocalClient();

            if (!showsOnPlayerCamera && thisClient)
            {
                return;
            }

            _typeRegistry.LoadAddessable<GameObject>(
                Visuals.PrefabAddress,
                prefab =>
                {
                    var newObj = UnityEngine.Object.Instantiate(prefab, parentTransform);

                    manipulateTransform?.Invoke(newObj.transform);

                    if (showsOnPlayerCamera && thisClient)
                    {
                        newObj.SetGameLayerRecursive(LayerMask.NameToLayer(Layers.InFrontOfPlayer));
                    }

                    playerFighter.Inventory.SetEquippedItemVisuals(slotId, newObj);
                });
        }

        public override string GetTextForUiComponent(string id, IPlayerFighter playerFighter)
        {
            return null;
        }
    }
}
