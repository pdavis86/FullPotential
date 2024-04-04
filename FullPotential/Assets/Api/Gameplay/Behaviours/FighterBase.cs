using System;
using System.Collections;
using FullPotential.Api.Data;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Combat.EventArgs;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Ui;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global

namespace FullPotential.Api.Gameplay.Behaviours
{
    public abstract class FighterBase : LivingEntityBase, IDefensible, IMoveable
    {
        public const string EventIdReload = "2337f94e-5a7d-4e02-b1c8-1b5e9934a3ce";
        public const string EventIdShotFired = "f01cd95a-67cc-4f38-a394-5a69eaa721c6";

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

        public readonly HandStatus HandStatusLeft = new HandStatus();
        public readonly HandStatus HandStatusRight = new HandStatus();

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

            _reloadArgsLeft = new ReloadEventArgs(this, true);
            _reloadArgsRight = new ReloadEventArgs(this, false);

            _shotFiredArgsLeft = new ShotFiredEventArgs(this, true);
            _shotFiredArgsRight = new ShotFiredEventArgs(this, false);
        }

        protected override void Start()
        {
            base.Start();

            _consumeResource = new DelayedAction(.5f, () =>
            {
                CheckIfActiveConsumerNeedsToStop(true);
                CheckIfActiveConsumerNeedsToStop(false);
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
        public void TryToAttackHoldServerRpc(bool isLeftHand)
        {
            TryToAttackHold(isLeftHand);
        }

        [ServerRpc]
        public void AttackWithItemInHandServerRpc(bool isLeftHand)
        {
            AttackWithItemInHand(isLeftHand);
        }

        [ServerRpc]
        public void ReloadServerRpc(bool isLeftHand)
        {
            Reload(GetReloadEventArgs(isLeftHand));
        }

        #endregion

        #region ClientRpc calls

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void UsedWeaponClientRpc(Vector3 startPosition, Vector3 endPosition, ClientRpcParams clientRpcParams)
        {
            _gameManager.GetUserInterface().SpawnProjectileTrail(startPosition, endPosition);
        }

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void ReloadFinishedClientRpc(bool isLeftHand, ClientRpcParams clientRpcParams)
        {
            var handStatus = GetHandStatus(isLeftHand);
            handStatus.IsReloading = false;
        }

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void StopActiveConsumerBehaviourClientRpc(bool isLeftHand, ClientRpcParams clientRpcParams)
        {
            var handStatus = GetHandStatus(isLeftHand);
            StopActiveConsumerBehaviour(handStatus);
        }

        [ClientRpc]
        public void ApplyMovementForceClientRpc(Vector3 force, ForceMode forceMode, ClientRpcParams clientRpcParams)
        {
            var targetRigidBody = GetComponent<Rigidbody>();
            targetRigidBody.AddForce(force, forceMode);
        }

        #endregion

        #region Reloading

        private ReloadEventArgs GetReloadEventArgs(bool isLeftHand)
        {
            return isLeftHand ? _reloadArgsLeft : _reloadArgsRight;
        }

        public void TriggerReloadFromClient(bool isLeftHand)
        {
            var handStatus = GetHandStatus(isLeftHand);

            handStatus.IsReloading = true;

            ReloadServerRpc(isLeftHand);
        }

        public static void DefaultHandlerForReloadEvent(IEventHandlerArgs eventArgs)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            var reloadEventArgs = (ReloadEventArgs)eventArgs;

            var slotId = reloadEventArgs.IsLeftHand ? HandSlotIds.LeftHand : HandSlotIds.RightHand;
            var itemInSlot = reloadEventArgs.Fighter.Inventory.GetItemInSlot(slotId);

            if (itemInSlot is not Weapon weapon)
            {
                return;
            }

            //Lose any remaining ammo
            weapon.Ammo = 0;

            ReloadAndUpdateClientInventory(reloadEventArgs, weapon.GetAmmoMax());
        }

        private void Reload(ReloadEventArgs reloadEventArgs)
        {
            StartCoroutine(ReloadCoroutine(reloadEventArgs));
        }

        private IEnumerator ReloadCoroutine(ReloadEventArgs reloadEventArgs)
        {
            var slotId = reloadEventArgs.IsLeftHand ? HandSlotIds.LeftHand : HandSlotIds.RightHand;
            var itemInSlot = reloadEventArgs.Fighter.Inventory.GetItemInSlot(slotId);

            if (itemInSlot is not Weapon weapon)
            {
                yield break;
            }

            yield return new WaitForSeconds(weapon.GetReloadTime());

            _eventManager.Trigger(EventIdReload, reloadEventArgs);

            ReloadFinishedClientRpc(reloadEventArgs.IsLeftHand, _rpcService.ForPlayer(OwnerClientId));
        }

        #endregion

        public override bool IsConsumingResource(string typeId)
        {
            return IsHandItemConsumingResource(true, typeId)
                || IsHandItemConsumingResource(false, typeId);
        }

        private bool IsHandItemConsumingResource(bool isLeftHand, string typeId)
        {
            var handStatus = GetHandStatus(isLeftHand);

            return !handStatus.ActiveConsumerId.IsNullOrWhiteSpace()
                   && GetItemInHand(isLeftHand) is Consumer consumer
                   && consumer.ResourceType.TypeId.ToString() == typeId;
        }

        private ItemBase GetItemInHand(bool isLeftHand)
        {
            return Inventory.GetItemInSlot(isLeftHand ? HandSlotIds.LeftHand : HandSlotIds.RightHand);
        }

        public HandStatus GetHandStatus(bool isLeftHand)
        {
            return isLeftHand ? HandStatusLeft : HandStatusRight;
        }

        public int GetAttributeValue(AttributeAffected attributeAffected)
        {
            //todo: zzz v0.5 - trait-based attributes
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

        public float GetCriticalHitChance()
        {
            var luckValue = GetAttributeValue(AttributeAffected.Luck);

            if (luckValue < 20)
            {
                return 0;
            }

            //e.g. 50 luck would mean a 50/5 = 10% chance
            return luckValue / 5f;
        }

        public override int GetDefenseValue()
        {
            return _inventory.GetDefenseValue();
        }

        public override void HandleDeath()
        {
            HandStatusLeft.IsReloading = false;
            HandStatusRight.IsReloading = false;

            StopActiveConsumerBehaviour(HandStatusLeft);
            StopActiveConsumerBehaviour(HandStatusRight);

            base.HandleDeath();
        }

        public int GetAvailableAmmo(bool isLeftHand)
        {
            var weapon = (Weapon)GetItemInHand(isLeftHand);
            var ammoTypeId = weapon.GetAmmoTypeId();
            return _inventory.GetItemStackTotal(ammoTypeId);
        }

        public void TryToAttackHold(bool isLeftHand)
        {
            //todo: zzz v0.8 - Prevent player cheating by sending the ServerRpc calls themselves

            var item = GetItemInHand(isLeftHand);
            var handStatus = GetHandStatus(isLeftHand);

            if (item is Consumer consumer)
            {
                if (!ConsumeResource(consumer, isTest: true))
                {
                    return;
                }

                if (!IsHost)
                {
                    TryToAttackHoldServerRpc(isLeftHand);
                }

                var timeToCharge = consumer.GetChargeTime();
                handStatus.ChargeEnumerator = ConsumerChargeCoroutine(isLeftHand, DateTime.Now.AddSeconds(timeToCharge));
                StartCoroutine(handStatus.ChargeEnumerator);

                return;
            }

            if (item is Weapon weapon
                && weapon.Attributes.IsAutomatic)
            {
                if (!IsHost)
                {
                    TryToAttackHoldServerRpc(isLeftHand);
                }

                handStatus.RapidFireEnumerator = AutomaticWeaponFire(weapon, isLeftHand);
                StartCoroutine(handStatus.RapidFireEnumerator);
            }

            //Debug.LogWarning("Trying to attack hold an item that is not compatible");
        }

        private IEnumerator ConsumerChargeCoroutine(bool isLeftHand, DateTime deadline)
        {
            var millisecondsUntilDone = (deadline - DateTime.Now).TotalMilliseconds;

            //var sw = System.Diagnostics.Stopwatch.StartNew();

            var item = GetItemInHand(isLeftHand);
            var handStatus = GetHandStatus(isLeftHand);

            if (item is not Consumer consumer)
            {
                yield break;
            }

            while (consumer.ChargePercentage < 100)
            {
                yield return new WaitForSeconds(0.01F);
                var millisecondsRemaining = (deadline - DateTime.Now).TotalMilliseconds;
                consumer.ChargePercentage = 100 - (int)(millisecondsRemaining / millisecondsUntilDone * 100);
            }

            //Debug.Log("Charged in: " + sw.ElapsedMilliseconds + "ms");

            handStatus.ChargeEnumerator = null;
        }

        //todo: zzz v0.4.1 - cooldown instead of charge for some consumers?
        //private IEnumerator SpellOrGadgetCooldownCoroutine(HandStatus handStatus, int startPercentage, DateTime deadline)
        //{
        //    var millisecondsUntilDone = (deadline - DateTime.Now).TotalMilliseconds;

        //    //var sw = System.Diagnostics.Stopwatch.StartNew();

        //    while (handStatus.EquippedSpellOrGadget.ChargePercentage > 0)
        //    {
        //        yield return new WaitForSeconds(0.01F);
        //        var millisecondsRemaining = (deadline - DateTime.Now).TotalMilliseconds;
        //        handStatus.EquippedSpellOrGadget.ChargePercentage = (int)(startPercentage * millisecondsRemaining / millisecondsUntilDone);

        //        //Safety to stop charge and cooldown at the same time
        //        if (handStatus.ChargeEnumerator != null)
        //        {
        //            StopCoroutine(handStatus.ChargeEnumerator);
        //            handStatus.ChargeEnumerator = null;
        //        }
        //    }

        //    //Debug.Log("Cooled in: " + sw.ElapsedMilliseconds + "ms");

        //    handStatus.CooldownEnumerator = null;
        //}

        private IEnumerator AutomaticWeaponFire(Weapon weapon, bool isLeftHand)
        {
            var delay = weapon.GetDelayBetweenShots();

            while (weapon.Ammo > 0)
            {
                AttackWithItemInHand(isLeftHand, true);
                yield return new WaitForSeconds(delay);
            }
        }

        public void TriggerAttackFromClient(bool isLeftHand)
        {
            AttackWithItemInHandServerRpc(isLeftHand);
        }

        public bool AttackWithItemInHand(bool isLeftHand, bool isAutoFire = false)
        {
            if (AliveState != LivingEntityState.Alive)
            {
                return false;
            }

            var itemInHand = isLeftHand
                ? _inventory.GetItemInSlot(HandSlotIds.LeftHand)
                : _inventory.GetItemInSlot(HandSlotIds.RightHand);

            bool wasAttackSuccessful;

            switch (itemInHand)
            {
                case null:
                    wasAttackSuccessful = Punch();
                    break;

                case Consumer consumer:
                    wasAttackSuccessful = UseConsumer(isLeftHand, consumer);
                    break;

                case Weapon weaponInHand:
                    wasAttackSuccessful = UseWeapon(isLeftHand, weaponInHand, isAutoFire);
                    break;

                default:
                    Debug.LogWarning("Not implemented attack for " + itemInHand.Name + " yet");
                    return false;
            }

            return wasAttackSuccessful;
        }

        private bool Punch()
        {
            if (IsServer)
            {
                if (Physics.Raycast(LookTransform.position, LookTransform.forward, out var hit, MeleeRangeLimit))
                {
                    _combatService.ApplyEffects(this, null, hit.transform.gameObject, hit.point, 1);
                }
            }

            return true;
        }

        public bool StopActiveConsumerBehaviour(HandStatus handStatus)
        {
            if (handStatus.ActiveConsumerId.IsNullOrWhiteSpace())
            {
                return false;
            }

            var activeConsumer = Inventory.GetItemWithId<Consumer>(handStatus.ActiveConsumerId);
            activeConsumer.StopStoppables();

            handStatus.ActiveConsumerId = null;

            return true;
        }

        private bool UseConsumer(bool isLeftHand, Consumer consumer)
        {
            if (consumer == null)
            {
                return false;
            }

            var handStatus = GetHandStatus(isLeftHand);

            if (StopActiveConsumerBehaviour(handStatus))
            {
                //Return true as the action also needs performing on the server
                return true;
            }

            if (consumer.ChargePercentage < 100)
            {
                if (handStatus.ChargeEnumerator != null)
                {
                    StopCoroutine(handStatus.ChargeEnumerator);
                    consumer.ChargePercentage = 0;
                }

                return false;
            }

            if (!ConsumeResource(consumer, isTest: true))
            {
                return false;
            }

            if (consumer.Targeting.IsContinuous)
            {
                handStatus.ActiveConsumerId = consumer.Id;
            }
            else
            {
                consumer.ChargePercentage = 0;
            }

            if (!IsServer)
            {
                return true;
            }

            var handPosition = isLeftHand
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
                        _combatService.ApplyEffects(this, consumer, target.GameObject, target.Position, target.EffectPercentage);
                    }
                }
            }
            else if (consumer.Shape != null)
            {
                _combatService.SpawnShapeGameObject(this, consumer, null, transform.position, transform.forward);
            }

            return true;
        }

        private bool UseWeapon(bool isLeftHand, Weapon weaponInHand, bool isAutoFire)
        {
            var handStatus = isLeftHand
                ? HandStatusLeft
                : HandStatusRight;

            if (!isAutoFire && handStatus.RapidFireEnumerator != null)
            {
                StopCoroutine(handStatus.RapidFireEnumerator);
                handStatus.RapidFireEnumerator = null;
                return true;
            }

            if (!weaponInHand.IsRanged)
            {
                return UseMeleeWeapon(weaponInHand);
            }

            return UseRangedWeapon(isLeftHand, handStatus, weaponInHand);
        }

        private bool UseRangedWeapon(bool isLeftHand, HandStatus handStatus, Weapon weaponInHand)
        {
            if (weaponInHand.Ammo == 0 || handStatus.IsReloading)
            {
                return false;
            }

            var handPosition = isLeftHand
                ? Positions.LeftHand.position
                : Positions.RightHand.position;

            var shotDirection = weaponInHand.GetShotDirection(LookTransform.forward);

            var endPos = Physics.Raycast(LookTransform.position, shotDirection, out var rangedHit, MaximumRange)
                ? rangedHit.point
                : handPosition + shotDirection * MaximumRange;

            var ammoUsed = Math.Min(
                1 + weaponInHand.Attributes.ExtraAmmoPerShot,
                weaponInHand.Ammo);

            var eventArgs = isLeftHand ? _shotFiredArgsLeft : _shotFiredArgsRight;
            eventArgs.StartPosition = handPosition;
            eventArgs.EndPosition = endPos;
            eventArgs.AmmoUsed = ammoUsed;

            _eventManager.Trigger(EventIdShotFired, eventArgs);

            if (rangedHit.transform == null)
            {
                return false;
            }

            if (IsServer)
            {
                //Debug.Log("Should only be called once on server");
                for (var i = 0; i < ammoUsed; i++)
                {
                    _combatService.ApplyEffects(this, weaponInHand, rangedHit.transform.gameObject, rangedHit.point, 1);
                }
            }

            return true;
        }

        public void ShotFired(Vector3 startPosition, Vector3 endPosition)
        {
            var nearbyClients = _rpcService.ForNearbyPlayers(transform.position);
            UsedWeaponClientRpc(startPosition, endPosition, nearbyClients);
        }

        public static void DefaultHandlerForShotFiredEvent(IEventHandlerArgs eventArgs)
        {
            var shotFiredArgs = (ShotFiredEventArgs)eventArgs;

            if (!shotFiredArgs.Fighter.IsServer)
            {
                return;
            }

            var fighter = shotFiredArgs.Fighter;

            var slotId = shotFiredArgs.IsLeftHand ? HandSlotIds.LeftHand : HandSlotIds.RightHand;
            var equippedWeapon = (Weapon)fighter.Inventory.GetItemInSlot(slotId);

            equippedWeapon.Ammo -= shotFiredArgs.AmmoUsed;

            var invChanges = new InventoryChanges
            {
                Weapons = new[] { equippedWeapon }
            };
            fighter.Inventory.SendInventoryChangesToClient(invChanges);

            fighter.ShotFired(shotFiredArgs.StartPosition, shotFiredArgs.EndPosition);
        }

        private bool UseMeleeWeapon(Weapon weaponInHand)
        {
            //todo: zzz v0.5 - take speed and recovery into account

            if (IsServer)
            {
                var meleeRange = weaponInHand.IsTwoHanded
                    ? MeleeRangeLimit
                    : MeleeRangeLimit / 2;

                if (Physics.Raycast(LookTransform.position, LookTransform.forward, out var meleeHit, meleeRange))
                {
                    _combatService.ApplyEffects(this, weaponInHand, meleeHit.transform.gameObject, meleeHit.point, 1);
                }
            }

            return true;
        }

        public bool ConsumeResource(IResourceConsumer resourceConsumerUsingItem, bool slowDrain = false, bool isTest = false)
        {
            var resourceCost = resourceConsumerUsingItem.GetResourceCost();
            var resourceTypeId = resourceConsumerUsingItem.ResourceType.TypeId.ToString();

            if (slowDrain)
            {
                resourceCost = (int)Math.Ceiling(resourceCost / 10f) + 1;
            }

            var currentValue = GetResourceValue(resourceTypeId);

            if (currentValue < resourceCost)
            {
                return false;
            }

            if (!isTest)
            {
                AdjustResourceValue(resourceTypeId, -resourceCost);
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

        private void CheckIfActiveConsumerNeedsToStop(bool isLeftHand)
        {
            var handStatus = GetHandStatus(isLeftHand);

            if (handStatus.ActiveConsumerId.IsNullOrWhiteSpace())
            {
                return;
            }

            var consumer = Inventory.GetItemWithId<Consumer>(handStatus.ActiveConsumerId);

            if (ConsumeResource(consumer, consumer.Targeting.IsContinuous))
            {
                return;
            }

            StopActiveConsumerBehaviour(handStatus);
            StopActiveConsumerBehaviourClientRpc(isLeftHand, _rpcService.ForNearbyPlayers(transform.position));
        }

        public static void ReloadAndUpdateClientInventory(ReloadEventArgs eventArgs, int ammoNeeded)
        {
            var fighter = eventArgs.Fighter;

            var slotId = eventArgs.IsLeftHand ? HandSlotIds.LeftHand : HandSlotIds.RightHand;
            var equippedWeapon = (Weapon)fighter.Inventory.GetItemInSlot(slotId);

            var ammoTypeId = equippedWeapon.GetAmmoTypeId();

            var (countTaken, invChanges) = fighter.Inventory.TakeCountFromItemStacks(ammoTypeId, ammoNeeded);

            if (invChanges == null)
            {
                return;
            }

            equippedWeapon.Ammo += countTaken;

            invChanges.Weapons = new[] { equippedWeapon };

            fighter.Inventory.SendInventoryChangesToClient(invChanges);
        }
    }
}
