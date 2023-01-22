using System;
using System.Collections;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Data;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Gameplay.Items;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry.Crafting;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Api.Utilities;
using Unity.Netcode;
using UnityEngine;

namespace FullPotential.Api.Gameplay.Behaviours
{
    public abstract class FighterBase : LivingEntityBase, IFighter, IMoveable
    {
        private const int MeleeRangeLimit = 8;
        private const int SpellOrGadgetRangeLimit = 50;
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

        protected IInventory _inventory;

        // ReSharper restore InconsistentNaming
        #endregion

        #region Other Variables

        public readonly HandStatus HandStatusLeft = new HandStatus();
        public readonly HandStatus HandStatusRight = new HandStatus();
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

            _gameManager = DependenciesContext.Dependencies.GetService<IModHelper>().GetGameManager();
            _rpcService = DependenciesContext.Dependencies.GetService<IRpcService>();
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
        }

        protected override void Start()
        {
            base.Start();

            _consumeResource = new DelayedAction(.5f, () =>
            {
                if (HandStatusLeft.ActiveSpellOrGadgetBehaviour != null
                    && !ConsumeResource(HandStatusLeft.EquippedSpellOrGadget, HandStatusLeft.EquippedSpellOrGadget.Targeting.IsContinuous))
                {
                    StopActiveSpellOrGadgetBehaviour(HandStatusLeft);
                    StopCastingClientRpc(true, _rpcService.ForNearbyPlayers(transform.position));
                }

                if (HandStatusRight.ActiveSpellOrGadgetBehaviour != null
                    && !ConsumeResource(HandStatusRight.EquippedSpellOrGadget, HandStatusRight.EquippedSpellOrGadget.Targeting.IsContinuous))
                {
                    StopActiveSpellOrGadgetBehaviour(HandStatusRight);
                    StopCastingClientRpc(false, _rpcService.ForNearbyPlayers(transform.position));
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
        public void ReloadServerRpc(bool isLeftHand)
        {
            var leftOrRight = isLeftHand
                ? HandStatusLeft
                : HandStatusRight;

            StartCoroutine(ReloadCoroutine(leftOrRight));

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
            var leftOrRight = isLeftHand
                ? HandStatusLeft
                : HandStatusRight;

            StartCoroutine(ReloadCoroutine(leftOrRight));
        }

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void StopCastingClientRpc(bool isLeftHand, ClientRpcParams clientRpcParams)
        {
            var leftOrRight = isLeftHand
                ? HandStatusLeft
                : HandStatusRight;

            StopActiveSpellOrGadgetBehaviour(leftOrRight);
        }

        [ClientRpc]
        public void ApplyMovementForceClientRpc(Vector3 force, ForceMode forceMode, ClientRpcParams clientRpcParams)
        {
            var targetRigidBody = GetComponent<Rigidbody>();
            targetRigidBody.AddForce(force, forceMode);
        }

        #endregion

        protected override bool IsConsumingEnergy()
        {
            return HandStatusLeft.IsConsumingEnergy() || HandStatusRight.IsConsumingEnergy();
        }

        protected override bool IsConsumingMana()
        {
            return HandStatusLeft.IsConsumingMana() || HandStatusRight.IsConsumingMana();
        }

        public IEnumerator ReloadCoroutine(HandStatus handStatus)
        {
            if (handStatus.EquippedWeapon == null)
            {
                yield break;
            }

            handStatus.IsReloading = true;

            yield return new WaitForSeconds(handStatus.EquippedWeapon.GetReloadTime());

            handStatus.EquippedWeapon.Ammo = handStatus.EquippedWeapon.GetAmmoMax();

            handStatus.IsReloading = false;
        }

        private int GetAttributeValue(AffectableAttribute attribute)
        {
            //todo: zzz v0.5 - trait-based attributes
            switch (attribute)
            {
                case AffectableAttribute.Strength:
                    return 0 + GetAttributeAdjustment(AffectableAttribute.Strength);

                default:
                    throw new Exception("Not yet implemented GetAttributeValue() for " + attribute);
            }
        }

        public override int GetDefenseValue()
        {
            return _inventory.GetDefenseValue() + GetAttributeValue(AffectableAttribute.Strength);
        }

        public override void HandleDeath()
        {
            StopActiveSpellOrGadgetBehaviour(HandStatusLeft);
            StopActiveSpellOrGadgetBehaviour(HandStatusRight);

            base.HandleDeath();
        }

        public bool TryToAttackHold(bool isLeftHand)
        {
            var leftOrRight = isLeftHand
                ? HandStatusLeft
                : HandStatusRight;

            if (leftOrRight.EquippedSpellOrGadget != null)
            {
                if (!ConsumeResource(leftOrRight.EquippedSpellOrGadget, isTest: true))
                {
                    return false;
                }

                var timeToCharge = leftOrRight.EquippedSpellOrGadget.GetChargeTime();
                leftOrRight.ChargeEnumerator = SpellOrGadgetChargeCoroutine(leftOrRight, DateTime.Now.AddSeconds(timeToCharge));
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

            Debug.LogWarning("Trying to attack hold an item that is not compatible");
            return false;
        }

        private IEnumerator SpellOrGadgetChargeCoroutine(HandStatus handStatus, DateTime deadline)
        {
            var millisecondsUntilDone = (deadline - DateTime.Now).TotalMilliseconds;

            //var sw = System.Diagnostics.Stopwatch.StartNew();

            while (handStatus.EquippedSpellOrGadget.ChargePercentage < 100)
            {
                yield return new WaitForSeconds(0.01F);
                var millisecondsRemaining = (deadline - DateTime.Now).TotalMilliseconds;
                handStatus.EquippedSpellOrGadget.ChargePercentage = 100 - (int)(millisecondsRemaining / millisecondsUntilDone * 100);
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
                ? _inventory.GetItemInSlot(SlotGameObjectName.LeftHand)
                : _inventory.GetItemInSlot(SlotGameObjectName.RightHand);

            var handPosition = isLeftHand
                ? Positions.LeftHandInFront.position
                : Positions.RightHandInFront.position;

            switch (itemInHand)
            {
                case null:
                    return Punch();

                case Gadget:
                case Spell:
                    return UseSpellOrGadget(isLeftHand, handPosition, itemInHand as SpellOrGadgetItemBase);

                case Weapon weaponInHand:
                    return UseWeapon(isLeftHand, handPosition, weaponInHand, isAutoFire);

                default:
                    Debug.LogWarning("Not implemented attack for " + itemInHand.Name + " yet");
                    return false;
            }
        }

        private Vector3 GetAttackDirection(Vector3 handPosition, float maxDistance)
        {
            return Physics.Raycast(LookTransform.position, LookTransform.forward, out var hit, maxDistance)
                ? (hit.point - handPosition).normalized
                : LookTransform.forward;
        }

        private bool Punch()
        {
            if (IsServer)
            {
                if (Physics.Raycast(LookTransform.position, LookTransform.forward, out var hit, MeleeRangeLimit))
                {
                    _effectService.ApplyEffects(this, null, hit.transform.gameObject, hit.point);
                }
            }

            return true;
        }

        private bool StopActiveSpellOrGadgetBehaviour(HandStatus handStatus)
        {
            if (handStatus.ActiveSpellOrGadgetBehaviour == null)
            {
                return false;
            }

            StartSpellOrGadgetCooldown(handStatus);

            handStatus.ActiveSpellOrGadgetBehaviour.Stop();
            handStatus.ActiveSpellOrGadgetBehaviour = null;

            return true;
        }

        //todo: zzz v0.4.1 - remove SoG cooldown if not necessary
        private void StartSpellOrGadgetCooldown(HandStatus handStatus)
        {
            handStatus.EquippedSpellOrGadget.ChargePercentage = 0;

            //var timeToCooldown = handStatus.EquippedSpellOrGadget.ChargePercentage / 100f * handStatus.EquippedSpellOrGadget.GetCooldownTime();
            //handStatus.CooldownEnumerator = SpellOrGadgetCooldownCoroutine(handStatus, handStatus.EquippedSpellOrGadget.ChargePercentage, DateTime.Now.AddSeconds(timeToCooldown));
            //StartCoroutine(handStatus.CooldownEnumerator);
        }

        private bool UseSpellOrGadget(bool isLeftHand, Vector3 handPosition, SpellOrGadgetItemBase spellOrGadget)
        {
            if (spellOrGadget == null)
            {
                return false;
            }

            var leftOrRight = isLeftHand
                ? HandStatusLeft
                : HandStatusRight;

            if (StopActiveSpellOrGadgetBehaviour(leftOrRight))
            {
                //Return true as the action also needs performing on the server
                return true;
            }

            if (IsServer || NetworkManager.LocalClientId == OwnerClientId)
            {
                if (leftOrRight.EquippedSpellOrGadget.ChargePercentage < 100)
                {
                    //Debug.Log("Charge was not finished");

                    if (leftOrRight.ChargeEnumerator != null)
                    {
                        StopCoroutine(leftOrRight.ChargeEnumerator);
                        StartSpellOrGadgetCooldown(leftOrRight);
                    }

                    return false;
                }

                if (leftOrRight.CooldownEnumerator != null)
                {
                    //Debug.Log("Still cooling down");
                    return false;
                }

                if (!ConsumeResource(spellOrGadget, isTest: true))
                {
                    return false;
                }
            }

            _typeRegistry.LoadAddessable(
                spellOrGadget.Targeting.PrefabAddress,
                prefab => InstantiateSogGameObject(leftOrRight, isLeftHand, handPosition, spellOrGadget, prefab));

            if (spellOrGadget.Targeting.IsServerSideOnly && IsServer)
            {
                return false;
            }

            return true;
        }

        private void InstantiateSogGameObject(
            HandStatus leftOrRight,
            bool isLeftHand,
            Vector3 handPosition,
            SpellOrGadgetItemBase spellOrGadget,
            GameObject prefab)
        {
            var targetDirection = GetAttackDirection(handPosition, SpellOrGadgetRangeLimit);

            var parentTransform = spellOrGadget.Targeting.IsParentedToSource
                ? transform
                : _gameManager.GetSceneBehaviour().GetTransform();

            var spellOrGadgetGameObject = Instantiate(prefab, handPosition, Quaternion.identity);

            spellOrGadget.Targeting.SetBehaviourVariables(spellOrGadgetGameObject, spellOrGadget, this, handPosition, targetDirection, isLeftHand);

            spellOrGadgetGameObject.transform.parent = parentTransform;

            if (spellOrGadget.Targeting.IsContinuous)
            {
                leftOrRight.ActiveSpellOrGadgetBehaviour = spellOrGadgetGameObject.GetComponent<ISpellOrGadgetBehaviour>();
            }
            else
            {
                StartSpellOrGadgetCooldown(leftOrRight);
            }

            if (IsServer)
            {
                ConsumeResource(spellOrGadget);
            }
        }

        private bool UseWeapon(bool isLeftHand, Vector3 handPosition, Weapon weaponInHand, bool isAutoFire)
        {
            var registryType = (IGearWeapon)weaponInHand.RegistryType;

            var isRanged = registryType.Category == IGearWeapon.WeaponCategory.Ranged;

            var handStatus = isLeftHand
                ? HandStatusLeft
                : HandStatusRight;

            if (!isAutoFire && handStatus.RapidFireEnumerator != null)
            {
                StopCoroutine(handStatus.RapidFireEnumerator);
                handStatus.RapidFireEnumerator = null;
                return true;
            }

            if (!isRanged || handStatus.EquippedWeapon == null)
            {
                return UseMeleeWeapon(weaponInHand);
            }

            if (handStatus.EquippedWeapon.Ammo == 0 || handStatus.IsReloading)
            {
                return false;
            }

            var requiredAmmo = 1 + handStatus.EquippedWeapon.Attributes.ExtraAmmoPerShot;

            if (handStatus.EquippedWeapon.Ammo >= requiredAmmo)
            {
                handStatus.EquippedWeapon.Ammo -= requiredAmmo;
                return UseRangedWeapon(handPosition, weaponInHand, requiredAmmo);
            }

            var ammoUsed = handStatus.EquippedWeapon.Ammo;
            handStatus.EquippedWeapon.Ammo = 0;
            return UseRangedWeapon(handPosition, weaponInHand, ammoUsed);
        }

        private bool UseRangedWeapon(Vector3 handPosition, Weapon weaponInHand, int ammoUsed)
        {
            var shotDirection = weaponInHand.GetShotDirection(LookTransform.forward);

            var endPos = Physics.Raycast(LookTransform.position, shotDirection, out var rangedHit, MaximumRange)
                ? rangedHit.point
                : handPosition + shotDirection * MaximumRange;

            var nearbyClients = _rpcService.ForNearbyPlayers(transform.position);
            UsedWeaponClientRpc(handPosition, endPos, nearbyClients);

            if (rangedHit.transform == null)
            {
                return false;
            }

            if (IsServer)
            {
                //Debug.Log("Should only be called once on server");
                for (var i = 0; i < ammoUsed; i++)
                {
                    _effectService.ApplyEffects(this, weaponInHand, rangedHit.transform.gameObject, rangedHit.point);
                }
            }

            return true;
        }

        private bool UseMeleeWeapon(Weapon weaponInHand)
        {
            //todo: take speed and recovery into account

            if (IsServer)
            {
                var meleeRange = weaponInHand.IsTwoHanded
                    ? MeleeRangeLimit
                    : MeleeRangeLimit / 2;

                Physics.Raycast(LookTransform.position, LookTransform.forward, out var meleeHit, meleeRange);
                _effectService.ApplyEffects(this, weaponInHand, meleeHit.transform.gameObject, meleeHit.point);
            }

            return true;
        }

        private (NetworkVariable<int> Variable, int? Cost)? GetResourceVariableAndCost(SpellOrGadgetItemBase spellOrGadget)
        {
            switch (spellOrGadget.ResourceConsumptionType)
            {
                case ResourceConsumptionType.Mana:
                    return (_mana, GetManaCost((Spell)spellOrGadget));

                case ResourceConsumptionType.Energy:
                    return (_energy, GetEnergyCost((Gadget)spellOrGadget));

                default:
                    Debug.LogError("Not yet implemented GetResourceVariable() for resource type " + spellOrGadget.ResourceConsumptionType);
                    return null;
            }
        }

        private bool ConsumeResource(SpellOrGadgetItemBase spellOrGadget, bool slowDrain = false, bool isTest = false)
        {
            var tuple = GetResourceVariableAndCost(spellOrGadget);

            if (!tuple.HasValue || !tuple.Value.Cost.HasValue)
            {
                Debug.LogError("Failed to get GetResourceVariableAndCost");
                return false;
            }

            var resourceCost = tuple.Value.Cost.Value;
            var resourceVariable = tuple.Value.Variable;

            if (slowDrain)
            {
                resourceCost = (int)Math.Ceiling(resourceCost / 10f) + 1;
            }

            if (resourceVariable.Value < resourceCost)
            {
                return false;
            }

            if (!isTest)
            {
                resourceVariable.Value -= resourceCost;
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
            public Transform LeftHandInFront;
            public Transform RightHandInFront;
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
