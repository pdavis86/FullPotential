using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Cysharp.Threading.Tasks;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Models.LivingEntity;

using Unity.Netcode;

namespace FullPotential.Core.Player
{
    public class PlayerInventory : InventoryBase, IPlayerInventory
    {
        private Dictionary<string, CombatItemBase> _combatItemsWithShape;
        private PlayerFighter _playerFighter;

        #region Unity Events Handlers

        protected override void Awake()
        {
            base.Awake();

            _playerFighter = GetComponent<PlayerFighter>();
        }

        #endregion

        #region ServerRpc calls

        [ServerRpc]
        public void EquipItemServerRpc(string itemId, string slotId)
        {
            EquipItemAsync(itemId, SlotId.FromString(slotId)).Forget();
        }

        #endregion

        [ClientRpc]
        private void EquipItemClientRpc(string itemId, string slotId, ClientRpcParams clientRpcParams)
        {
            EquipItemAsync(itemId, SlotId.FromString(slotId)).Forget();
        }

        private async UniTask EquipItemAsync(string itemId, SlotId slotId)
        {
            ItemBase item;
            if (!_items.ContainsKey(itemId))
            {
                var itemData = (await _dataLoader.GetInventoryItemDataAsync(_characterId, new[] { itemId }))[0];
                item = _itemFactory.GetItemFromData(itemData);
                _items.Add(item.Id, item);
            }
            else
            {
                item = _items[itemId];
            }

            HandleSlotChange(item, slotId);

            if (IsServer)
            {
                MarkAsDirtyAndAddToQueue();

                var nearbyClients = _rpcService.ForNearbyPlayersExcept(transform.position, 0);
                EquipItemClientRpc(itemId, slotId.ToString(), nearbyClients);
            }
        }

        public IEnumerable<ItemBase> GetHandItems()
        {
            return _items
                .Where(x => x.Value.RegistryType is null or IWeaponType)
                .Select(x => x.Value)
                .OrderBy(x => x.Name);
        }

        public IEnumerable<ItemBase> GetCompatibleItems(SlotId? slotId)
        {
            if (slotId == null)
            {
                return _items
                    .Select(x => x.Value)
                    .OrderBy(x => x.Name);
            }

            var matches = _items
                .Where(x => x.Value.RegistryType != null && slotId.Value.ToString().StartsWith(x.Value.RegistryType.TypeId.ToString()));

            if (!matches.Any())
            {
                matches = _items.Where(
                    i => i.Value.RegistryType is ICustomGearType customGearType
                         && slotId.Value.ToString().StartsWith(customGearType.SlotId.ToString()));
            }

            return matches
                .Select(x => x.Value)
                .OrderBy(x => x.Name);
        }

        public override void LoadInventory(InventoryData inventoryData)
        {
            base.LoadInventory(inventoryData);

            _combatItemsWithShape = _items
                .Where(x => x.Value is CombatItemBase combatItem && combatItem.ShapeCode != null)
                .ToDictionary(x => x.Key, x => (CombatItemBase)x.Value);
        }

        public override void ApplyEquippedItemChange(string itemId, SlotId slotId)
        {
            var changes = new Dictionary<string, string> { { slotId.ToString(), itemId } };
            ApplyEquippedItemChanges(changes);
        }

        protected override void ApplyEquippedItemChanges(Dictionary<string, string> equippedItems)
        {
            if (equippedItems == null || !equippedItems.Any())
            {
                return;
            }

            foreach (var kvp in equippedItems)
            {
                var item = kvp.Value.IsNullOrWhiteSpace() ? null : _items[kvp.Value];
                var slotId = SlotId.FromString(kvp.Key);

                _playerFighter.GetSlotStatus(slotId)?.StopActiveConsumerBehaviour();

                if (_equippedItems.TryGetValue(slotId, out var equippedItem))
                {
                    equippedItem.Item = item;
                }
                else
                {
                    if (!IsValidSlotId(slotId))
                    {
                        _logger.Warn("Invalid slot ID {0}", slotId);
                        return;
                    }

                    _equippedItems.Add(slotId, new EquippedItem { Item = item });
                }

                SpawnEquippedObject(item, slotId);
            }

            MarkAsDirtyAndAddToQueue();
        }

        private void DespawnEquippedObject(SlotId slotId)
        {
            if (!_equippedItems.ContainsKey(slotId))
            {
                return;
            }

            var currentlyInGame = _equippedItems[slotId].GameObject;

            if (currentlyInGame == null)
            {
                return;
            }

            currentlyInGame.name = "DESTROY" + currentlyInGame.name;
            Destroy(currentlyInGame);
            _equippedItems[slotId].GameObject = null;
        }

        private void SpawnEquippedObject(ItemBase item, SlotId slotId)
        {
            DespawnEquippedObject(slotId);

            if (item == null)
            {
                return;
            }

            if (item is IHasItemVisuals itemWithVisuals && itemWithVisuals.Visuals != null)
            {
                itemWithVisuals.InstantiateItemVisuals(_playerFighter, slotId);
            }
        }

        public void AddItemAsAdmin(ItemBase item)
        {
            GameManager.Instance.CheckIsAdmin();

            _items.Add(item.Id, item);

            MarkAsDirtyAndAddToQueue();
        }

        public string GetAssignedShape(string itemId)
        {
            if (!_combatItemsWithShape.ContainsKey(itemId))
            {
                return null;
            }

            return _combatItemsWithShape[itemId].ShapeCode;
        }

        public bool SetAssignedShape(string itemId, string shapeCode)
        {
            if (shapeCode.IsNullOrWhiteSpace())
            {
                _combatItemsWithShape[itemId].ShapeCode = null;
                _combatItemsWithShape[itemId].IsDirty = true;
                _combatItemsWithShape.Remove(itemId);
                return true;
            }

            var conflict = _combatItemsWithShape.Any(x => x.Key != itemId && x.Value.ShapeCode == shapeCode);
            if (conflict)
            {
                return false;
            }

            if (!_combatItemsWithShape.ContainsKey(itemId))
            {
                _combatItemsWithShape.Add(itemId, (CombatItemBase)_items[itemId]);
            }

            _combatItemsWithShape[itemId].ShapeCode = shapeCode;
            _combatItemsWithShape[itemId].IsDirty = true;
            return true;
        }

        public ItemBase GetItemFromAssignedShape(string shapeCode)
        {
            var comparisonShapeCode = GetShapeCodeWithoutLengths(shapeCode);

            var match = _combatItemsWithShape
                .FirstOrDefault(x => GetShapeCodeWithoutLengths(x.Value.ShapeCode) == comparisonShapeCode);

            if (match.Key.IsNullOrWhiteSpace())
            {
                return null;
            }

            return GetItemWithId<ItemBase>(match.Key);
        }

        private string GetShapeCodeWithoutLengths(string shapeCode)
        {
            return Regex.Replace(shapeCode, "(:\\d+)", string.Empty);
        }

        public Dictionary<SlotId, string> GetEquippedItems()
        {
            return _equippedItems.ToDictionary(
                x => x.Key,
                x => x.Value.Item?.Id);
        }
    }
}
