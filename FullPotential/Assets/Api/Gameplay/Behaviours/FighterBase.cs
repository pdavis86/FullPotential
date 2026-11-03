using System;
using System.Collections.Generic;
using System.Linq;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Attributes;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items;
using FullPotential.Api.Utilities;

using Unity.Netcode;

using UnityEngine;


namespace FullPotential.Api.Gameplay.Behaviours
{
    public abstract class FighterBase : LivingEntityBase, IFighter, IMoveable
    {
        #region Inspector Variables

        public PositionTransforms Positions;
        public BodyPartTransforms BodyParts;

        #endregion

        #region Other Variables

        private readonly Dictionary<SlotId, SlotStatus> _slotStatuses = new Dictionary<SlotId, SlotStatus>();

        private DelayedAction _consumeValuePool;

        #endregion

        #region Properties

        public abstract Transform Transform { get; }

        public abstract Transform LookTransform { get; }

        public abstract GameObject GameObject { get; }

        public string FighterName => _entityName.Value.ToString();

        public Transform GraphicsTransform => _graphicsTransform;

        PositionTransforms IFighter.Positions => Positions;

        BodyPartTransforms IFighter.BodyParts => BodyParts;

        #endregion

        #region Unity Events Handlers

        protected override void Awake()
        {
            base.Awake();

            var leftSlotStatus = DependenciesContext.Dependencies.CreateInstance<SlotStatus>();
            leftSlotStatus.Initialise(this, HandSlotIds.LeftHand);
            _slotStatuses.Add(HandSlotIds.LeftHand, leftSlotStatus);

            var rightSlotStatus = DependenciesContext.Dependencies.CreateInstance<SlotStatus>();
            rightSlotStatus.Initialise(this, HandSlotIds.RightHand);
            _slotStatuses.Add(HandSlotIds.RightHand, rightSlotStatus);
        }

        protected override void Start()
        {
            base.Start();

            // todo: how do we add DelayedAction instances from mods?
            _consumeValuePool = new DelayedAction(.5f, () =>
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

            _consumeValuePool.TryPerformAction();
        }

        #endregion

        #region ClientRpc calls

        [ClientRpc]
        public void ApplyMovementForceClientRpc(Vector3 force, ForceMode forceMode, ClientRpcParams clientRpcParams)
        {
            // todo: zzz v0.8 - Apply position changes on client rather than forces
            var targetRigidBody = GetComponent<Rigidbody>();
            targetRigidBody.AddForce(force, forceMode);
        }

        #endregion

        public override bool IsConsumingValuePool(ValuePoolTypeId typeId)
        {
            return _slotStatuses.Any(
                kvp => kvp.Value.IsConsumingValuePool
                && Inventory.GetItemInSlot(kvp.Key) is IValuePoolConsumer consumer
                && consumer.ValuePoolType.TypeId.ToString() == typeId);
        }

        public SlotStatus GetSlotStatus(SlotId slotId)
        {
            return _slotStatuses.ContainsKey(slotId)
                ? _slotStatuses[slotId]
                : null;
        }

        public int GetAdjustedAttributeValue(AttributeTypeId attributeId)
        {
            //todo: zzz v0.8 - trait-based attributes
            if (attributeId == Strength.Id)
            {
                return 25 + GetAttributeAdjustment(attributeId);
            }

            if (attributeId == Luck.Id)
            {
                return 50 + GetAttributeAdjustment(attributeId);
            }

            throw new Exception("Not yet implemented GetAttributeValue() for " + attributeId);
        }

        public override void HandleDeath()
        {
            foreach (var kvp in _slotStatuses)
            {
                kvp.Value.SetBusyState(false);
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

        public bool ConsumeValuePool(IValuePoolConsumer valuePoolConsumerUsingItem, bool slowDrain = false, bool isTest = false)
        {
            var valuePoolCost = valuePoolConsumerUsingItem.GetValuePoolCost();
            var valuePoolTypeId = new ValuePoolTypeId(valuePoolConsumerUsingItem.ValuePoolType.TypeId);

            if (slowDrain)
            {
                // todo: zzz v0.8 - trait-based valuePool cost?
                valuePoolCost = (int)Math.Ceiling(valuePoolCost / 10f) + 1;
            }

            var currentValue = GetValuePoolValue(valuePoolTypeId);

            if (currentValue < valuePoolCost)
            {
                return false;
            }

            if (!isTest)
            {
                TriggerValuePoolValueUpdate(valuePoolTypeId, -valuePoolCost, name, valuePoolConsumerUsingItem.Name);
            }

            return true;
        }

        private void CheckIfActiveConsumerNeedsToStop(SlotStatus slotStatus)
        {
            if (!slotStatus.IsConsumingValuePool)
            {
                return;
            }

            var consumer = Inventory.GetItemInSlot<Consumer>(slotStatus.SlotId);

            if (ConsumeValuePool(consumer, consumer.TargetingType.IsContinuous))
            {
                return;
            }

            slotStatus.StopActiveConsumerBehaviour();
        }
    }
}
