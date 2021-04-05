namespace Assets.Core.Data
{
    [System.Serializable]
    public class InventoryChange : Inventory
    {
        public string[] IdsToRemove;

        public InventoryChange() { }

        public InventoryChange(Inventory inventory)
        {
            MaxItems = inventory.MaxItems;

            Loot = inventory.Loot;
            Accessories = inventory.Accessories;
            Armor = inventory.Armor;
            Spells = inventory.Spells;
            Weapons = inventory.Weapons;

            EquipSlots = inventory.EquipSlots;
        }
    }
}
