﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Data;
using FullPotential.Api.Gameplay.Enums;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Base;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Localization;
using FullPotential.Core.PlayerBehaviours;
using FullPotential.Core.Ui.Components;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace FullPotential.Core.Gameplay.Combat
{
    public abstract class FighterBase : NetworkBehaviour, IFighter
    {
        private const int MeleeRangeLimit = 8;
        private const float SpellOrGadgetRangeLimit = 50f;

        #region Inspector Variables
#pragma warning disable 0649
        // ReSharper disable UnassignedField.Global
        // ReSharper disable InconsistentNaming

        public PositionTransforms Positions;
        public BodyPartTransforms BodyParts;
        [SerializeField] protected TextMeshProUGUI _nameTag;
        [SerializeField] protected BarSlider _healthSlider;

        // ReSharper enable UnassignedField.Global
        // ReSharper enable InconsistentNaming
#pragma warning restore 0649
        #endregion

        #region Other Variables

        private readonly Dictionary<ulong, long> _damageTaken = new Dictionary<ulong, long>();
        private readonly Dictionary<IEffect, DateTime> _activeEffects = new Dictionary<IEffect, DateTime>();

        private Rigidbody _rb;
        private bool _isSprinting;

        // ReSharper disable InconsistentNaming
        protected GameManager _gameManager;
        protected IRpcService _rpcService;
        protected Localizer _localizer;

        protected readonly NetworkVariable<int> _energy = new NetworkVariable<int>(100);
        protected readonly NetworkVariable<int> _health = new NetworkVariable<int>(100);
        protected readonly NetworkVariable<int> _mana = new NetworkVariable<int>(100);
        protected readonly NetworkVariable<int> _stamina = new NetworkVariable<int>(100);
        // ReSharper enable InconsistentNaming

        public readonly HandStatus HandStatusLeft = new HandStatus();
        public readonly HandStatus HandStatusRight = new HandStatus();

        protected IInventory _inventory;
        protected IEffectService _effectService;
        protected ITypeRegistry _typeRegistry;

        //Action-related
        private DelayedAction _replenishStamina;
        private DelayedAction _replenishMana;
        private DelayedAction _replenishEnergy;
        private DelayedAction _consumeStamina;
        private DelayedAction _consumeResource;

        #endregion

        #region Properties
        public abstract Transform Transform { get; }

        public abstract GameObject GameObject { get; }

        public Rigidbody RigidBody => _rb == null ? _rb = GetComponent<Rigidbody>() : _rb;

        public abstract Transform LookTransform { get; }

        public abstract string FighterName { get; }

        public LivingEntityState AliveState { get; protected set; }

        #endregion

        #region Unity Events Handlers
        // ReSharper disable UnusedMemberHierarchy.Global

        protected virtual void Awake()
        {
            _gameManager = GameManager.Instance;
            _rpcService = _gameManager.GetService<IRpcService>();
            _localizer = _gameManager.GetService<Localizer>();

            if (IsServer)
            {
                InvokeRepeating(nameof(CheckIfOffTheMap), 1, 1);
            }
        }

        protected virtual void Start()
        {
            AliveState = LivingEntityState.Alive;

            _replenishStamina = new DelayedAction(.01f, () =>
            {
                if (!_isSprinting && _stamina.Value < GetStaminaMax())
                {
                    //todo: trait-based stamina recharge
                    _stamina.Value += 1;
                }
            });

            _replenishMana = new DelayedAction(.2f, () =>
            {
                var isConsumingMana = HandStatusLeft.IsConsumingMana() || HandStatusRight.IsConsumingMana();
                if (!isConsumingMana && _mana.Value < GetManaMax())
                {
                    //todo: trait-based mana recharge
                    _mana.Value += 1;
                }
            });

            _replenishEnergy = new DelayedAction(.2f, () =>
            {
                var isConsumingEnergy = HandStatusLeft.IsConsumingEnergy() || HandStatusRight.IsConsumingEnergy();
                if (!isConsumingEnergy && _energy.Value < GetEnergyMax())
                {
                    //todo: trait-based energy recharge
                    _energy.Value += 1;
                }
            });

            _consumeStamina = new DelayedAction(.05f, () =>
            {
                var staminaCost = GetStaminaCost();
                if (_isSprinting && _stamina.Value >= staminaCost)
                {
                    _stamina.Value -= staminaCost / 2;
                }
            });

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

        protected virtual void FixedUpdate()
        {
            ReplenishAndConsume();
        }

        // ReSharper enable UnusedMemberHierarchy.Global
        #endregion

        #region ServerRpc calls

        [ServerRpc]
        public void TryToAttackServerRpc(bool isLeftHand)
        {
            if (TryToAttack(isLeftHand))
            {
                var nearbyClients = _rpcService.ForNearbyPlayersExcept(transform.position, OwnerClientId);
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

            var nearbyClients = _rpcService.ForNearbyPlayersExcept(transform.position, OwnerClientId);
            ReloadingClientRpc(isLeftHand, nearbyClients);
        }

        [ServerRpc]
        public void UpdateSprintingServerRpc(bool isSprinting)
        {
            _isSprinting = isSprinting;
        }

        #endregion

        #region ClientRpc calls

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void UsedWeaponClientRpc(Vector3 startPosition, Vector3 endPosition, ClientRpcParams clientRpcParams)
        {
            var projectile = Instantiate(
                _gameManager.Prefabs.Combat.ProjectileWithTrail,
                startPosition,
                Quaternion.identity);

            var projectileScript = projectile.GetComponent<ProjectileWithTrail>();
            projectileScript.TargetPosition = endPosition;
            projectileScript.Speed = 500;
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

        public virtual int GetHealth()
        {
            return _health.Value;
        }

        public virtual int GetHealthMax()
        {
            //todo: trait-based health max
            return 100;
        }

        public virtual int GetStamina()
        {
            return _stamina.Value;
        }

        public int GetStaminaMax()
        {
            //todo: trait-based stamina max
            return 100;
        }

        public int GetStaminaCost()
        {
            //todo: trait-based stamina cost
            return 10;
        }

        public float GetSprintSpeed()
        {
            //todo: trait-based sprint speed
            return 2.5f;
        }

        public int GetMana()
        {
            return _mana.Value;
        }

        public int GetManaMax()
        {
            //todo: trait-based mana max
            return 100;
        }

        private int GetManaCost(Spell spell)
        {
            //todo: trait-based mana cost
            return 20;
        }

        public int GetEnergy()
        {
            return _energy.Value;
        }

        public int GetEnergyMax()
        {
            //todo: trait-based energy max
            return 100;
        }

        private int GetEnergyCost(Gadget gadget)
        {
            //todo: trait-based energy cost
            return 20;
        }

        public virtual int GetDefenseValue()
        {
            //todo: trait-based defense
            return 50;
        }

        public void TakeDamage(IFighter sourceFighter,
            ItemBase itemUsed,
            Vector3? position)
        {
            var sourceIsPlayer = sourceFighter != null && sourceFighter.GameObject.CompareTag(Tags.Player);
            var sourcePlayerState = sourceIsPlayer ? sourceFighter.GameObject.GetComponent<PlayerState>() : null;

            var targetFighter = this;

            var damageDealt = AttributeCalculator.GetAttackValue(itemUsed?.Attributes, targetFighter.GetDefenseValue());

            var sourceName = sourceIsPlayer
                ? sourcePlayerState.Username
                : (sourceFighter != null ? sourceFighter.FighterName : null).OrIfNullOrWhitespace(_localizer.Translate("ui.alert.unknownattacker"));
            var sourceItemName = itemUsed?.Name ?? _localizer.Translate("ui.alert.attack.noitem");
            var sourceNetworkObject = sourceFighter != null ? sourceFighter.GameObject.GetComponent<NetworkObject>() : null;
            var sourceClientId = sourceNetworkObject != null ? (ulong?)sourceNetworkObject.OwnerClientId : null;

            if (sourceClientId != null)
            {
                if (_damageTaken.ContainsKey(sourceClientId.Value))
                {
                    _damageTaken[sourceClientId.Value] += damageDealt;
                }
                else
                {
                    _damageTaken.Add(sourceClientId.Value, damageDealt);
                }
            }

            if (sourceFighter == null)
            {
                Debug.LogWarning("Attack source not found. Did they sign out?");
                return;
            }

            if (itemUsed == null)
            {
                var targetRb = GetComponent<Rigidbody>();
                if (targetRb != null && position.HasValue)
                {
                    targetRb.AddForceAtPosition(sourceFighter.Transform.forward * 150, position.Value);
                }
            }

            if (sourceIsPlayer && position.HasValue && !ReferenceEquals(sourceFighter, this))
            {
                sourceFighter.GameObject.GetComponent<PlayerActions>().ShowDamageClientRpc(
                    position.Value,
                    damageDealt.ToString(CultureInfo.InvariantCulture),
                    _rpcService.ForPlayer(sourcePlayerState.OwnerClientId));
            }

            _health.Value -= damageDealt;

            CheckStats(sourceName, sourceItemName);
        }

        public virtual void HandleDeath(string killerName, string itemName)
        {
            AliveState = LivingEntityState.Dead;

            GetComponent<Collider>().enabled = false;

            foreach (var (clientId, _) in _damageTaken)
            {
                if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
                {
                    continue;
                }

                var playerState = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerState>();
                playerState.SpawnLootChest(transform.position);
            }

            _damageTaken.Clear();

            HandStatusLeft.StopConsumingResources();
            HandStatusRight.StopConsumingResources();

            var deathMessage = GetDeathMessage(false, name, killerName, itemName);
            var nearbyClients = _rpcService.ForNearbyPlayers(transform.position);
            _gameManager.GetSceneBehaviour().MakeAnnouncementClientRpc(deathMessage, nearbyClients);
        }

        public void CheckIfOffTheMap()
        {
            if (AliveState != LivingEntityState.Dead
                && transform.position.y < _gameManager.GetSceneBehaviour().Attributes.LowestYValue)
            {
                HandleDeath(_localizer.Translate("ui.alert.falldamage"), null);
            }
        }

        private void CheckStats(string sourceName, string sourceItemName)
        {
            if (_health.Value <= 0)
            {
                HandleDeath(sourceName, sourceItemName);
            }

            //todo: other min values
        }

        protected string GetDeathMessage(bool isOwner, string victimName, string killerName, string itemName)
        {
            if (itemName.IsNullOrWhiteSpace())
            {
                return isOwner
                    ? string.Format(_localizer.Translate("ui.alert.attack.youwerekilledby"), killerName)
                    : string.Format(_localizer.Translate("ui.alert.attack.victimkilledby"), victimName, killerName);
            }

            return isOwner
                ? string.Format(_localizer.Translate("ui.alert.attack.youwerekilledbyusing"), killerName, itemName)
                : string.Format(_localizer.Translate("ui.alert.attack.victimkilledbyusing"), victimName, killerName, itemName);
        }

        public bool TryToAttack(bool isLeftHand)
        {
            var handStatus = isLeftHand
                ? HandStatusLeft
                : HandStatusRight;

            if (handStatus.EquippedWeapon != null)
            {
                if (handStatus.EquippedWeapon.Ammo == 0 || handStatus.IsReloading)
                {
                    return false;
                }

                if (handStatus.EquippedWeapon.Ammo > 0)
                {
                    handStatus.EquippedWeapon.Ammo -= 1 + handStatus.EquippedWeapon.Attributes.ExtraAmmoPerShot;
                }
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
                    return Punch() != null;

                case Gadget:
                case Spell:
                    return UseSpellOrGadget(isLeftHand, handPosition, itemInHand as SpellOrGadgetItemBase);

                case Weapon weaponInHand:
                    return UseWeapon(handPosition, weaponInHand) != null;

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

            if (!ConsumeResource(spellOrGadget, isTest: true))
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

        private ulong? UseWeapon(Vector3 handPosition, Weapon weaponInHand)
        {
            var registryType = (IGearWeapon)weaponInHand.RegistryType;

            return registryType.Category == IGearWeapon.WeaponCategory.Ranged
                ? UseRangedWeapon(handPosition, weaponInHand)
                : UseMeleeWeapon(weaponInHand);
        }

        private ulong? UseRangedWeapon(Vector3 handPosition, Weapon weaponInHand)
        {
            //todo: automatic weapons

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

            return meleeHit.transform.gameObject.GetComponent<NetworkObject>().NetworkObjectId;
        }

        public void AddAttributeModifier(IAttributeEffect attributeEffect, Attributes attributes)
        {
            //todo: AddAttributeModifier
            throw new NotImplementedException();
        }

        public void ApplyPeriodicActionToStat(IStatEffect statEffect, Attributes attributes)
        {
            //todo: ApplyPeriodicActionToStat
            throw new NotImplementedException();
        }

        public void ApplyStatValueChange(IStatEffect statEffect, ItemBase itemUsed, IFighter sourceFighter, Vector3? position)
        {
            const float displayTimeForSingleChangeToStat = 2f;

            var statVariable = GetStatVariable(statEffect.StatToAffect);
            var statMax = GetStatVariableMax(statEffect.StatToAffect);

            var (change, duration) = AttributeCalculator.GetStatChangeAndDuration(itemUsed.Attributes);

            if (_activeEffects.ContainsKey(statEffect))
            {
                _activeEffects.Remove(statEffect);
            }
            _activeEffects.Add(statEffect, DateTime.Now.AddSeconds(displayTimeForSingleChangeToStat));

            if (statEffect.Affect == Affect.SingleDecrease && statEffect.StatToAffect == AffectableStat.Health)
            {
                TakeDamage(sourceFighter, itemUsed, position);
                return;
            }

            if (statVariable.Value >= statMax)
            {
                return;
            }

            if (statEffect.Affect == Affect.SingleIncrease)
            {
                if (statVariable.Value < statMax - change)
                {
                    statVariable.Value += change;
                }
                else
                {
                    statVariable.Value = statMax;
                }
                return;
            }

            if (statVariable.Value - change >= 0)
            {
                statVariable.Value -= change;
            }
            else
            {
                statVariable.Value = 0;

                CheckStats(FighterName, itemUsed.Name);
            }
        }

        public void ApplyTemporaryMaxActionToStat(IStatEffect statEffect, Attributes attributes)
        {
            //todo: ApplyTemporaryMaxActionToStat
            throw new NotImplementedException();
        }

        public void ApplyElementalEffect(IEffect elementalEffect, Attributes attributes)
        {
            //todo: ApplyElementalEffect
            throw new NotImplementedException();
        }

        public void BeginMaintainDistanceOn(GameObject targetGameObject)
        {
            //todo: BeginMaintainDistanceOn
            throw new NotImplementedException();
        }

        public Dictionary<IEffect, float> GetActiveEffects()
        {
            var expiredEffects = _activeEffects
                .Where(x => x.Value < DateTime.Now)
                .ToList();

            foreach (var kvp in expiredEffects)
            {
                _activeEffects.Remove(kvp.Key);
            }

            return _activeEffects.ToDictionary(
                x => x.Key,
                x => (float)(x.Value - DateTime.Now).TotalSeconds);
        }

        private NetworkVariable<int> GetStatVariable(AffectableStat stat)
        {
            switch (stat)
            {
                case AffectableStat.Energy: return _energy;
                case AffectableStat.Health: return _health;
                case AffectableStat.Mana: return _mana;
                case AffectableStat.Stamina: return _stamina;
                default:
                    throw new ArgumentException("Unexpected AffectableStat: " + stat);
            }
        }

        private int GetStatVariableMax(AffectableStat stat)
        {
            switch (stat)
            {
                case AffectableStat.Energy: return GetEnergyMax();
                case AffectableStat.Health: return GetHealthMax();
                case AffectableStat.Mana: return GetManaMax();
                case AffectableStat.Stamina: return GetStaminaMax();
                default:
                    throw new ArgumentException("Unexpected AffectableStat: " + stat);
            }
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

        private void ReplenishAndConsume()
        {
            if (!IsServer)
            {
                return;
            }

            _replenishStamina.TryPerformAction();
            _replenishMana.TryPerformAction();
            _replenishEnergy.TryPerformAction();

            _consumeStamina.TryPerformAction();
            _consumeResource.TryPerformAction();
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

        // ReSharper enable UnassignedField.Global
        #endregion

    }
}
