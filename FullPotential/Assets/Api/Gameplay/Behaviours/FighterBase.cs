using System;
using System.Collections.Generic;
using System.Linq;

using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Items;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Obsolete.Items.Types;
using FullPotential.Api.Ui;
using FullPotential.Api.Utilities;

using Unity.Netcode;

using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global

// todo: zzz v0.6 - Break this up e.g. combat, equipment, etc.
// todo: zzz v0.6 - Only use ClientRpc methods for non-state situations (otherwise network variables)

namespace FullPotential.Api.Gameplay.Behaviours
{
    public abstract class FighterBase : LivingEntityBase, IMoveable
    {
        #region Inspector Variables
        // ReSharper disable UnassignedField.Global
        // ReSharper disable InconsistentNaming

        public PositionTransforms Positions;
        public BodyPartTransforms BodyParts;

        // ReSharper restore UnassignedField.Global
        // ReSharper restore InconsistentNaming
        #endregion

        #region Other Variables

        private readonly Dictionary<string, SlotStatus> _slotStatuses = new Dictionary<string, SlotStatus>();

        private DelayedAction _consumeResource;

        #endregion

        #region Properties

        public abstract Transform Transform { get; }

        public abstract Transform LookTransform { get; }

        public abstract GameObject GameObject { get; }

        public string FighterName => _entityName.Value.ToString();

        #endregion

        #region Unity Events Handlers
        // ReSharper disable UnusedMemberHierarchy.Global

        protected override void Awake()
        {
            base.Awake();

            // todo: add a SlotStatus for each registered slot?

            var leftSlotStatus = new SlotStatus(_logger, this, HandSlotIds.LeftHand);
            _slotStatuses.Add(HandSlotIds.LeftHand, leftSlotStatus);

            var rightSlotStatus = new SlotStatus(_logger, this, HandSlotIds.RightHand);
            _slotStatuses.Add(HandSlotIds.RightHand, rightSlotStatus);
        }

        protected override void Start()
        {
            base.Start();

            // todo: how do we add DelayedAction instances from mods?
            _consumeResource = new DelayedAction(.5f, () =>
            {
                foreach (var kvp in _slotStatuses)
                {
                    CheckIfActiveConsumerNeedsToStop(kvp.Value);
                }
            });
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!IsServer)
            {
                return;
            }

            _consumeResource.TryPerformAction();
        }

        // ReSharper restore UnusedMemberHierarchy.Global
        #endregion

        #region ClientRpc calls

        [ClientRpc]
        public void ApplyMovementForceClientRpc(Vector3 force, ForceMode forceMode, ClientRpcParams clientRpcParams)
        {
            // todo: zzz v0.6 - Apply position changes on client rather than forces
            var targetRigidBody = GetComponent<Rigidbody>();
            targetRigidBody.AddForce(force, forceMode);
        }

        #endregion

        public override bool IsConsumingResource(string typeId)
        {
            return _slotStatuses.Any(
                kvp => kvp.Value.IsConsumingResource
                && Inventory.GetItemInSlot(kvp.Key) is Consumer consumer
                && consumer.ResourceType.TypeId.ToString() == typeId);
        }

        public SlotStatus GetSlotStatus(string slotId)
        {
            return _slotStatuses.ContainsKey(slotId)
                ? _slotStatuses[slotId]
                : null;
        }

        public int GetAttributeValue(AttributeAffected attributeAffected)
        {
            //todo: zzz v0.8 - trait-based attributes
            switch (attributeAffected)
            {
                case AttributeAffected.Strength:
                    return 25 + GetAttributeAdjustment(AttributeAffected.Strength);

                case AttributeAffected.Luck:
                    return 50 + GetAttributeAdjustment(AttributeAffected.Luck);

                default:
                    throw new Exception("Not yet implemented GetAttributeValue() for " + attributeAffected);
            }
        }

        public override void HandleDeath()
        {
            foreach (var kvp in _slotStatuses)
            {
                kvp.Value.IsBusy = false;
                kvp.Value.StopActiveConsumerBehaviour();
            }

            base.HandleDeath();
        }

        public bool StopActiveConsumerBehaviour(Consumer consumer)
        {
            var leftConsumer = Inventory.GetItemInSlot<Consumer>(HandSlotIds.LeftHand);
            if (leftConsumer == consumer)
            {
                return _slotStatuses[HandSlotIds.LeftHand].StopActiveConsumerBehaviour();
            }

            var rightConsumer = Inventory.GetItemInSlot<Consumer>(HandSlotIds.RightHand);
            if (rightConsumer == consumer)
            {
                return _slotStatuses[HandSlotIds.RightHand].StopActiveConsumerBehaviour();
            }

            return false;
        }

        public bool ConsumeResource(IResourceConsumer resourceConsumerUsingItem, bool slowDrain = false, bool isTest = false)
        {
            var resourceCost = resourceConsumerUsingItem.GetResourceCost();
            var resourceTypeId = resourceConsumerUsingItem.ResourceType.TypeId.ToString();

            if (slowDrain)
            {
                // todo: zzz v0.8 - trait-based resource cost?
                resourceCost = (int)Math.Ceiling(resourceCost / 10f) + 1;
            }

            var currentValue = GetResourceValue(resourceTypeId);

            if (currentValue < resourceCost)
            {
                return false;
            }

            if (!isTest)
            {
                TriggerResourceValueUpdate(resourceTypeId, -resourceCost, true);
            }

            return true;
        }

        private void CheckIfActiveConsumerNeedsToStop(SlotStatus slotStatus)
        {
            if (!slotStatus.IsConsumingResource)
            {
                return;
            }

            var consumer = Inventory.GetItemInSlot<Consumer>(slotStatus.SlotId);

            if (ConsumeResource(consumer, consumer.Targeting.IsContinuous))
            {
                return;
            }

            slotStatus.StopActiveConsumerBehaviour();
        }

        #region Nested Classes
        // ReSharper disable UnassignedField.Global

        [Serializable]
        public struct PositionTransforms
        {
            public Transform LeftHand;
            public Transform RightHand;
        }

        [Serializable]
        public struct BodyPartTransforms
        {
            public Transform Head;
            public Transform Body;
            public Transform LeftArm;
            public Transform RightArm;
        }

        // ReSharper restore UnassignedField.Global
        #endregion

        // todo: move to Standard
        public static int GetAvailableAmmo(FighterBase fighter, string slotId)
        {
            var weapon = fighter.Inventory.GetItemInSlot<Weapon>(slotId);
            var ammoTypeId = weapon.WeaponType.AmmunitionTypeIdString;
            return fighter.Inventory.GetItemStackTotal(ammoTypeId);
        }
    }
}
