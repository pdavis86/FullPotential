using System;
using System.Collections.Generic;
using System.Globalization;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Modding;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Crafting;
using FullPotential.Api.Registry.Effects;
using FullPotential.Core.Gameplay.Combat;
using Moq;
using NUnit.Framework;

namespace FullPotential.Core.Tests.Gameplay.Combat
{
    public class EffectServiceTests
    {
        private IEffectService _effectService;
        private IEffect _singleDamageEffect;

        [SetUp]
        public void Setup()
        {
            _singleDamageEffect = new SingleDamageEffect();

            DependenciesContext.Dependencies.ResetForTesting();

            var gameManagerMock = new Mock<IGameManager>();

            gameManagerMock
                .Setup(m => m.CurrentCulture)
                .Returns(new CultureInfo("en-GB"));

            DependenciesContext.Dependencies.Register(new Dependency
            {
                Type = typeof(IEffectService),
                Factory = () => _effectService,
                IsSingleton = true
            });

            var modHelperMock = new Mock<IModHelper>();

            modHelperMock
                .Setup(m => m.GetGameManager())
                .Returns(gameManagerMock.Object);

            DependenciesContext.Dependencies.Register(new Dependency
            {
                Type = typeof(IModHelper),
                Factory = () => modHelperMock.Object,
                IsSingleton = true
            });

            _effectService = new EffectService(Mock.Of<ITypeRegistry>(), Mock.Of<IRpcService>());
        }

        [Test]
        public void GetDamageValueFromAttack_GivenItemsWithAttributesAll50_SameBaseDamage()
        {
            //Calc is Math.Ceiling(attackStrength * defenceRatio / 4f);
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

            var consumer = new Consumer { Attributes = allFifty, Effects = new List<IEffect> { _singleDamageEffect } };
            Assert.AreEqual((int)expectedBaseDamage, GetDamage(consumer));

            //todo: test multiple effects
            //todo: test periodic damage

            var meleeOneHandedWeapon = GetMeleeWeapon(false, allFifty);
            Assert.AreEqual((int)(expectedBaseDamage * 2), GetDamage(meleeOneHandedWeapon));

            var meleeTwoHandedWeapon = GetMeleeWeapon(true, allFifty);
            Assert.AreEqual((int)(expectedBaseDamage * 4), GetDamage(meleeTwoHandedWeapon));

            var rangedOneHandedWeapon = GetRangedWeapon(false, allFifty);
            Assert.AreEqual((int)(expectedBaseDamage / rangedOneHandedWeapon.GetBulletsPerSecond()), GetDamage(rangedOneHandedWeapon));

            var rangedTwoHandedWeapon = GetRangedWeapon(true, allFifty);
            Assert.AreEqual((int)(expectedBaseDamage * 2 / rangedTwoHandedWeapon.GetBulletsPerSecond()), GetDamage(rangedTwoHandedWeapon));
        }

        [Test]
        public void GetDefenceValue_GivenItemsWithAttributesAll50_SameBaseDefence()
        {
            //todo: GetDefenceValue_GivenItemsWithAttributesAll50_SameBaseDefence()
        }

        private int GetDamage(ItemBase item)
        {
            return _effectService.GetDamageValueFromAttack(item, 0, false);
        }

        private Weapon GetMeleeWeapon(bool isTwoHanded, Attributes attributes)
        {
            return new Weapon
            {
                RegistryType = new TestWeaponType(WeaponCategory.Melee),
                Attributes = attributes,
                IsTwoHanded = isTwoHanded
            };
        }

        private Weapon GetRangedWeapon(bool isTwoHanded, Attributes attributes)
        {
            return new Weapon
            {
                RegistryType = new TestWeaponType(WeaponCategory.Ranged),
                Attributes = attributes,
                IsTwoHanded = isTwoHanded
            };
        }
    }

    public class SingleDamageEffect : IStatEffect
    {
        public Guid TypeId => new Guid("3fd4d8d9-6fed-4ada-85dd-408602769ee5");

        public string TypeName => nameof(SingleDamageEffect);

        public AffectType AffectType => AffectType.SingleDecrease;

        public AffectableStat StatToAffect => AffectableStat.Health;
    }

    public class TestWeaponType : IGearWeapon
    {
        // ReSharper disable UnassignedGetOnlyAutoProperty

        public TestWeaponType(WeaponCategory category)
        {
            Category = category;
        }

        public Guid TypeId { get; }
        public string TypeName { get; }
        public string PrefabAddress { get; }
        public WeaponCategory Category { get; }
        public bool AllowAutomatic { get; }
        public bool AllowTwoHanded { get; }
        public bool EnforceTwoHanded { get; }
        public string PrefabAddressTwoHanded { get; }
    }
}