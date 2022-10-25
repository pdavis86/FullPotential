using System;
using System.Collections;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Data;
using FullPotential.Api.Gameplay.Enums;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;
using Unity.Netcode;
using UnityEngine;

namespace FullPotential.Api.Gameplay.Behaviours
{
    public abstract class FighterBase : LivingEntityBase, IFighter
    {
        private const int MeleeRangeLimit = 8;
        private const float SpellOrGadgetRangeLimit = 50f;

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

            _gameManager = ModHelper.GetGameManager();
            _rpcService = _gameManager.GetService<IRpcService>();
            _localizer = _gameManager.GetService<ILocalizer>();
        }

        protected override void Start()
        {
            base.Start();

            _consumeResource = new DelayedAction(.5f, () =>
            {
                if (HandStatusLeft.ActiveSpellOrGadgetGameObject != null
                    && !ConsumeResource(HandStatusLeft.EquippedSpellOrGadget, HandStatusLeft.EquippedSpellOrGadget.Targeting.IsContinuous))
                {
                    HandStatusLeft.StopConsumingResources();
                    StopCastingClientRpc(true, _rpcService.ForNearbyPlayers(transform.position));
                }

                if (HandStatusRight.ActiveSpellOrGadgetGameObject != null
                    && !ConsumeResource(HandStatusRight.EquippedSpellOrGadget, HandStatusRight.EquippedSpellOrGadget.Targeting.IsContinuous))
                {
                    HandStatusRight.StopConsumingResources();
                    StopCastingClientRpc(false, _rpcService.ForNearbyPlayers(transform.position));
                }
            });
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            _consumeResource.TryPerformAction();
        }

        // ReSharper restore UnusedMemberHierarchy.Global
        #endregion

        #region ServerRpc calls

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

            leftOrRight.StopConsumingResources();
        }

        #endregion

        public override bool IsConsumingEnergy()
        {
            return HandStatusLeft.IsConsumingEnergy() || HandStatusRight.IsConsumingEnergy();
        }

        public override bool IsConsumingMana()
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

            yield return new WaitForSeconds(handStatus.EquippedWeapon.Attributes.GetReloadTime());

            handStatus.EquippedWeapon.Ammo = handStatus.EquippedWeapon.Attributes.GetAmmoMax();

            handStatus.IsReloading = false;
        }

        private int GetAttributeValue(AffectableAttribute attribute)
        {
            //todo: trait-based attributes
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
            HandStatusLeft.StopConsumingResources();
            HandStatusRight.StopConsumingResources();

            base.HandleDeath();
        }

        public bool TryToAttack(bool isLeftHand)
        {
            var itemInHand = isLeftHand
                ? _inventory.GetItemInSlot(SlotGameObjectName.LeftHand)
                : _inventory.GetItemInSlot(SlotGameObjectName.RightHand);

            var handPosition = isLeftHand
                ? Positions.LeftHandInFront.position
                : Positions.RightHandInFront.position;

            switch (itemInHand)
            {
                case null:
                    return Punch() != null;

                case Gadget:
                case Spell:
                    return UseSpellOrGadget(isLeftHand, handPosition, itemInHand as SpellOrGadgetItemBase);

                case Weapon weaponInHand:
                    return UseWeapon(isLeftHand, handPosition, weaponInHand) != null;

                default:
                    Debug.LogWarning("Not implemented attack for " + itemInHand.Name + " yet");
                    return false;
            }
        }

        private Vector3 GetAttackDirection(Vector3 handPosition, float maxDistance)
        {
            return Physics.Raycast(LookTransform.position, LookTransform.forward, out var hit, maxDistance: maxDistance)
                ? (hit.point - handPosition).normalized
                : LookTransform.forward;
        }

        private ulong? Punch()
        {
            if (!Physics.Raycast(LookTransform.position, LookTransform.forward, out var hit, MeleeRangeLimit))
            {
                return null;
            }

            if (IsServer)
            {
                _effectService.ApplyEffects(this, null, hit.transform.gameObject, hit.point);
            }

            return hit.transform.gameObject.GetComponent<NetworkObject>().NetworkObjectId;
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

            if (leftOrRight.StopConsumingResources())
            {
                //Return true as the action also needs performing on the server
                return true;
            }

            if (!ConsumeResource(spellOrGadget, isTest: true))
            {
                return false;
            }

            var targetDirection = GetAttackDirection(handPosition, SpellOrGadgetRangeLimit);

            var parentTransform = spellOrGadget.Targeting.IsParentedToSource
                ? transform
                : _gameManager.GetSceneBehaviour().GetTransform();

            _typeRegistry.LoadAddessable(
                spellOrGadget.Targeting.PrefabAddress,
                prefab =>
                {
                    var spellOrGadgetGameObject = Instantiate(prefab, handPosition, Quaternion.identity);

                    spellOrGadget.Targeting.SetBehaviourVariables(spellOrGadgetGameObject, spellOrGadget, this, handPosition, targetDirection, isLeftHand);

                    spellOrGadgetGameObject.transform.parent = parentTransform;

                    if (spellOrGadget.Targeting.IsContinuous)
                    {
                        leftOrRight.ActiveSpellOrGadgetGameObject = spellOrGadgetGameObject;
                    }

                    if (IsServer)
                    {
                        ConsumeResource(spellOrGadget);
                    }
                }
            );

            if (spellOrGadget.Targeting.IsServerSideOnly && IsServer)
            {
                return false;
            }

            return true;
        }

        private ulong? UseWeapon(bool isLeftHand, Vector3 handPosition, Weapon weaponInHand)
        {
            var registryType = (IGearWeapon)weaponInHand.RegistryType;

            var isRanged = registryType.Category == IGearWeapon.WeaponCategory.Ranged;

            var handStatus = isLeftHand
                ? HandStatusLeft
                : HandStatusRight;

            if (!isRanged || handStatus.EquippedWeapon == null)
            {
                return UseMeleeWeapon(weaponInHand);
            }

            if (handStatus.EquippedWeapon.Ammo == 0 || handStatus.IsReloading)
            {
                return null;
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

        private ulong? UseRangedWeapon(Vector3 handPosition, Weapon weaponInHand, int ammoUsed)
        {
            //todo: handle multiple shots 

            var range = weaponInHand.Attributes.GetProjectileRange();
            var endPos = Physics.Raycast(LookTransform.position, LookTransform.forward, out var rangedHit, range)
                ? rangedHit.point
                : handPosition + LookTransform.forward * range;

            var nearbyClients = _rpcService.ForNearbyPlayers(transform.position);
            UsedWeaponClientRpc(handPosition, endPos, nearbyClients);

            if (rangedHit.transform == null)
            {
                return null;
            }

            if (IsServer)
            {
                //Debug.Log("Should only be called once on server");
                _effectService.ApplyEffects(this, weaponInHand, rangedHit.transform.gameObject, rangedHit.point);
            }

            var rangedHitNetworkObject = rangedHit.transform.gameObject.GetComponent<NetworkObject>();
            return rangedHitNetworkObject != null ? rangedHitNetworkObject.NetworkObjectId : null;
        }

        private ulong? UseMeleeWeapon(Weapon weaponInHand)
        {
            var meleeRange = weaponInHand.IsTwoHanded
                ? MeleeRangeLimit
                : MeleeRangeLimit / 2;

            if (!Physics.Raycast(LookTransform.position, LookTransform.forward, out var meleeHit, maxDistance: meleeRange))
            {
                return null;
            }

            if (IsServer)
            {
                _effectService.ApplyEffects(this, weaponInHand, meleeHit.transform.gameObject, meleeHit.point);
            }

            var hitNetworkObject = meleeHit.transform.gameObject.GetComponent<NetworkObject>();

            if (hitNetworkObject == null)
            {
                return null;
            }

            return hitNetworkObject.NetworkObjectId;
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
