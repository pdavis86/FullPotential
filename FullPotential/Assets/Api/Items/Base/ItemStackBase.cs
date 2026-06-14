using System;

using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Localization;

namespace FullPotential.Api.Items.Base
{
    // todo: zzz v0.6 - remove Serializable
    // todo: zzz v0.6 - make abstract
    [Serializable]
    public class ItemStackBase : ItemBase
    {
        public int CountForSerialization;
        public string BaseName;

        public int Count
        {
            get => int.Parse(PropertyDictionary[nameof(Count)]);
            set
            {
                CountForSerialization = value;
                PropertyDictionary[nameof(Count)] = value.ToString();
            }
        }

        public int MaxSize => ((IItemStack)RegistryType).MaxStackSize;

        public override string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full, string itemName = null)
        {
            return levelOfDetail == LevelOfDetail.Full ? $"{localizer.Translate(RegistryType)} ({Count})" : null;
        }
    }
}
