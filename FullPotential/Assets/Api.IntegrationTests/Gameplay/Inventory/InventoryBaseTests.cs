using System.Collections.Generic;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry.Weapons;
using Moq;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Local

namespace FullPotential.Api.IntegrationTests.Gameplay.Inventory
{
    public class InventoryBaseTests
    {
        private Mock<IAmmunition> _stackRegistryType1;
        private Mock<IAmmunition> _stackRegistryType2;

        [SetUp]
        public void Setup()
        {
            _stackRegistryType1 = new Mock<IAmmunition>();
            _stackRegistryType1
                .Setup(m => m.MaxStackSize)
                .Returns(2);

            _stackRegistryType2 = new Mock<IAmmunition>();
            _stackRegistryType2
                .Setup(m => m.MaxStackSize)
                .Returns(5);
        }

        [Test]
        public void MergeItemStacks_GivenNoItems_Adds()
        {
            var inventory = new GameObject().AddComponent<MyInventory>();

            var onlyItem = new ItemStack { Id = "a", Count = 1, RegistryType = _stackRegistryType1.Object };
            inventory.RunTest(onlyItem);

            Assert.IsTrue(inventory.Items.Count == 1);
            Assert.IsTrue(onlyItem.Count == 1);
        }

        [Test]
        public void MergeItemStacks_GivenFitsInSpace_FillsSpace()
        {
            var inventory = new GameObject().AddComponent<MyInventory>();

            var onlyItem = new ItemStack { Id = "a", Count = 1, RegistryType = _stackRegistryType1.Object };
            inventory.Items.Add(onlyItem.Id, onlyItem);

            inventory.RunTest(new ItemStack { RegistryType = _stackRegistryType1.Object, Count = 1 });

            Assert.IsTrue(inventory.Items.Count == 1);
            Assert.IsTrue(onlyItem.Count == 2);
        }

        [Test]
        public void MergeItemStacks_GivenSpansMultipleStacks_FillsSpace()
        {
            var inventory = new GameObject().AddComponent<MyInventory>();

            var itemA = new ItemStack { Id = "a", Count = 1, RegistryType = _stackRegistryType1.Object };
            inventory.Items.Add(itemA.Id, itemA);

            var itemB = new ItemStack { Id = "b", Count = 1, RegistryType = _stackRegistryType1.Object };
            inventory.Items.Add(itemB.Id, itemB);

            inventory.RunTest(new ItemStack { RegistryType = _stackRegistryType1.Object, Count = 2 });

            Assert.IsTrue(inventory.Items.Count == 2);
            Assert.IsTrue(itemA.Count == 2);
            Assert.IsTrue(itemB.Count == 2);
        }

        [Test]
        public void MergeItemStacks_GivenNoSpace_AddsANewStack()
        {
            var inventory = new GameObject().AddComponent<MyInventory>();

            var itemA = new ItemStack { Id = "a", Count = 1, RegistryType = _stackRegistryType1.Object };
            inventory.Items.Add(itemA.Id, itemA);

            var itemB = new ItemStack { Id = "b", Count = 1, RegistryType = _stackRegistryType1.Object };
            inventory.Items.Add(itemB.Id, itemB);

            var itemC = new ItemStack { Id = "c", Count = 3, RegistryType = _stackRegistryType1.Object };
            inventory.RunTest(itemC);

            Assert.IsTrue(inventory.Items.Count == 3);
            Assert.IsTrue(itemA.Count == 2);
            Assert.IsTrue(itemB.Count == 2);
            Assert.IsTrue(itemC.Count == 1);
        }

        private class MyInventory : InventoryBase
        {
            public Dictionary<string, ItemBase> Items => _items;

            public void RunTest(ItemStack itemStack)
            {
                MergeItemStacks(itemStack);
            }
        }
    }
}
