using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Ioc;
using Moq;
using NUnit.Framework;

namespace FullPotential.Api.Tests.Ioc
{
    [TestFixture]
    public class DependenciesCollectionTests
    {
        private DependenciesCollection _service;
        private Mock<IPlayerFighter> _playerFighterMock;
        private Mock<IPlayerInventory> _playerInventoryMock;

        [SetUp]
        public void Setup()
        {
            _service = new DependenciesCollection();

            _playerFighterMock = new Mock<IPlayerFighter>();
            _service.Register(new Dependency
            {
                Type = typeof(IPlayerFighter),
                Factory = () => _playerFighterMock.Object,
                IsSingleton = true
            });

            _playerInventoryMock = new Mock<IPlayerInventory>();
            _service.Register(new Dependency
            {
                Type = typeof(IPlayerInventory),
                Factory = () => _playerInventoryMock.Object,
                IsSingleton = true
            });
        }

        [Test]
        public void GetService_GivenConstructorInjectionExample_InstantiatesCorrectly()
        {
            _service.Register<ConstructorInjectionExample, ConstructorInjectionExample>();

            var result = _service.GetService<ConstructorInjectionExample>();

            Assert.AreEqual(_playerFighterMock.Object, result.Fighter);
            Assert.AreEqual(_playerInventoryMock.Object, result.Inventory);
        }

        [Test]
        public void GetService_GivenMethodInjectionExample_InstantiatesCorrectly()
        {
            _service.Register<MethodInjectionExample, MethodInjectionExample>();

            var result = _service.GetService<MethodInjectionExample>();

            Assert.AreEqual(_playerFighterMock.Object, result.Fighter);
            Assert.AreEqual(_playerInventoryMock.Object, result.Inventory);
        }
    }
}
