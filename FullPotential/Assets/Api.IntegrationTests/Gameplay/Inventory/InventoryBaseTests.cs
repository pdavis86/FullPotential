using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FullPotential.Api.Data;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.IntegrationTests.TestHelpers;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Localization;
using FullPotential.Api.Networking;
using FullPotential.Api.Obsolete.Networking;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Weapons;
using Moq;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Local

namespace FullPotential.Api.IntegrationTests.Gameplay.Inventory
{
    public class InventoryBaseTests : MonoBehaviourTestsBase
    {
        private Mock<ITypeRegistry> _typeRegistryMock;
        private Mock<ILocalizer> _localizerMock;
        private Mock<IRpcService> _rpcServiceMock;
        private Mock<IFragmentedMessageReconstructorFactory> _fragmentedMessageReconstructorFactoryMock;

        private Mock<IAmmunitionType> _stackRegistryType1;
        private Mock<IAmmunitionType> _stackRegistryType2;
        private MyInventory _inventory;

        [SetUp]
        public void Setup()
        {
            _typeRegistryMock = new Mock<ITypeRegistry>();
            SetupSingletonServiceMock(_typeRegistryMock);

            _localizerMock = new Mock<ILocalizer>();
            SetupSingletonServiceMock(_localizerMock);

            _rpcServiceMock = new Mock<IRpcService>();
            SetupSingletonServiceMock(_rpcServiceMock);

            _fragmentedMessageReconstructorFactoryMock = new Mock<IFragmentedMessageReconstructorFactory>();
            SetupSingletonServiceMock(_fragmentedMessageReconstructorFactoryMock);

            var fragmentedMessageReconstructorMock = new Mock<IFragmentedMessageReconstructor>();
            fragmentedMessageReconstructorMock
                .Setup(m => m.GetFragmentedMessages(It.IsAny<object>(), It.IsAny<int>()))
                .Returns(Enumerable.Empty<string>());

            _fragmentedMessageReconstructorFactoryMock
                .Setup(m => m.Create())
                .Returns(fragmentedMessageReconstructorMock.Object);

            var guid1 = Guid.NewGuid();
            _stackRegistryType1 = new Mock<IAmmunitionType>();
            _stackRegistryType1
                .Setup(m => m.TypeId)
                .Returns(guid1);
            _stackRegistryType1
                .Setup(m => m.MaxStackSize)
                .Returns(2);

            var guid2 = Guid.NewGuid();
            _stackRegistryType2 = new Mock<IAmmunitionType>();
            _stackRegistryType2
                .Setup(m => m.TypeId)
                .Returns(guid2);
            _stackRegistryType2
                .Setup(m => m.MaxStackSize)
                .Returns(5);

            _inventory = new GameObject().AddComponent<MyInventory>();
            _inventory.OverrideIsServer(true);
        }

        [Test]
        public void ApplyInventoryChanges_GivenNoItems_AddsItemStack()
        {
            var newItem1 = new ItemStack { Id = "a", Count = 1, RegistryType = _stackRegistryType1.Object };

            _inventory.ApplyInventoryChanges(new InventoryChanges { ItemStacks = new[] { newItem1 } });

            Assert.IsTrue(_inventory.Items.Count == 1);
            Assert.IsTrue(newItem1.Count == 1);
        }

        [Test]
        public void ApplyInventoryChanges_GivenFitsInSpace_FillsSpace()
        {
            var existingItem1 = new ItemStack { Id = "a", Count = 1, RegistryType = _stackRegistryType1.Object };
            _inventory.Items.Add(existingItem1.Id, existingItem1);

            var newItem1 = new ItemStack { Id = "b", Count = 1, RegistryType = _stackRegistryType1.Object };

            _inventory.ApplyInventoryChanges(new InventoryChanges { ItemStacks = new[] { newItem1 } });

            Assert.IsTrue(_inventory.Items.Count == 1);
            Assert.IsTrue(existingItem1.Count == 2);
        }

        [Test]
        public void ApplyInventoryChanges_GivenSpansMultipleStacks_FillsSpace()
        {
            var existingItem1 = new ItemStack { Id = "a", Count = 4, RegistryType = _stackRegistryType2.Object };
            _inventory.Items.Add(existingItem1.Id, existingItem1);

            var existingItem2 = new ItemStack { Id = "b", Count = 1, RegistryType = _stackRegistryType2.Object };
            _inventory.Items.Add(existingItem2.Id, existingItem2);

            var newItem1 = new ItemStack { Id = "c", Count = 2, RegistryType = _stackRegistryType2.Object };

            _inventory.ApplyInventoryChanges(new InventoryChanges { ItemStacks = new[] { newItem1 } });

            Assert.IsTrue(_inventory.Items.Count == 2);
            Assert.IsTrue(existingItem1.Count == 5);
            Assert.IsTrue(existingItem2.Count == 2);
        }

        [Test]
        public void ApplyInventoryChanges_GivenNoSpace_AddsANewStack()
        {
            var existingItem1 = new ItemStack { Id = "a", Count = 1, RegistryType = _stackRegistryType1.Object };
            _inventory.Items.Add(existingItem1.Id, existingItem1);

            var existingItem2 = new ItemStack { Id = "b", Count = 1, RegistryType = _stackRegistryType1.Object };
            _inventory.Items.Add(existingItem2.Id, existingItem2);

            var newItem1 = new ItemStack { Id = "c", Count = 3, RegistryType = _stackRegistryType1.Object };

            _inventory.ApplyInventoryChanges(new InventoryChanges { ItemStacks = new[] { newItem1 } });

            Assert.IsTrue(_inventory.Items.Count == 3);
            Assert.IsTrue(existingItem1.Count == 2);
            Assert.IsTrue(existingItem2.Count == 2);
            Assert.IsTrue(newItem1.Count == 1);
        }

        [Test]
        public void TakeCountFromItemStacks_GivenNoMatches_ReturnsNull()
        {
            var (countTaken, invChanges) = _inventory.TakeCountFromItemStacks(_stackRegistryType1.Object.TypeId.ToString(), 1);

            Assert.IsTrue(countTaken == 0);
            Assert.IsNull(invChanges);
        }

        [Test]
        public void TakeCountFromItemStacks_GivenInRangeMatch_RemovesCount()
        {
            var itemA = new ItemStack { Id = "a", Count = 9, RegistryType = _stackRegistryType1.Object };
            _inventory.Items.Add(itemA.Id, itemA);

            var (countTaken, invChanges) = _inventory.TakeCountFromItemStacks(_stackRegistryType1.Object.TypeId.ToString(), 2);

            Assert.IsTrue(countTaken == 2);
            Assert.IsTrue(invChanges.ItemStacks[0].Id == itemA.Id);
            Assert.IsTrue(invChanges.ItemStacks[0].Count == 7);
            Assert.IsTrue(_inventory.GetItemWithId<ItemStack>(itemA.Id).Count == 7);
        }

        [Test]
        public void TakeCountFromItemStacks_GivenPartialMatches_RemovesItemStacksSmallestToLargest()
        {
            var itemA = new ItemStack { Id = "a", Count = 4, RegistryType = _stackRegistryType1.Object };
            _inventory.Items.Add(itemA.Id, itemA);

            var itemB = new ItemStack { Id = "b", Count = 3, RegistryType = _stackRegistryType1.Object };
            _inventory.Items.Add(itemB.Id, itemB);

            var itemC = new ItemStack { Id = "c", Count = 2, RegistryType = _stackRegistryType1.Object };
            _inventory.Items.Add(itemC.Id, itemC);

            var (countTaken, invChanges) = _inventory.TakeCountFromItemStacks(_stackRegistryType1.Object.TypeId.ToString(), 7);

            Assert.IsTrue(countTaken == 7);

            Assert.IsTrue(invChanges.IdsToRemove.Length == 2);
            Assert.IsTrue(invChanges.IdsToRemove[0] == itemC.Id);
            Assert.IsTrue(invChanges.IdsToRemove[1] == itemB.Id);
            Assert.IsTrue(invChanges.ItemStacks.Length == 1);
            Assert.IsTrue(invChanges.ItemStacks[0].Id == itemA.Id);
            Assert.IsTrue(invChanges.ItemStacks[0].Count == 2);

            Assert.IsTrue(_inventory.GetItemWithId<ItemStack>(itemA.Id).Count == 2);
            Assert.IsTrue(_inventory.GetItemWithId<ItemStack>(itemB.Id, false) == null);
            Assert.IsTrue(_inventory.GetItemWithId<ItemStack>(itemC.Id, false) == null);
        }

        [Test]
        public void TakeCountFromItemStacks_GivenNotEnoughItems_GetAsManyAsPossible()
        {
            var itemA = new ItemStack { Id = "a", Count = 5, RegistryType = _stackRegistryType1.Object };
            _inventory.Items.Add(itemA.Id, itemA);

            var (countTaken, invChanges) = _inventory.TakeCountFromItemStacks(_stackRegistryType1.Object.TypeId.ToString(), 7);

            Assert.IsTrue(countTaken == 5);

            Assert.IsTrue(invChanges.IdsToRemove.Length == 1);
            Assert.IsTrue(invChanges.IdsToRemove[0] == itemA.Id);

            Assert.IsNull(_inventory.GetItemWithId<ItemStack>(itemA.Id, false));
        }

        private class MyInventory : InventoryBase
        {
            public Dictionary<string, ItemBase> Items => _items;

            // ReSharper disable once UnusedMember.Local
            private new void Awake()
            {
                base.Awake();

                _maxItemCount = 100;
            }

            public void OverrideIsServer(bool newValue)
            {
                var propInfo = typeof(NetworkBehaviour).GetProperty(nameof(IsServer), BindingFlags.NonPublic | BindingFlags.Instance);
                propInfo!.SetValue(this, newValue);
            }

            protected override void SetEquippedItem(string itemId, string slotId)
            {
                //Nothing here
            }

            protected override void ApplyEquippedItemChanges(SerializableKeyValuePair<string, string>[] equippedItems)
            {
                //Nothing here
            }

            protected override void NotifyOfItemsAdded(IEnumerable<ItemBase> itemsAdded)
            {
                //Nothing here
            }

            protected override void NotifyOfInventoryFull()
            {
                //Nothing here
            }

            protected override void NotifyOfItemsRemoved(IEnumerable<ItemBase> itemsRemoved)
            {
                //Nothing here
            }
        }
    }
}
