using System;
using FullPotential.Api.Items.Base;

namespace FullPotential.Api.Gameplay.Inventory
{
    [Serializable]
    public class ItemStack : ItemBase
    {
        public int CountForSerialization;
        public string BaseName;

        public int Count
        {
            get => CountForSerialization;
            set
            {
                CountForSerialization = value;
                Name = $"{BaseName} ({value})";
            }
        }

        public int MaxSize => ((IItemStack)RegistryType).MaxStackSize;
    }
}
