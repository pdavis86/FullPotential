using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Weapons;
using Moq;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Local

namespace FullPotential.Api.IntegrationTests.Gameplay.Inventory
{
    public class InventoryBaseTests
    {
        private Mock<ITypeRegistry> _typeRegistryMock;
        private Mock<ILocalizer> _localizerMock;
        private Mock<IAmmunition> _stackRegistryType1;
        private Mock<IAmmunition> _stackRegistryType2;
        private MyInventory _inventory;

        [SetUp]
        public void Setup()
        {
            DependenciesContext.Dependencies.ResetForTesting();

            _typeRegistryMock = new Mock<ITypeRegistry>();
            DependenciesContext.Dependencies.Register(new Dependency
            {
                Type = typeof(ITypeRegistry),
                Factory = () => _typeRegistryMock.Object,
                IsSingleton = true
            });

            _localizerMock = new Mock<ILocalizer>();
            DependenciesContext.Dependencies.Register(new Dependency
            {
                Type = typeof(ILocalizer),
                Factory = () => _localizerMock.Object,
                IsSingleton = true
            });

            var guid1 = Guid.NewGuid();
            _stackRegistryType1 = new Mock<IAmmunition>();
            _stackRegistryType1
                .Setup(m => m.TypeId)
                .Returns(guid1);
            _stackRegistryType1
                .Setup(m => m.MaxStackSize)
                .Returns(2);

            var guid2 = Guid.NewGuid();
            _stackRegistryType2 = new Mock<IAmmunition>();
            _stackRegistryType2
                .Setup(m => m.TypeId)
                .Returns(guid2);
            _stackRegistryType2
                .Setup(m => m.MaxStackSize)
                .Returns(5);

            _inventory = new GameObject().AddComponent<MyInventory>();
        }

        [Test]
        public void MergeItemStacks_GivenNoItems_Adds()
        {
            var onlyItem = new ItemStack { Id = "a", Count = 1, RegistryType = _stackRegistryType1.Object };
            _inventory.TriggerMergeItemStacks(onlyItem);

            Assert.IsTrue(_inventory.Items.Count == 1);
            Assert.IsTrue(onlyItem.Count == 1);
        }

        [Test]
        public void MergeItemStacks_GivenFitsInSpace_FillsSpace()
        {
            var onlyItem = new ItemStack { Id = "a", Count = 1, RegistryType = _stackRegistryType1.Object };
            _inventory.Items.Add(onlyItem.Id, onlyItem);

            _inventory.TriggerMergeItemStacks(new ItemStack { RegistryType = _stackRegistryType1.Object, Count = 1 });

            Assert.IsTrue(_inventory.Items.Count == 1);
            Assert.IsTrue(onlyItem.Count == 2);
        }

        [Test]
        public void MergeItemStacks_GivenSpansMultipleStacks_FillsSpace()
        {
            var itemA = new ItemStack { Id = "a", Count = 1, RegistryType = _stackRegistryType1.Object };
            _inventory.Items.Add(itemA.Id, itemA);

            var itemB = new ItemStack { Id = "b", Count = 1, RegistryType = _stackRegistryType1.Object };
            _inventory.Items.Add(itemB.Id, itemB);

            _inventory.TriggerMergeItemStacks(new ItemStack { RegistryType = _stackRegistryType1.Object, Count = 2 });

            Assert.IsTrue(_inventory.Items.Count == 2);
            Assert.IsTrue(itemA.Count == 2);
            Assert.IsTrue(itemB.Count == 2);
        }

        [Test]
        public void MergeItemStacks_GivenNoSpace_AddsANewStack()
        {
            var itemA = new ItemStack { Id = "a", Count = 1, RegistryType = _stackRegistryType1.Object };
            _inventory.Items.Add(itemA.Id, itemA);

            var itemB = new ItemStack { Id = "b", Count = 1, RegistryType = _stackRegistryType1.Object };
            _inventory.Items.Add(itemB.Id, itemB);

            var itemC = new ItemStack { Id = "c", Count = 3, RegistryType = _stackRegistryType1.Object };
            _inventory.TriggerMergeItemStacks(itemC);

            Assert.IsTrue(_inventory.Items.Count == 3);
            Assert.IsTrue(itemA.Count == 2);
            Assert.IsTrue(itemB.Count == 2);
            Assert.IsTrue(itemC.Count == 1);
        }

        [Test]
        public void TakeItemStack_GivenNoMatches_ReturnsNull()
        {
            var result = _inventory.TakeItemStack(_stackRegistryType1.Object.TypeId.ToString(), 1);

            Assert.IsNull(result);
        }

        [Test]
        public void TakeItemStack_GivenInRangeMatch_RemovesCount()
        {
            var itemA = new ItemStack { Id = "a", Count = 9, RegistryType = _stackRegistryType1.Object };
            _inventory.Items.Add(itemA.Id, itemA);

            var result = _inventory.TakeItemStack(_stackRegistryType1.Object.TypeId.ToString(), 2);

            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(_inventory.GetItemWithId<ItemStack>(itemA.Id).Count == 7);
        }

        [Test]
        public void TakeItemStack_GivenPartialMatches_RemovesItemStacks()
        {
            var itemA = new ItemStack { Id = "a", Count = 5, RegistryType = _stackRegistryType1.Object };
            _inventory.Items.Add(itemA.Id, itemA);

            var itemB = new ItemStack { Id = "b", Count = 5, RegistryType = _stackRegistryType1.Object };
            _inventory.Items.Add(itemB.Id, itemB);

            var itemC = new ItemStack { Id = "c", Count = 5, RegistryType = _stackRegistryType1.Object };
            _inventory.Items.Add(itemC.Id, itemC);

            var result = _inventory.TakeItemStack(_stackRegistryType1.Object.TypeId.ToString(), 7);

            Assert.IsTrue(result.Count == 7);
            Assert.IsNull(_inventory.GetItemWithId<ItemStack>(itemA.Id, false));
            Assert.IsTrue(_inventory.GetItemWithId<ItemStack>(itemB.Id).Count == 3);
            Assert.IsTrue(_inventory.GetItemWithId<ItemStack>(itemC.Id).Count == 5);
        }

        [Test]
        public void TakeItemStack_GivenNotEnoughItems_GetAsManyAsPossible()
        {
            var itemA = new ItemStack { Id = "a", Count = 5, RegistryType = _stackRegistryType1.Object };
            _inventory.Items.Add(itemA.Id, itemA);

            var result = _inventory.TakeItemStack(_stackRegistryType1.Object.TypeId.ToString(), 7);

            Assert.IsTrue(result.Count == 5);
            Assert.IsNull(_inventory.GetItemWithId<ItemStack>(itemA.Id, false));
        }

        private class MyInventory : InventoryBase
        {
            public Dictionary<string, ItemBase> Items => _items;

            public void TriggerMergeItemStacks(ItemStack itemStack)
            {
                MergeItemStacks(itemStack);
            }

            protected override void SetEquippedItem(string itemId, string slotId)
            {
                //Nothing here
            }
        }
    }
}
