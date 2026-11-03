using System;
using System.Collections.Generic;
using System.Linq;

using FullPotential.Api.Gameplay.Attributes;
using FullPotential.Api.Gameplay.Crafting;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Api.Gameplay.Inventory;

using Range = FullPotential.Api.Gameplay.Attributes.Range;

namespace FullPotential.Standard.Items
{
    public class WeaponItemMapper : IItemTypeMapping
    {
        private readonly ITypeRegistry _typeRegistry;

        public Type GroupType => typeof(IWeaponType);

        public WeaponItemMapper(
            ITypeRegistry typeRegistry)
        {
            _typeRegistry = typeRegistry;
        }

        public ItemBase CreateNew()
        {
            return DependenciesContext.Dependencies.CreateInstance<Weapon>();
        }

        public ItemBase GetItemFromCrafting(IList<ItemBase> components, string itemTypeId, string itemName, Dictionary<string, string> properties)
        {
            // todo: zzz v0.6 - Cannot inject IResultFactory as it is not ready to be used yet
            var resultFactory = DependenciesContext.Dependencies.GetService<IResultFactory>();

            var weaponType = _typeRegistry.GetRegisteredByTypeId<IWeaponType>(itemTypeId);

            var isTwoHandedCurrent = properties.GetBoolValue(nameof(CombatItemBase.IsTwoHanded));
            var isTwoHandedNew = weaponType.EnforceTwoHanded || (weaponType.AllowTwoHanded && isTwoHandedCurrent);
            properties[nameof(CombatItemBase.IsTwoHanded)] = isTwoHandedNew.ToString();

            var weapon = DependenciesContext.Dependencies.CreateInstance<Weapon>();

            weapon.RegistryType = weaponType;
            weapon.Properties = properties;

            if (weaponType.IsDefensive || weapon.IsMelee)
            {
                weapon.Attributes = new Dictionary<AttributeTypeId, int>
                {
                    { Strength.Id, resultFactory.ComputeAttribute(components, Strength.Id) },
                    { Speed.Id, resultFactory.ComputeAttribute(components, Speed.Id) },
                    { Recovery.Id, resultFactory.ComputeAttribute(components, Recovery.Id) },
                };
            }
            else
            {
                weapon.Attributes = new Dictionary<AttributeTypeId, int>
                {
                    { Strength.Id, resultFactory.ComputeAttribute(components, Strength.Id) },
                    { Efficiency.Id, resultFactory.ComputeAttribute(components, Efficiency.Id) },
                    { Range.Id, resultFactory.ComputeAttribute(components, Range.Id) },
                    { Accuracy.Id, resultFactory.ComputeAttribute(components, Accuracy.Id) },
                    { Speed.Id, resultFactory.ComputeAttribute(components, Speed.Id) },
                    { Recovery.Id, resultFactory.ComputeAttribute(components, Recovery.Id) },
                };

                weapon.IsAutomatic = weaponType.AllowAutomatic && components.Any(x => x.Properties.GetBoolValue(nameof(weapon.IsAutomatic)));
                weapon.ExtraAmmoPerShot = components.FirstOrDefault(x => x.Properties.GetIntValueOrZero(nameof(weapon.ExtraAmmoPerShot)) > 0)?.Properties.GetIntValueOrZero(nameof(weapon.ExtraAmmoPerShot)) ?? 0;
                weapon.Ammo = weapon.GetAmmoMax();
            }

            resultFactory.SetEffects(weapon, components);
            resultFactory.SetItemName(weapon, true);

            return weapon;
        }
    }
}
