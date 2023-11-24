using System;
using System.Collections;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Combat.EventArgs;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Ui;
using FullPotential.Api.Utilities;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global

namespace FullPotential.Api.Gameplay.Behaviours
{
    public abstract class FighterBase : LivingEntityBase, IDefensible, IDamageable, IMoveable
    {
        public const string EventIdReload = "2337f94e-5a7d-4e02-b1c8-1b5e9934a3ce";
        public const string EventIdDamageTaken = "4e8f6a71-3708-47f2-bc57-36bcc5596d0c";
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

        #region Protected variables
        // ReSharper disable InconsistentNaming

        protected InventoryBase _inventory;

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

        public InventoryBase Inventory => _inventory;

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
                if (HandStatusLeft.ActiveConsumer != null
                    && !ConsumeResource(HandStatusLeft.EquippedConsumer, HandStatusLeft.EquippedConsumer.Targeting.IsContinuous))
                {
                    StopActiveConsumerBehaviour(HandStatusLeft);
                    StopActiveConsumerBehaviourClientRpc(true, _rpcService.ForNearbyPlayers(transform.position));
                }

                if (HandStatusRight.ActiveConsumer != null
                    && !ConsumeResource(HandStatusRight.EquippedConsumer, HandStatusRight.EquippedConsumer.Targeting.IsContinuous))
                {
                    StopActiveConsumerBehaviour(HandStatusRight);
                    StopActiveConsumerBehaviourClientRpc(false, _rpcService.ForNearbyPlayers(transform.position));
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
        public void TryToAttackHoldServerRpc(bool isLeftHand)
        {
            TryToAttackHold(isLeftHand);
        }

        [ServerRpc]
        public void TryToAttackServerRpc(bool isLeftHand)
        {
            if (TryToAttack(isLeftHand))
            {
                var nearbyClients = _rpcService.ForNearbyPlayersExcept(transform.position, new[] { 0ul, OwnerClientId });
                TryToAttackClientRpc(isLeftHand, nearbyClients);
            }
        }

        [ServerRpc]
        private void ReloadServerRpc(bool isLeftHand)
        {
            StartCoroutine(ReloadCoroutine(GetReloadEventArgs(isLeftHand)));

            var nearbyClients = _rpcService.ForNearbyPlayersExcept(transform.position, new[] { 0ul, OwnerClientId });
            ReloadingClientRpc(isLeftHand, nearbyClients);
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
        private void TryToAttackClientRpc(bool isLeftHand, ClientRpcParams clientRpcParams)
        {
            TryToAttack(isLeftHand);
        }

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void ReloadingClientRpc(bool isLeftHand, ClientRpcParams clientRpcParams)
        {
            StartCoroutine(ReloadCoroutine(GetReloadEventArgs(isLeftHand)));
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

        protected override bool IsConsumingResource(string typeId)
        {
            return HandStatusLeft.IsConsumingResource(typeId)
                   || HandStatusRight.IsConsumingResource(typeId);
        }

        public HandStatus GetHandStatus(bool isLeftHand)
        {
            return isLeftHand ? HandStatusLeft : HandStatusRight;
        }

        #region Reloading

        private ReloadEventArgs GetReloadEventArgs(bool isLeftHand)
        {
            return isLeftHand ? _reloadArgsLeft : _reloadArgsRight;
        }

        private ShotFiredEventArgs GetShotFiredEventArgs(bool isLeftHand, Vector3 startPosition, Vector3 endPosition)
        {
            var eventArgs = isLeftHand ? _shotFiredArgsLeft : _shotFiredArgsRight;
            eventArgs.StartPosition = startPosition;
            eventArgs.EndPosition = endPosition;
            return eventArgs;
        }

        public void TriggerReloadEvent(bool isLeftHand)
        {
            _eventManager.Trigger(FighterBase.EventIdReload, GetReloadEventArgs(isLeftHand));
        }

        public static void DefaultHandlerForReloadEvent(IEventHandlerArgs eventArgs)
        {
            var reloadEventArgs = (ReloadEventArgs)eventArgs;

            reloadEventArgs.GetNewAmmoCount = () =>
            {
                var fighter = reloadEventArgs.Fighter;
                var handStatus = fighter.GetHandStatus(reloadEventArgs.IsLeftHand);

                var ammoTypeId = handStatus.EquippedWeapon.GetAmmoTypeId();
                var ammoMax = handStatus.EquippedWeapon.GetAmmoMax();
                return fighter.Inventory.TakeItemStack(ammoTypeId, ammoMax)?.Count ?? 0;
            };

            reloadEventArgs.Fighter.Reload(reloadEventArgs);
        }

        public void Reload(ReloadEventArgs reloadEventArgs)
        {
            if (GetHandStatus(reloadEventArgs.IsLeftHand).EquippedWeapon == null)
            {
                return;
            }

            StartCoroutine(ReloadCoroutine(reloadEventArgs));

            ReloadServerRpc(reloadEventArgs.IsLeftHand);
        }

        private IEnumerator ReloadCoroutine(ReloadEventArgs reloadEventArgs)
        {
            var handStatus = GetHandStatus(reloadEventArgs.IsLeftHand);

            handStatus.IsReloading = true;

            yield return new WaitForSeconds(handStatus.EquippedWeapon.GetReloadTime());

            //Fighter may have died during the wait
            if (!handStatus.IsReloading)
            {
                yield break;
            }

            handStatus.EquippedWeapon.Ammo = reloadEventArgs.GetNewAmmoCount();
            handStatus.IsReloading = false;

            _eventManager.After(FighterBase.EventIdReload, reloadEventArgs);
        }

        #endregion

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
            var handStatus = GetHandStatus(isLeftHand);
            var ammoTypeId = handStatus.EquippedWeapon.GetAmmoTypeId();
            return _inventory.GetItemStackTotal(ammoTypeId);
        }

        public bool TryToAttackHold(bool isLeftHand)
        {
            var leftOrRight = GetHandStatus(isLeftHand);

            if (leftOrRight.EquippedConsumer != null)
            {
                if (!ConsumeResource(leftOrRight.EquippedConsumer, isTest: true))
                {
                    return false;
                }

                var timeToCharge = leftOrRight.EquippedConsumer.GetChargeTime();
                leftOrRight.ChargeEnumerator = ConsumerChargeCoroutine(leftOrRight, DateTime.Now.AddSeconds(timeToCharge));
                StartCoroutine(leftOrRight.ChargeEnumerator);

                return true;
            }

            if (leftOrRight.EquippedWeapon != null
                && leftOrRight.EquippedWeapon.Attributes.IsAutomatic)
            {
                leftOrRight.RapidFireEnumerator = AutomaticWeaponFire(leftOrRight, leftOrRight.EquippedWeapon.GetDelayBetweenShots(), isLeftHand);
                StartCoroutine(leftOrRight.RapidFireEnumerator);
                return true;
            }

            //Debug.LogWarning("Trying to attack hold an item that is not compatible");
            return false;
        }

        private IEnumerator ConsumerChargeCoroutine(HandStatus handStatus, DateTime deadline)
        {
            var millisecondsUntilDone = (deadline - DateTime.Now).TotalMilliseconds;

            //var sw = System.Diagnostics.Stopwatch.StartNew();

            while (handStatus.EquippedConsumer.ChargePercentage < 100)
            {
                yield return new WaitForSeconds(0.01F);
                var millisecondsRemaining = (deadline - DateTime.Now).TotalMilliseconds;
                handStatus.EquippedConsumer.ChargePercentage = 100 - (int)(millisecondsRemaining / millisecondsUntilDone * 100);
            }

            //Debug.Log("Charged in: " + sw.ElapsedMilliseconds + "ms");

            handStatus.ChargeEnumerator = null;
        }

        //todo: zzz v0.4.1 - remove SoG cooldown if not necessary
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

        private IEnumerator AutomaticWeaponFire(HandStatus handStatus, float delay, bool isLeftHand)
        {
            while (handStatus?.EquippedWeapon != null && handStatus.EquippedWeapon.Ammo > 0)
            {
                TryToAttack(isLeftHand, true);
                yield return new WaitForSeconds(delay);
            }
        }

        public bool TryToAttack(bool isLeftHand, bool isAutoFire = false)
        {
            if (AliveState != LivingEntityState.Alive)
            {
                return false;
            }

            var itemInHand = isLeftHand
                ? _inventory.GetItemInSlot(HandSlotIds.LeftHand)
                : _inventory.GetItemInSlot(HandSlotIds.RightHand);

            var handPosition = isLeftHand
                ? Positions.LeftHand.position
                : Positions.RightHand.position;

            switch (itemInHand)
            {
                case null:
                    return Punch();

                case Consumer consumer:
                    return UseConsumer(isLeftHand, handPosition, consumer);

                case Weapon weaponInHand:
                    return UseWeapon(isLeftHand, handPosition, weaponInHand, isAutoFire);

                default:
                    Debug.LogWarning("Not implemented attack for " + itemInHand.Name + " yet");
                    return false;
            }
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

        private bool StopActiveConsumerBehaviour(HandStatus handStatus)
        {
            if (handStatus.ActiveConsumer == null)
            {
                return false;
            }

            StartConsumerCooldown(handStatus);

            handStatus.ActiveConsumer.StopStoppables();
            handStatus.ActiveConsumer = null;

            return true;
        }

        //todo: zzz v0.4.1 - remove SoG cooldown if not necessary
        private void StartConsumerCooldown(HandStatus handStatus)
        {
            handStatus.EquippedConsumer.ChargePercentage = 0;

            //var timeToCooldown = handStatus.EquippedSpellOrGadget.ChargePercentage / 100f * handStatus.EquippedSpellOrGadget.GetCooldownTime();
            //handStatus.CooldownEnumerator = SpellOrGadgetCooldownCoroutine(handStatus, handStatus.EquippedSpellOrGadget.ChargePercentage, DateTime.Now.AddSeconds(timeToCooldown));
            //StartCoroutine(handStatus.CooldownEnumerator);
        }

        private bool UseConsumer(bool isLeftHand, Vector3 handPosition, Consumer consumer)
        {
            if (consumer == null)
            {
                return false;
            }

            var leftOrRight = GetHandStatus(isLeftHand);

            if (StopActiveConsumerBehaviour(leftOrRight))
            {
                //Return true as the action also needs performing on the server
                return true;
            }

            if (leftOrRight.EquippedConsumer.ChargePercentage < 100)
            {
                //Debug.Log("Charge was not finished");

                if (leftOrRight.ChargeEnumerator != null)
                {
                    StopCoroutine(leftOrRight.ChargeEnumerator);
                    StartConsumerCooldown(leftOrRight);
                }

                return false;
            }

            //if (leftOrRight.CooldownEnumerator != null)
            //{
            //    //Debug.Log("Still cooling down");
            //    return false;
            //}

            if (!ConsumeResource(consumer, isTest: true))
            {
                return false;
            }

            if (consumer.Targeting.IsContinuous)
            {
                leftOrRight.ActiveConsumer = consumer;
            }
            else
            {
                StartConsumerCooldown(leftOrRight);
            }

            var attackDirection = Physics.Raycast(LookTransform.position, LookTransform.forward, out var hit, ConsumerRangeLimit)
                ? (hit.point - handPosition).normalized
                : LookTransform.forward;

            if (!IsServer)
            {
                return true;
            }

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

        private bool UseWeapon(bool isLeftHand, Vector3 handPosition, Weapon weaponInHand, bool isAutoFire)
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

            if (!weaponInHand.IsRanged || handStatus.EquippedWeapon == null)
            {
                return UseMeleeWeapon(weaponInHand);
            }

            if (handStatus.EquippedWeapon.Ammo == 0 || handStatus.IsReloading)
            {
                return false;
            }

            var requiredAmmo = Math.Min(
                1 + handStatus.EquippedWeapon.Attributes.ExtraAmmoPerShot,
                handStatus.EquippedWeapon.Ammo);

            handStatus.EquippedWeapon.Ammo -= requiredAmmo;
            return UseRangedWeapon(isLeftHand, handPosition, weaponInHand, requiredAmmo);
        }

        private bool UseRangedWeapon(bool isLeftHand, Vector3 handPosition, Weapon weaponInHand, int ammoUsed)
        {
            var shotDirection = weaponInHand.GetShotDirection(LookTransform.forward);

            var endPos = Physics.Raycast(LookTransform.position, shotDirection, out var rangedHit, MaximumRange)
                ? rangedHit.point
                : handPosition + shotDirection * MaximumRange;

            var eventArgs = GetShotFiredEventArgs(isLeftHand, handPosition, endPos);
            _eventManager.Trigger(FighterBase.EventIdShotFired, eventArgs);
            _eventManager.After(FighterBase.EventIdShotFired, eventArgs);

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
            shotFiredArgs.Fighter.ShotFired(shotFiredArgs.StartPosition, shotFiredArgs.EndPosition);
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

        protected int GetResourceCost(Consumer consumer)
        {
            //todo: zzz v0.5 - trait-based mana cost
            return consumer.GetResourceCost();
        }

        private bool ConsumeResource(Consumer consumer, bool slowDrain = false, bool isTest = false)
        {
            var resourceCost = GetResourceCost(consumer);
            var resourceTypeId = consumer.ResourceType.TypeId.ToString();

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
    }
}
