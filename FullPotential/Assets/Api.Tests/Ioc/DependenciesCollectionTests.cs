using System;
using System.Diagnostics.CodeAnalysis;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Ioc;
using FullPotential.Api.Scenes;
using Moq;
using NUnit.Framework;
using UnityEngine;

namespace FullPotential.Api.Tests.Ioc
{
    [TestFixture]
    public class DependenciesCollectionTests
    {
        private DependenciesCollection _collection;
        private Mock<IPlayerFighter> _playerFighterMock;
        private Mock<IPlayerInventory> _playerInventoryMock;

        [SetUp]
        public void Setup()
        {
            _collection = new DependenciesCollection();

            _playerFighterMock = new Mock<IPlayerFighter>();
            _collection.Register(new Dependency
            {
                Type = typeof(IPlayerFighter),
                Factory = () => _playerFighterMock.Object,
                IsSingleton = true
            });

            _playerInventoryMock = new Mock<IPlayerInventory>();
            _collection.Register(new Dependency
            {
                Type = typeof(IPlayerInventory),
                Factory = () => _playerInventoryMock.Object,
                IsSingleton = true
            });
        }

        [Test]
        public void IsReady_GivenNoRegistrations_ReturnsFalse()
        {
            var collection = new DependenciesCollection();

            Assert.IsFalse(collection.IsReady());
        }

        [Test]
        public void IsReady_GivenRegistrationsHappened_ReturnsTrue()
        {
            Assert.IsTrue(_collection.IsReady());
        }

        [Test]
        public void GetService_GivenConstructorInjectionExample_InstantiatesCorrectly()
        {
            _collection.Register<ConstructorInjectionExample, ConstructorInjectionExample>();

            var result = _collection.GetService<ConstructorInjectionExample>();

            Assert.AreEqual(_playerFighterMock.Object, result.Fighter);
            Assert.AreEqual(_playerInventoryMock.Object, result.Inventory);
        }

        [Test]
        public void GetService_GivenMethodInjectionExample_InstantiatesCorrectly()
        {
            _collection.Register<MethodInjectionExample, MethodInjectionExample>();

            var result = _collection.GetService<MethodInjectionExample>();

            Assert.AreEqual(_playerFighterMock.Object, result.Fighter);
            Assert.AreEqual(_playerInventoryMock.Object, result.Inventory);
        }

        [Test]
        public void GetService_GivenNewInstanceOnRequestViaTypeRegistration_GivesNewInstance()
        {
            _collection.Register<ISceneService, MyDummySceneService>(true);

            var instance1 = _collection.GetService<ISceneService>();
            var instance2 = _collection.GetService<ISceneService>();

            Assert.AreNotSame(instance2, instance1);
        }

        [Test]
        public void GetService_GivenNewInstanceOnRequestViaDependencyRegistration_GivesNewInstance()
        {
            _collection.Register(new Dependency
            {
                IsSingleton = false,
                Type = typeof(ISceneService),
                Factory = () => new MyDummySceneService()
            });

            var instance1 = _collection.GetService<ISceneService>();
            var instance2 = _collection.GetService<ISceneService>();

            Assert.AreNotSame(instance2, instance1);
        }

        [Test]
        public void GetService_GivenTypeNotRegistered_Throws()
        {
            Assert.Throws<ArgumentException>(() => _collection.GetService<ISceneService>());
        }

        [ExcludeFromCodeCoverage]
        private class MyDummySceneService : ISceneService
        {
            public Vector3 GetPositionOnSolidObject(Vector3 startingPoint)
            {
                throw new NotImplementedException();
            }

            public Vector3 GetHeightAdjustedPosition(Vector3 startingPoint, GameObject gameObject)
            {
                throw new NotImplementedException();
            }

            public Vector3 GetHeightAdjustedPosition(Vector3 startingPoint, Collider collider)
            {
                throw new NotImplementedException();
            }

            public Vector3 GetHeightAdjustedPosition(Vector3 startingPoint, float gameObjectHeight)
            {
                throw new NotImplementedException();
            }
        }
    }
}
