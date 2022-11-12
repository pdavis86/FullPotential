using System;
using System.Collections.Generic;
using System.Globalization;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Gameplay.Items;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry.Crafting;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Utilities;
using Moq;
using NUnit.Framework;

namespace FullPotential.Tests
{
    public class ValueCalculatorTests
    {
        private IValueCalculator _valueCalculator;
        private IEffect _singleDamageEffect;

        [SetUp]
        public void Setup()
        {
            _valueCalculator = new ValueCalculator();
            _singleDamageEffect = new SingleDamageEffect();

            var modHelperMock = new Mock<IModHelper>();
            var gameManagerMock = new Mock<IGameManager>();

            DependenciesContext.Dependencies.ResetForTesting();

            DependenciesContext.Dependencies.Register(new Dependency
            {
                Type = typeof(IModHelper),
                Factory = () => modHelperMock.Object,
                IsSingleton = true
            });

            DependenciesContext.Dependencies.Register(new Dependency
            {
                Type = typeof(IValueCalculator),
                Factory = () => _valueCalculator,
                IsSingleton = true
            });

            modHelperMock
                .Setup(m => m.GetGameManager())
                .Returns(gameManagerMock.Object);

            gameManagerMock
                .Setup(m => m.CurrentCulture)
                .Returns(new CultureInfo("en-GB"));
        }

        [Test]
        public void GetDamageValueFromAttack_GivenItemsWithAttributesAll50_SameBaseDamage()
        {
            const float expectedBaseDamage = 13f;

            var allFifty = new Attributes
            {
                Strength = 50,
                Efficiency = 50,
                Range = 50,
                Accuracy = 50,
                Speed = 50,
                Recovery = 50,
                Duration = 50,
                Luck = 50
            };

            var meleeOneHandedWeapon = GetMeleeWeapon(false, allFifty);
            Assert.AreEqual(expectedBaseDamage * 2, GetDamage(meleeOneHandedWeapon));

            var meleeTwoHandedWeapon = GetMeleeWeapon(true, allFifty);
            Assert.AreEqual(expectedBaseDamage * 4, GetDamage(meleeTwoHandedWeapon));

            var spell = new Spell { Attributes = allFifty, Effects = new List<IEffect> { _singleDamageEffect } };
            Assert.AreEqual(expectedBaseDamage, GetDamage(spell));

            var gadget = new Gadget { Attributes = allFifty, Effects = new List<IEffect> { _singleDamageEffect } };
            Assert.AreEqual(expectedBaseDamage, GetDamage(gadget));

            //todo: multiple effects
            //todo: plus periodic damage
        }

        private float GetDamage(ItemBase item)
        {
            return _valueCalculator.GetDamageValueFromAttack(item, 0, false);
        }

        private Weapon GetMeleeWeapon(bool isTwoHanded, Attributes attributes)
        {
            return new Weapon
            {
                RegistryType = new TestWeaponType(IGearWeapon.WeaponCategory.Melee),
                Attributes = attributes,
                IsTwoHanded = isTwoHanded
            };
        }
    }

    public class SingleDamageEffect : IStatEffect
    {
        public Guid TypeId => new Guid("3fd4d8d9-6fed-4ada-85dd-408602769ee5");

        public string TypeName => nameof(SingleDamageEffect);

        public Affect Affect => Affect.SingleDecrease;

        public AffectableStat StatToAffect => AffectableStat.Health;
    }

    public class TestWeaponType : IGearWeapon
    {
        public TestWeaponType(IGearWeapon.WeaponCategory category)
        {
            Category = category;
        }

        public Guid TypeId { get; }
        public string TypeName { get; }
        public string PrefabAddress { get; }
        public IGearWeapon.WeaponCategory Category { get; }
        public bool AllowAutomatic { get; }
        public bool AllowTwoHanded { get; }
        public bool EnforceTwoHanded { get; }
        public string PrefabAddressTwoHanded { get; }
    }
}