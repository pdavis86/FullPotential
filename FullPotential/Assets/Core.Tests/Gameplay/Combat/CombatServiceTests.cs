using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;
using FullPotential.Api.Logging;
using FullPotential.Api.Networking;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Core.Gameplay.Combat;
using FullPotential.Core.Registry.Effects;

using Moq;

using NUnit.Framework;

namespace FullPotential.Core.Tests.Gameplay.Combat
{
    public class CombatServiceTests
    {
        private ICombatService _combatService;
        private IEffectType _singleDamageEffect;

        [SetUp]
        public void Setup()
        {
            _singleDamageEffect = new Hurt();

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
                Mock.Of<IAuditorFactory>(),
                Mock.Of<ITypeRegistry>(),
                Mock.Of<IRpcService>());
        }

        // todo: zzz v0.6 - Create Standard.Tests and move this code there
        //    [Test]
        //    public void GetDamageValueFromAttack_GivenItemsWithAttributesAll50_SameBaseDamage()
        //    {
        //        //Calc is Math.Ceiling(attackStrength * defenceRatio / 4f);
        //        const float expectedBaseDamage = 13f;

        //        var allFifty = new Dictionary<string, string>
        //        {
        //            { "Strength", "50" },
        //            { "Efficiency", "50" },
        //            { "Range", "50" },
        //            { "Accuracy", "50" },
        //            { "Speed", "50" },
        //            { "Recovery", "50" },
        //            { "Duration", "50" },
        //            { "Luck", "50" }
        //        };

        //        var consumerSingleDamage = GetConsumer(allFifty, new List<IEffectType> { _singleDamageEffect });
        //        Assert.AreEqual((int)expectedBaseDamage, GetDamage(consumerSingleDamage));

        //        var consumerMultipleEffects = GetConsumer(allFifty, new List<IEffectType> { _singleDamageEffect, _singleDamageEffect });
        //        Assert.AreEqual((int)expectedBaseDamage, GetDamage(consumerMultipleEffects));

        //        var meleeOneHandedWeapon = GetMeleeWeapon(false, allFifty);
        //        Assert.AreEqual((int)(expectedBaseDamage * 2), GetDamage(meleeOneHandedWeapon));

        //        var meleeTwoHandedWeapon = GetMeleeWeapon(true, allFifty);
        //        Assert.AreEqual((int)(expectedBaseDamage * 4), GetDamage(meleeTwoHandedWeapon));

        //        var rangedOneHandedWeapon = GetRangedWeapon(false, allFifty);
        //        Assert.AreEqual((int)(expectedBaseDamage / rangedOneHandedWeapon.GetAmmoPerSecond()), GetDamage(rangedOneHandedWeapon));

        //        var rangedTwoHandedWeapon = GetRangedWeapon(true, allFifty);
        //        Assert.AreEqual((int)(expectedBaseDamage * 2 / rangedTwoHandedWeapon.GetAmmoPerSecond()), GetDamage(rangedTwoHandedWeapon));
        //    }

        //    private int GetDamage(CombatItemBase item)
        //    {
        //        return (int)_combatService.GetEffectBaseChange(null, item, _singleDamageEffect, false);
        //    }

        //    private Consumer GetConsumer(Dictionary<string, string> attributes, List<IEffectType> effects)
        //    {
        //        return new Consumer
        //        {
        //            Attributes = new Dictionary<string, string>(attributes), 
        //            Effects = effects
        //        };
        //    }

        //    private Weapon GetMeleeWeapon(bool isTwoHanded, Dictionary<string, string> attributes)
        //    {
        //        return new Weapon
        //        {
        //            Attributes = new Dictionary<string, string>(attributes),
        //            IsTwoHanded = isTwoHanded
        //        };
        //    }

        //    private Weapon GetRangedWeapon(bool isTwoHanded, Dictionary<string, string> attributes)
        //    {
        //        return new Weapon
        //        {
        //            Attributes = new Dictionary<string, string>(attributes),
        //            IsTwoHanded = isTwoHanded
        //        };
        //    }
    }

    [ExcludeFromCodeCoverage]
    public class TestWeaponType : IWeaponType
    {
        // ReSharper disable UnassignedGetOnlyAutoProperty
        public Guid TypeId { get; }
        public bool IsDefensive { get; }
        public string AmmunitionTypeIdString { get; set; }
        public bool AllowAutomatic { get; }
        public bool AllowTwoHanded { get; }
        public bool EnforceTwoHanded { get; }
    }
}
