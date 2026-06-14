using System;
using System.Collections.Generic;
using System.Linq;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Obsolete.Items.Types;
using FullPotential.Api.Ui;
using FullPotential.Api.Utilities;

using Unity.Netcode;

using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global

namespace FullPotential.Api.Gameplay.Behaviours
{
    public abstract class FighterBase : LivingEntityBase, IMoveable
    {
        public const string ReloadEventId = "2337f94e-5a7d-4e02-b1c8-1b5e9934a3ce";
        public const string ShotFiredEventId = "f01cd95a-67cc-4f38-a394-5a69eaa721c6";

        private const int MeleeRangeLimit = 8;
        private const int ConsumerRangeLimit = 50;
        private const int MaximumRange = 100;

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
        private ReloadEventArgs _reloadArgsLeft;
        private ReloadEventArgs _reloadArgsRight;
        private ShotFiredEventArgs _shotFiredArgsLeft;
        private ShotFiredEventArgs _shotFiredArgsRight;
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

            var leftSlotStatus = new SlotStatus(this, HandSlotIds.LeftHand);
            _slotStatuses.Add(HandSlotIds.LeftHand, leftSlotStatus);
            _reloadArgsLeft = new ReloadEventArgs(this, HandSlotIds.LeftHand);
            _shotFiredArgsLeft = new ShotFiredEventArgs(this, HandSlotIds.LeftHand);

            var rightSlotStatus = new SlotStatus(this, HandSlotIds.RightHand);
            _slotStatuses.Add(HandSlotIds.RightHand, rightSlotStatus);
            _reloadArgsRight = new ReloadEventArgs(this, HandSlotIds.RightHand);
            _shotFiredArgsRight = new ShotFiredEventArgs(this, HandSlotIds.RightHand);
        }

        protected override void Start()
        {
            base.Start();

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

        #region ServerRpc calls

        [ServerRpc]
        public void TryToAttackHoldServerRpc(string slotId)
        {
            var item = Inventory.GetItemInSlot(slotId);
            TryToAttackHold(slotId, item);
        }

        [ServerRpc]
        public void AttackWithItemInHandServerRpc(string slotId)
        {
            AttackWithItemInHand(slotId);
        }

        [ServerRpc]
        public void ReloadServerRpc(string slotId)
        {
            TryToReload(slotId);
        }

        #endregion

        #region ClientRpc calls

        // ReSharper disable once UnusedParameter.Local
        //[ClientRpc]
        // todo: replace - private void ReloadFinishedClientRpc(string slotId, ClientRpcParams clientRpcParams)
        //{
        //    //todo: zzz v0.6 - Use an event instead
        //    var slotStatus = GetSlotStatus(isLeftHand);
        //    slotStatus.IsBusy = false;
        //}

        //// ReSharper disable once UnusedParameter.Local
        //[ClientRpc]
        // todo: replace - private void StopActiveConsumerBehaviourClientRpc(string slotId, ClientRpcParams clientRpcParams)
        //{
        //    //todo: zzz v0.6 - Use an event instead
        //    var slotStatus = GetSlotStatus(isLeftHand);
        //    StopActiveConsumerBehaviour(slotStatus);
        //}

        [ClientRpc]
        public void ApplyMovementForceClientRpc(Vector3 force, ForceMode forceMode, ClientRpcParams clientRpcParams)
        {
            var targetRigidBody = GetComponent<Rigidbody>();
            targetRigidBody.AddForce(force, forceMode);
        }

        #endregion

        #region Reloading

        public void TriggerReloadFromClient(string slotId)
        {
            if (TryToReload(slotId) && !IsServer)
            {
                ReloadServerRpc(slotId);
            }
        }

        private bool TryToReload(string slotId)
        {
            var itemInSlot = Inventory.GetItemInSlot(slotId);

            if (itemInSlot is not Weapon)
            {
                return false;
            }

            var reloadEventArgs = slotId == HandSlotIds.LeftHand ? _reloadArgsLeft : _reloadArgsRight;
            _eventBus.PublishAsync(ReloadEventId, reloadEventArgs).Forget();

            return true;
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

        public int GetAvailableAmmo(string slotId)
        {
            var weapon = Inventory.GetItemInSlot<Weapon>(slotId);
            var ammoTypeId = weapon.WeaponType.AmmunitionTypeIdString;
            return _inventory.GetItemStackTotal(ammoTypeId);
        }

        public void TryToAttackHold(string slotId, ItemBase item)
        {
            var slotStatus = GetSlotStatus(slotId);

            if (item is Weapon weapon
                && weapon.Attributes.IsAutomatic)
            {
                if (!IsServer)
                {
                    TryToAttackHoldServerRpc(slotId);
                }

                slotStatus.StartAutomaticWeaponFireAsync(weapon).Forget();
                return;
            }

            if (item is Consumer consumer)
            {
                if (!ConsumeResource(consumer, isTest: true))
                {
                    return;
                }
            }

            if (item is not IHasCharge itemWithCharge || !itemWithCharge.IsChargePercentageUsed)
            {
                //Debug.LogWarning("Trying to attack hold an item that is not compatible");
                return;
            }

            if (!IsServer)
            {
                TryToAttackHoldServerRpc(slotId);
            }

            //Still cooling down
            if (itemWithCharge.ChargePercentage > 0)
            {
                return;
            }

            slotStatus.StopCooldownLoop();
            slotStatus.StartChargeUpLoopAsync(itemWithCharge).Forget();
        }

        public void TriggerAttackFromClient(string slotId)
        {
            var item = Inventory.GetItemInSlot(slotId);

            if (item is IHasCharge itemWithCharge
                && itemWithCharge.IsChargePercentageUsed
                && itemWithCharge.ChargePercentage <= 0)
            {
                TryToAttackHold(slotId, item);
                return;
            }

            AttackWithItemInHand(slotId);
        }

        public void TriggerAttackHoldFromClient(string slotId)
        {
            var item = Inventory.GetItemInSlot(slotId);
            TryToAttackHold(slotId, item);
        }

        public void AttackWithItemInHand(string slotId, bool isAutoFire = false)
        {
            if (AliveState != LivingEntityState.Alive)
            {
                return;
            }

            var itemInHand = _inventory.GetItemInSlot(slotId);

            switch (itemInHand)
            {
                case null:
                    Punch();
                    break;

                case Consumer consumer:
                    UseConsumer(slotId, consumer);
                    break;

                case Weapon weaponInHand:
                    UseWeapon(slotId, weaponInHand, isAutoFire);
                    break;

                default:
                    Debug.LogWarning("Not implemented attack for " + itemInHand.Name + " yet");
                    return;
            }

            if (!IsServer)
            {
                AttackWithItemInHandServerRpc(slotId);
            }
        }

        private void Punch()
        {
            if (!IsServer)
            {
                return;
            }

            if (Physics.Raycast(LookTransform.position, LookTransform.forward, out var hit, MeleeRangeLimit))
            {
                _combatService.ApplyEffects(this, null, hit.transform.gameObject, hit.point);
            }
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

        private void UseConsumer(string slotId, Consumer consumer)
        {
            if (consumer == null)
            {
                return;
            }

            var slotStatus = GetSlotStatus(slotId);

            if (slotStatus.StopActiveConsumerBehaviour())
            {
                return;
            }

            if (consumer.ChargePercentage < 100)
            {
                slotStatus.StopChargeUpLoop();
                slotStatus.StartCooldownLoopAsync(consumer).Forget();
                return;
            }

            if (!ConsumeResource(consumer, isTest: true))
            {
                return;
            }

            if (consumer.Targeting.IsContinuous)
            {
                slotStatus.IsConsumingResource = true;
            }

            consumer.ChargePercentage = 0;

            if (!IsServer)
            {
                return;
            }

            var handPosition = slotId == HandSlotIds.LeftHand
                ? Positions.LeftHand.position
                : Positions.RightHand.position;

            var attackDirection = Physics.Raycast(LookTransform.position, LookTransform.forward, out var hit, ConsumerRangeLimit)
                ? (hit.point - handPosition).normalized
                : LookTransform.forward;

            ConsumeResource(consumer);

            var targets = consumer.Targeting.GetTargets(this, consumer);

            if (targets == null || consumer.Shape == null)
            {
                _combatService.SpawnTargetingGameObject(this, consumer, handPosition, attackDirection);

                if (targets != null)
                {
                    foreach (var target in targets)
                    {
                        _combatService.ApplyEffects(this, consumer, target.GameObject, target.Position);
                    }
                }
            }
            else if (consumer.Shape != null)
            {
                _combatService.SpawnShapeGameObject(this, consumer, null, transform.position, transform.forward);
            }
        }

        private void UseWeapon(string slotId, Weapon weaponInHand, bool isAutoFire)
        {
            var slotStatus = GetSlotStatus(slotId);

            if (!isAutoFire && slotStatus.IsAutoFiring)
            {
                slotStatus.StopAutomaticWeaponFire();
            }

            if (weaponInHand.IsRanged)
            {
                UseRangedWeapon(slotId, slotStatus, weaponInHand);
                return;
            }

            UseMeleeWeapon(slotId, weaponInHand);
        }

        private void UseRangedWeapon(string slotId, SlotStatus slotStatus, Weapon weaponInHand)
        {
            if (weaponInHand.Ammo == 0 || slotStatus.IsBusy)
            {
                return;
            }

            var handPosition = slotId == HandSlotIds.LeftHand
                ? Positions.LeftHand.position
                : Positions.RightHand.position;

            var shotDirection = weaponInHand.GetShotDirection(LookTransform.forward);

            var endPos = Physics.Raycast(LookTransform.position, shotDirection, out var rangedHit, MaximumRange)
                ? rangedHit.point
                : handPosition + shotDirection * MaximumRange;

            var ammoUsed = Math.Min(
                1 + weaponInHand.Attributes.ExtraAmmoPerShot,
                weaponInHand.Ammo);

            var eventArgs = slotId == HandSlotIds.LeftHand ? _shotFiredArgsLeft : _shotFiredArgsRight;
            eventArgs.StartPosition = handPosition;
            eventArgs.EndPosition = endPos;
            eventArgs.AmmoUsed = ammoUsed;
            eventArgs.ObjectHit = rangedHit.transform?.gameObject;

            _eventBus.PublishAsync(ShotFiredEventId, eventArgs).Forget();

            if (rangedHit.transform == null)
            {
                return;
            }

            if (IsServer)
            {
                for (var i = 0; i < ammoUsed; i++)
                {
                    _combatService.ApplyEffects(this, weaponInHand, rangedHit.transform.gameObject, rangedHit.point);
                }
            }
        }

        public static UniTask DefaultHandlerForShotFiredEventAsync(ShotFiredEventArgs eventArgs)
        {
            if (!eventArgs.Fighter.IsServer)
            {
                return UniTask.CompletedTask;
            }

            var fighter = eventArgs.Fighter;

            var equippedWeapon = fighter.Inventory.GetItemInSlot<Weapon>(eventArgs.SlotId);

            equippedWeapon.UpdateAmmo(equippedWeapon.Ammo - eventArgs.AmmoUsed);

            // todo: is there a bug where logging out then back in restores my ammo?

            return UniTask.CompletedTask;
        }

        private void UseMeleeWeapon(string slotId, Weapon weaponInHand)
        {
            var slotStatus = GetSlotStatus(slotId);

            if (weaponInHand.ChargePercentage < 100)
            {
                slotStatus.StopChargeUpLoop();
                slotStatus.StartCooldownLoopAsync(weaponInHand).Forget();
                return;
            }

            slotStatus.StartChargeUpLoopAsync(weaponInHand).Forget();

            if (!IsServer)
            {
                return;
            }

            if (Physics.Raycast(LookTransform.position, LookTransform.forward, out var meleeHit, MeleeRangeLimit))
            {
                _combatService.ApplyEffects(this, weaponInHand, meleeHit.transform.gameObject, meleeHit.point);
            }
        }

        public bool ConsumeResource(IResourceConsumer resourceConsumerUsingItem, bool slowDrain = false, bool isTest = false)
        {
            var resourceCost = resourceConsumerUsingItem.GetResourceCost();
            var resourceTypeId = resourceConsumerUsingItem.ResourceType.TypeId.ToString();

            if (slowDrain)
            {
                // todo: trait-based resource cost?
                resourceCost = (int)Math.Ceiling(resourceCost / 10f) + 1;
            }

            var currentValue = GetResourceValue(resourceTypeId);

            if (currentValue < resourceCost)
            {
                return false;
            }

            if (!isTest)
            {
                TriggerResourceValueUpdate(resourceTypeId, -resourceCost);
            }

            return true;
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

        public static void UpdateAmmoCounts(ReloadEventArgs eventArgs)
        {
            var fighter = eventArgs.Fighter;

            var equippedWeapon = fighter.Inventory.GetItemInSlot<Weapon>(eventArgs.SlotId);

            var ammoTypeId = equippedWeapon.WeaponType.AmmunitionTypeIdString;
            var ammoNeeded = equippedWeapon.GetAmmoMax() - equippedWeapon.Ammo;

            var countTaken = fighter.Inventory.TakeCountFromItemStacks(ammoTypeId, ammoNeeded);

            equippedWeapon.UpdateAmmo(equippedWeapon.Ammo + countTaken);
        }
    }
}
