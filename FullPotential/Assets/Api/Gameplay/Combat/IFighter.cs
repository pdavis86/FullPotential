using FullPotential.Api.Gameplay.Combat.EventArgs;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Effects;
using UnityEngine;

// ReSharper disable UnusedParameter.Global

namespace FullPotential.Api.Gameplay.Combat
{
    public interface IFighter : IDefensible, IDamageable
    {
        public const string EventIdReload = "2337f94e-5a7d-4e02-b1c8-1b5e9934a3ce";
        public const string EventIdDamageTaken = "4e8f6a71-3708-47f2-bc57-36bcc5596d0c";
        public const string EventIdShotFired = "f01cd95a-67cc-4f38-a394-5a69eaa721c6";

        Transform Transform { get; }

        GameObject GameObject { get; }

        Rigidbody RigidBody { get; }

        Transform LookTransform { get; }

        string FighterName { get; }

        ulong OwnerClientId { get; }

        IInventory Inventory { get; }

        void AddAttributeModifier(IAttributeEffect attributeEffect, ItemForCombatBase itemUsed, float effectPercentage);

        void ApplyPeriodicActionToStat(IStatEffect statEffect, ItemForCombatBase itemUsed, IFighter sourceFighter, float effectPercentage);

        void ApplyStatValueChange(IStatEffect statEffect, ItemForCombatBase itemUsed, IFighter sourceFighter, Vector3? position, float effectPercentage);

        void ApplyTemporaryMaxActionToStat(IStatEffect statEffect, ItemForCombatBase itemUsed, IFighter sourceFighter, Vector3? position, float effectPercentage);

        void ApplyElementalEffect(IEffect elementalEffect, ItemForCombatBase itemUsed, IFighter sourceFighter, Vector3? position, float effectPercentage);

        int GetAttributeValue(AttributeAffected attributeAffected);

        float GetCriticalHitChance();

        HandStatus GetHandStatus(bool isLeftHand);

        void Reload(ReloadEventArgs reloadArgs);
    }
}
