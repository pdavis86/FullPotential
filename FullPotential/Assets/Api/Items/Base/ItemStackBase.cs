using System;

using FullPotential.Api.Localization;
using FullPotential.Api.Registry;

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

        public int MaxSize => ((IItemStackType)RegistryType).MaxStackSize;

        public override string GetName(ILocalizer localizer)
        {
            return $"{localizer.Translate(RegistryType)} ({Count})";
        }
        public override string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full, string itemName = null)
        {
            return levelOfDetail == LevelOfDetail.Full ? GetName(localizer) : null;
        }
    }
}
