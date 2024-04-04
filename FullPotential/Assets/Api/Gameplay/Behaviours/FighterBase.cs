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
using Unity.Netcode;
using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global

namespace FullPotential.Api.Gameplay.Behaviours
{
    public abstract class FighterBase : LivingEntityBase, IDefensible, IMoveable
    {
        private const float ChargeGaugeUpdateSeconds = 0.05f;

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
            handStatus.IsBusy = false;
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

            handStatus.IsBusy = true;

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

            return handStatus.IsConsumingResource
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
            //todo: zzz v0.4 - trait-based attributes
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
            HandStatusLeft.IsBusy = false;
            HandStatusRight.IsBusy = false;

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

                if (handStatus.PostActionEnumerator != null)
                {
                    StopCoroutine(handStatus.PostActionEnumerator);
                }
                
                handStatus.PreActionEnumerator = ConsumerChargeCoroutine(consumer);
                handStatus.PostActionEnumerator = ConsumerCooldownCoroutine(consumer);

                StartCoroutine(handStatus.PreActionEnumerator);

                return;
            }

            if (item is Weapon weapon
                && weapon.Attributes.IsAutomatic)
            {
                if (!IsHost)
                {
                    TryToAttackHoldServerRpc(isLeftHand);
                }

                handStatus.IntraActionEnumerator = AutomaticWeaponFire(weapon, isLeftHand);
                StartCoroutine(handStatus.IntraActionEnumerator);
            }

            //Debug.LogWarning("Trying to attack hold an item that is not compatible");
        }

        private IEnumerator ConsumerChargeCoroutine(Consumer consumer)
        {
            var secondsToTake = consumer.GetChargeTime();
            var secondsUntilDone = secondsToTake * (100 - consumer.ChargePercentage) / 100f;
            var elapsedSeconds = secondsToTake - secondsUntilDone;

            //var sw = System.Diagnostics.Stopwatch.StartNew();

            while (consumer.ChargePercentage < 100)
            {
                yield return new WaitForSeconds(ChargeGaugeUpdateSeconds);
                
                elapsedSeconds += ChargeGaugeUpdateSeconds;
                consumer.ChargePercentage = (int)(elapsedSeconds / secondsToTake * 100);
            }

            //Debug.Log($"Charged in: {sw.ElapsedMilliseconds}ms and should have taken {secondsUntilDone}s");
        }

        private IEnumerator ConsumerCooldownCoroutine(Consumer consumer)
        {
            var secondsToTake = consumer.GetCooldownTime();
            var secondsUntilDone = secondsToTake * consumer.ChargePercentage / 100f;
            var elapsedSeconds = secondsToTake - secondsUntilDone;

            //var sw = System.Diagnostics.Stopwatch.StartNew();

            while (consumer.ChargePercentage > 0)
            {
                yield return new WaitForSeconds(ChargeGaugeUpdateSeconds);
                
                elapsedSeconds += ChargeGaugeUpdateSeconds;
                consumer.ChargePercentage = 100 - (int)(elapsedSeconds / secondsToTake * 100);
            }

            //Debug.Log($"Cooled in: {sw.ElapsedMilliseconds}ms and should have taken {secondsUntilDone}s");
        }

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

        public void AttackWithItemInHand(bool isLeftHand, bool isAutoFire = false)
        {
            if (AliveState != LivingEntityState.Alive)
            {
                return;
            }

            var itemInHand = isLeftHand
                ? _inventory.GetItemInSlot(HandSlotIds.LeftHand)
                : _inventory.GetItemInSlot(HandSlotIds.RightHand);

            switch (itemInHand)
            {
                case null:
                    Punch();
                    break;

                case Consumer consumer:
                    UseConsumer(isLeftHand, consumer);
                    break;

                case Weapon weaponInHand:
                    UseWeapon(isLeftHand, weaponInHand, isAutoFire);
                    break;

                default:
                    Debug.LogWarning("Not implemented attack for " + itemInHand.Name + " yet");
                    return;
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
                _combatService.ApplyEffects(this, null, hit.transform.gameObject, hit.point, 1);
            }
        }

        public bool StopActiveConsumerBehaviour(HandStatus handStatus)
        {
            if (!handStatus.IsConsumingResource)
            {
                return false;
            }

            var slotId = handStatus == HandStatusLeft
                ? HandSlotIds.LeftHand
                : HandSlotIds.RightHand;

            var activeConsumer = Inventory.GetItemInSlot<Consumer>(slotId);
            activeConsumer.StopStoppables();

            handStatus.IsConsumingResource = false;

            return true;
        }

        private void UseConsumer(bool isLeftHand, Consumer consumer)
        {
            if (consumer == null)
            {
                return;
            }

            var handStatus = GetHandStatus(isLeftHand);

            if (StopActiveConsumerBehaviour(handStatus))
            {
                return;
            }

            if (consumer.ChargePercentage < 100)
            {
                if (handStatus.PreActionEnumerator != null)
                {
                    StopCoroutine(handStatus.PreActionEnumerator);
                    StartCoroutine(handStatus.PostActionEnumerator);
                }

                return;
            }

            if (!ConsumeResource(consumer, isTest: true))
            {
                return;
            }

            if (consumer.Targeting.IsContinuous)
            {
                handStatus.IsConsumingResource = true;
            }

            StartCoroutine(handStatus.PostActionEnumerator);

            if (!IsServer)
            {
                return;
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
        }

        private void UseWeapon(bool isLeftHand, Weapon weaponInHand, bool isAutoFire)
        {
            var handStatus = isLeftHand
                ? HandStatusLeft
                : HandStatusRight;

            if (!isAutoFire && handStatus.IntraActionEnumerator != null)
            {
                StopCoroutine(handStatus.IntraActionEnumerator);
                handStatus.IntraActionEnumerator = null;
            }

            if (weaponInHand.IsRanged)
            {
                UseRangedWeapon(isLeftHand, handStatus, weaponInHand);
            }

            if (weaponInHand.IsDefensive)
            {
                UseMeleeWeapon(weaponInHand, 0.5f);
            }

            UseMeleeWeapon(weaponInHand, 1);
        }

        private void UseRangedWeapon(bool isLeftHand, HandStatus handStatus, Weapon weaponInHand)
        {
            if (weaponInHand.Ammo == 0 || handStatus.IsBusy)
            {
                return;
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
                return;
            }

            if (IsServer)
            {
                for (var i = 0; i < ammoUsed; i++)
                {
                    _combatService.ApplyEffects(this, weaponInHand, rangedHit.transform.gameObject, rangedHit.point, 1);
                }
            }
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

        private void UseMeleeWeapon(Weapon weaponInHand, float effectPercentage)
        {
            if (!IsServer)
            {
                return;
            }

            if (Physics.Raycast(LookTransform.position, LookTransform.forward, out var meleeHit, MeleeRangeLimit))
            {
                _combatService.ApplyEffects(this, weaponInHand, meleeHit.transform.gameObject, meleeHit.point, effectPercentage);
            }
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

            if (!handStatus.IsConsumingResource)
            {
                return;
            }

            var slotId = handStatus == HandStatusLeft
                ? HandSlotIds.LeftHand
                : HandSlotIds.RightHand;

            var consumer = Inventory.GetItemInSlot<Consumer>(slotId);

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
