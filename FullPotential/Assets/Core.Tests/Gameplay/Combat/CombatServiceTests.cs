using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Localization;
using FullPotential.Api.Networking;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Resources;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Core.Gameplay.Combat;
using Moq;
using NUnit.Framework;

namespace FullPotential.Core.Tests.Gameplay.Combat
{
    public class CombatServiceTests
    {
        private ICombatService _combatService;
        private IEffect _singleDamageEffect;

        [SetUp]
        public void Setup()
        {
            _singleDamageEffect = new SingleDamageEffect();

            DependenciesContext.Dependencies.ResetForTesting();

            var localizerMock = new Mock<ILocalizer>();

            localizerMock
                .Setup(m => m.CurrentCulture)
                .Returns(new CultureInfo("en-GB"));

            DependenciesContext.Dependencies.Register(new Dependency
            {
                Type = typeof(ICombatService),
                Factory = () => _combatService,
                IsSingleton = true
            });

            _combatService = new CombatService(
                Mock.Of<ITypeRegistry>(),
                Mock.Of<IRpcService>());
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

            var consumerSingleDamage = new Consumer { Attributes = allFifty, Effects = new List<IEffect> { _singleDamageEffect } };
            Assert.AreEqual((int)expectedBaseDamage, GetDamage(consumerSingleDamage));

            var consumerMultipleEffects = new Consumer { Attributes = allFifty, Effects = new List<IEffect> { _singleDamageEffect, _singleDamageEffect } };
            Assert.AreEqual((int)expectedBaseDamage, GetDamage(consumerMultipleEffects));

            var meleeOneHandedWeapon = GetMeleeWeapon(false, allFifty);
            Assert.AreEqual((int)(expectedBaseDamage * 2), GetDamage(meleeOneHandedWeapon));

            var meleeTwoHandedWeapon = GetMeleeWeapon(true, allFifty);
            Assert.AreEqual((int)(expectedBaseDamage * 4), GetDamage(meleeTwoHandedWeapon));

            var rangedOneHandedWeapon = GetRangedWeapon(false, allFifty);
            Assert.AreEqual((int)(expectedBaseDamage / rangedOneHandedWeapon.GetAmmoPerSecond()), GetDamage(rangedOneHandedWeapon));

            var rangedTwoHandedWeapon = GetRangedWeapon(true, allFifty);
            Assert.AreEqual((int)(expectedBaseDamage * 2 / rangedTwoHandedWeapon.GetAmmoPerSecond()), GetDamage(rangedTwoHandedWeapon));
        }

        private int GetDamage(ItemForCombatBase item)
        {
            return _combatService.GetDamageValueFromAttack(item, 0, false);
        }

        private Weapon GetMeleeWeapon(bool isTwoHanded, Attributes attributes)
        {
            return new Weapon
            {
                RegistryType = new TestWeaponType(),
                Attributes = attributes,
                IsTwoHanded = isTwoHanded
            };
        }

        private Weapon GetRangedWeapon(bool isTwoHanded, Attributes attributes)
        {
            return new Weapon
            {
                RegistryType = new TestWeaponType { AmmunitionTypeId = new Guid("8c25a561-7321-4599-b30c-0ef1bf94ad1c") },
                Attributes = attributes,
                IsTwoHanded = isTwoHanded
            };
        }
    }

    [ExcludeFromCodeCoverage]
    public class SingleDamageEffect : IResourceEffect
    {
        public Guid TypeId => new Guid("3fd4d8d9-6fed-4ada-85dd-408602769ee5");

        public AffectType AffectType => AffectType.SingleDecrease;

        public Guid ResourceTypeId => ResourceTypeIds.Health;
    }

    [ExcludeFromCodeCoverage]
    public class TestWeaponType : IWeapon
    {
        // ReSharper disable UnassignedGetOnlyAutoProperty
        public Guid TypeId { get; }
        public bool IsDefensive { get; }
        public Guid? AmmunitionTypeId { get; set; }
        public bool AllowAutomatic { get; }
        public bool AllowTwoHanded { get; }
        public bool EnforceTwoHanded { get; }
    }
}