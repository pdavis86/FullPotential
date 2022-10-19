using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Data;
using FullPotential.Api.Gameplay.Enums;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Base;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Api.Ui.Components;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace FullPotential.Api.Gameplay.Combat
{
    public abstract class FighterBase : NetworkBehaviour, IFighter
    {
        private const int MeleeRangeLimit = 8;
        private const float SpellOrGadgetRangeLimit = 50f;

        #region Inspector Variables
        // ReSharper disable UnassignedField.Global
        // ReSharper disable InconsistentNaming

        public PositionTransforms Positions;
        public BodyPartTransforms BodyParts;
        [SerializeField] protected TextMeshProUGUI _nameTag;

        // ReSharper restore UnassignedField.Global
        // ReSharper restore InconsistentNaming
        #endregion

        #region Protected variables
        // ReSharper disable InconsistentNaming

        protected string _lastDamageSourceName;
        protected string _lastDamageItemName;

        protected IGameManager _gameManager;
        protected IRpcService _rpcService;
        protected ILocalizer _localizer;

        protected readonly NetworkVariable<FixedString32Bytes> _fighterName = new NetworkVariable<FixedString32Bytes>();
        protected readonly NetworkVariable<int> _energy = new NetworkVariable<int>(100);
        protected readonly NetworkVariable<int> _health = new NetworkVariable<int>(100);
        protected readonly NetworkVariable<int> _mana = new NetworkVariable<int>(100);
        protected readonly NetworkVariable<int> _stamina = new NetworkVariable<int>(100);

        protected IInventory _inventory;
        protected IEffectService _effectService;
        protected ITypeRegistry _typeRegistry;

        // ReSharper restore InconsistentNaming
        #endregion

        #region Other Variables

        private readonly Dictionary<ulong, long> _damageTaken = new Dictionary<ulong, long>();
        private readonly List<ActiveEffect> _activeEffects = new List<ActiveEffect>();

        private Rigidbody _rb;
        private bool _isSprinting;

        public readonly HandStatus HandStatusLeft = new HandStatus();
        public readonly HandStatus HandStatusRight = new HandStatus();

        //Action-related
        private DelayedAction _replenishStamina;
        private DelayedAction _replenishMana;
        private DelayedAction _replenishEnergy;
        private DelayedAction _consumeStamina;
        private DelayedAction _consumeResource;

        #endregion

        #region Properties

        public abstract Transform Transform { get; }

        public abstract Transform LookTransform { get; }

        public abstract GameObject GameObject { get; }

        public abstract IStatSlider HealthStatSlider { get; protected set; }

        public Rigidbody RigidBody => _rb == null ? _rb = GetComponent<Rigidbody>() : _rb;

        public string FighterName { get; protected set; }

        public LivingEntityState AliveState { get; protected set; }

        #endregion

        #region Unity Events Handlers
        // ReSharper disable UnusedMemberHierarchy.Global

        protected virtual void Awake()
        {
            _gameManager = ModHelper.GetGameManager();
            _rpcService = _gameManager.GetService<IRpcService>();
            _localizer = _gameManager.GetService<ILocalizer>();

            _fighterName.OnValueChanged += OnNameChanged;
            _health.OnValueChanged += OnHealthChanged;
            _stamina.OnValueChanged += OnStaminaChanged;
            _energy.OnValueChanged += OnEnergyChanged;
            _mana.OnValueChanged += OnManaChanged;

            if (!NetworkManager.Singleton.IsConnectedClient || IsServer)
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

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            UpdateNameOnUi();
        }

        protected virtual void FixedUpdate()
        {
            ReplenishAndConsume();
            RemoveExpiredEffects();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            _health.OnValueChanged -= OnHealthChanged;
            _stamina.OnValueChanged -= OnStaminaChanged;
            _energy.OnValueChanged -= OnEnergyChanged;
            _mana.OnValueChanged -= OnManaChanged;
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

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void AddOrUpdateEffectClientRpc(string effectTypeId, int change, DateTime expiry, ClientRpcParams clientRpcParams)
        {
            //Debug.Log("AddOrUpdateEffectClientRpc called with typeId: " + effectTypeId);

            var effect = _typeRegistry.GetEffect(new Guid(effectTypeId));
            AddOrUpdateEffect(effect, change, expiry);
        }

        #endregion

        #region NetworkVariable Handlers

        private void OnNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
        {
            UpdateNameOnUi();
        }

        #endregion

        #region Health

        private void ApplyHealthChange(
            int change,
            IFighter sourceFighter,
            ItemBase itemUsed,
            Vector3? position)
        {
            if (change < 0)
            {
                TakeDamageInternal(sourceFighter, itemUsed, position, change * -1);
                return;
            }

            //todo: what else do we do when we heal? e.g. green damage number?
            _health.Value += change;
        }

        private void CheckHealth()
        {
            if (IsServer)
            {
                if (_health.Value <= 0)
                {
                    HandleDeath();
                }

                var healthMax = GetHealthMax();
                if (_health.Value > healthMax)
                {
                    _health.Value = healthMax;
                }
            }
        }

        private void OnHealthChanged(int previousValue, int newValue)
        {
            UpdateUiHealthAndDefenceValues();
            CheckHealth();
        }

        public int GetHealth()
        {
            CheckHealth();
            return _health.Value;
        }

        public int GetHealthMax()
        {
            //todo: trait-based health max
            return 100 + GetStatMaxAdjustment(AffectableStat.Health);
        }

        #endregion

        #region Stamina

        private void ApplyStaminaChange(int change)
        {
            _stamina.Value += change;
        }

        private void CheckStamina()
        {
            if (IsServer)
            {
                if (_stamina.Value <= 0)
                {
                    //todo: handle no stamina
                }

                var staminaMax = GetStaminaMax();
                if (_stamina.Value > staminaMax)
                {
                    _stamina.Value = staminaMax;
                }
            }
        }

        private void OnStaminaChanged(int previousValue, int newValue)
        {
            CheckStamina();
        }

        public int GetStamina()
        {
            CheckStamina();
            return _stamina.Value;
        }

        public int GetStaminaMax()
        {
            //todo: trait-based stamina max
            return 100 + GetStatMaxAdjustment(AffectableStat.Stamina);
        }

        public int GetStaminaCost()
        {
            //todo: trait-based stamina cost
            return 10;
        }

        #endregion

        #region Energy

        private void ApplyEnergyChange(int change)
        {
            _energy.Value += change;
        }

        private void CheckEnergy()
        {
            if (IsServer)
            {
                if (_energy.Value <= 0)
                {
                    //todo: handle no energy
                }

                var energyMax = GetEnergyMax();
                if (_energy.Value > energyMax)
                {
                    _energy.Value = energyMax;
                }
            }
        }

        private void OnEnergyChanged(int previousValue, int newValue)
        {
            CheckEnergy();
        }

        public int GetEnergy()
        {
            CheckEnergy();
            return _energy.Value;
        }

        public int GetEnergyMax()
        {
            //todo: trait-based energy max
            return 100 + GetStatMaxAdjustment(AffectableStat.Energy);
        }

        private int GetEnergyCost(Gadget gadget)
        {
            //todo: trait-based energy cost
            return 20;
        }

        #endregion

        #region Mana

        private void ApplyManaChange(int change)
        {
            _mana.Value += change;
        }

        private void CheckMana()
        {
            if (IsServer)
            {
                if (_mana.Value <= 0)
                {
                    //todo: handle no mana
                }

                var manaMax = GetManaMax();
                if (_mana.Value > manaMax)
                {
                    _mana.Value = manaMax;
                }
            }
        }
        private void OnManaChanged(int previousValue, int newValue)
        {
            CheckMana();
        }

        public int GetMana()
        {
            CheckMana();
            return _mana.Value;
        }

        public int GetManaMax()
        {
            //todo: trait-based mana max
            return 100 + GetStatMaxAdjustment(AffectableStat.Mana);
        }

        private int GetManaCost(Spell spell)
        {
            //todo: trait-based mana cost
            return 20;
        }

        #endregion

        public void SetName(string newName)
        {
            if (!IsServer)
            {
                Debug.LogWarning("Client tried to set fighter name");
                return;
            }

            _fighterName.Value = newName;
        }

        private void UpdateNameOnUi()
        {
            var fighterName = _fighterName.Value.ToString();

            var displayName = string.IsNullOrWhiteSpace(fighterName)
                ? "Fighter " + NetworkObjectId
                : fighterName;

            FighterName = displayName;
            gameObject.name = displayName;
            _nameTag.text = displayName;
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

        public float GetSprintSpeed()
        {
            //todo: trait-based sprint speed
            return 2.5f;
        }

        public int GetDefenseValue()
        {
            return _inventory.GetDefenseValue() + GetAttributeValue(AffectableAttribute.Strength);
        }

        protected void TakeDamageInternal(
            IFighter sourceFighter,
            ItemBase itemUsed,
            Vector3? position,
            int damageDealt)
        {
            _lastDamageSourceName = sourceFighter != null ? sourceFighter.FighterName : null;
            _lastDamageItemName = itemUsed?.Name ?? _localizer.Translate("ui.alert.attack.noitem");

            var sourceIsPlayer = sourceFighter != null && sourceFighter.GameObject.CompareTag(Tags.Player);

            var sourceNetworkObject = sourceFighter != null ? sourceFighter.GameObject.GetComponent<NetworkObject>() : null;
            var sourceClientId = sourceNetworkObject != null ? (ulong?)sourceNetworkObject.OwnerClientId : null;

            if (sourceClientId != null && !sourceFighter.Equals(this))
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
                sourceFighter.GameObject.GetComponent<IPlayerBehaviour>().ShowDamageClientRpc(
                    position.Value,
                    damageDealt.ToString(CultureInfo.InvariantCulture),
                    _rpcService.ForPlayer(sourceFighter.OwnerClientId));
            }

            _health.Value -= damageDealt;
        }

        public void TakeDamage(
            IFighter sourceFighter,
            ItemBase itemUsed,
            Vector3? position)
        {
            var damageDealt = AttributeCalculator.GetAttackValue(itemUsed?.Attributes, GetDefenseValue());
            TakeDamageInternal(sourceFighter, itemUsed, position, damageDealt);
        }

        public void HandleDeath()
        {
            if (AliveState == LivingEntityState.Dead)
            {
                return;
            }

            AliveState = LivingEntityState.Dead;

            GetComponent<Collider>().enabled = false;

            foreach (var (clientId, _) in _damageTaken)
            {
                if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
                {
                    continue;
                }

                var playerState = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<IPlayerFighter>();
                playerState.SpawnLootChest(transform.position);
            }

            _damageTaken.Clear();

            HandStatusLeft.StopConsumingResources();
            HandStatusRight.StopConsumingResources();

            var deathMessage = GetDeathMessage(false, name);
            var nearbyClients = _rpcService.ForNearbyPlayers(transform.position);
            _gameManager.GetSceneBehaviour().MakeAnnouncementClientRpc(deathMessage, nearbyClients);

            StopAllCoroutines();
            _activeEffects.Clear();

            HandleDeathAfter(_lastDamageSourceName, _lastDamageItemName);
        }

        // ReSharper disable UnusedParameter.Global
        protected virtual void HandleDeathAfter(string killerName, string itemName)
        {
            //Here for override only
        }
        // ReSharper restore UnusedParameter.Global

        public void CheckIfOffTheMap()
        {
            if (AliveState != LivingEntityState.Dead
                && transform.position.y < _gameManager.GetSceneBehaviour().Attributes.LowestYValue)
            {
                //todo: give credit to previous _lastDamageSourceName
                _lastDamageSourceName = _localizer.Translate("ui.alert.falldamage");
                _lastDamageItemName = null;
                HandleDeath();
            }
        }

        private string GetDeathMessage(bool isOwner, string victimName)
        {
            if (_lastDamageItemName.IsNullOrWhiteSpace())
            {
                return isOwner
                    ? string.Format(_localizer.Translate("ui.alert.attack.youwerekilledby"), _lastDamageSourceName)
                    : string.Format(_localizer.Translate("ui.alert.attack.victimkilledby"), victimName, _lastDamageSourceName);
            }

            return isOwner
                ? string.Format(_localizer.Translate("ui.alert.attack.youwerekilledbyusing"), _lastDamageSourceName, _lastDamageItemName)
                : string.Format(_localizer.Translate("ui.alert.attack.victimkilledbyusing"), victimName, _lastDamageSourceName, _lastDamageItemName);
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

        private int GetStatMaxAdjustment(AffectableStat affectableStat)
        {
            return _activeEffects
                .Where(x =>
                    x.Effect is IStatEffect statEffect
                    && statEffect.StatToAffect == affectableStat
                    && statEffect.Affect is Affect.TemporaryMaxIncrease or Affect.TemporaryMaxDecrease)
                .Sum(x => x.Change);
        }

        private int GetAttributeAdjustment(AffectableAttribute attribute)
        {
            return _activeEffects
                .Where(x =>
                    x.Effect is IAttributeEffect attributeEffect
                    && attributeEffect.AttributeToAffect == attribute)
                .Sum(x => x.Change * (x.Effect is IAttributeEffect attributeEffect && attributeEffect.TemporaryMaxIncrease ? 1 : -1));
        }

        private bool AllowMultiple(Affect affect)
        {
            return affect == Affect.TemporaryMaxIncrease
                || affect == Affect.TemporaryMaxDecrease;
        }

        private void AddOrUpdateEffect(IEffect effect, int change, DateTime expiry)
        {
            var multipleAllowed = (effect is IStatEffect statEffect && AllowMultiple(statEffect.Affect))
                || effect is IAttributeEffect;

            var effectMatch = _activeEffects.FirstOrDefault(x => x.Effect == effect);

            if (effectMatch != null)
            {
                if (multipleAllowed)
                {
                    _activeEffects.Add(new ActiveEffect
                    {
                        Id = Guid.NewGuid(),
                        Effect = effect,
                        Change = change,
                        Expiry = expiry
                    });
                }
                else
                {
                    effectMatch.Change = change;
                    effectMatch.Expiry = expiry;
                }
            }
            else
            {
                _activeEffects.Add(new ActiveEffect
                {
                    Id = Guid.NewGuid(),
                    Effect = effect,
                    Change = change,
                    Expiry = expiry
                });
            }

            if (OwnerClientId != NetworkManager.Singleton.LocalClientId)
            {
                AddOrUpdateEffectClientRpc(effect.TypeId.ToString(), change, expiry, _rpcService.ForPlayer(OwnerClientId));
            }
        }

        public void AddAttributeModifier(IAttributeEffect attributeEffect, Attributes attributes)
        {
            var (change, expiry) = attributes.GetAttributeChangeAndExpiry(attributeEffect);
            AddOrUpdateEffect(attributeEffect, change, expiry);
        }

        public void ApplyPeriodicActionToStat(IStatEffect statEffect, ItemBase itemUsed, IFighter sourceFighter)
        {
            var (change, expiry, delay) = itemUsed.Attributes.GetStatChangeExpiryAndDelay(statEffect);
            AddOrUpdateEffect(statEffect, change, expiry);
            StartCoroutine(PeriodicActionToStatCoroutine(statEffect.StatToAffect, change, sourceFighter, itemUsed, delay, expiry));
        }

        private IEnumerator PeriodicActionToStatCoroutine(AffectableStat stat, int change, IFighter sourceFighter, ItemBase itemUsed, float delay, DateTime expiry)
        {
            do
            {
                ApplyStatChange(stat, change, sourceFighter, itemUsed, transform.position);
                yield return new WaitForSeconds(delay);

            } while (DateTime.Now < expiry);
        }

        public void ApplyStatValueChange(IStatEffect statEffect, ItemBase itemUsed, IFighter sourceFighter, Vector3? position)
        {
            var (change, expiry) = itemUsed.Attributes.GetStatChangeAndExpiry(statEffect);

            AddOrUpdateEffect(statEffect, change, expiry);

            ApplyStatChange(statEffect.StatToAffect, change, sourceFighter, itemUsed, position);
        }

        public void ApplyTemporaryMaxActionToStat(IStatEffect statEffect, ItemBase itemUsed, IFighter sourceFighter, Vector3? position)
        {
            var (change, expiry) = itemUsed.Attributes.GetStatChangeAndExpiry(statEffect);

            AddOrUpdateEffect(statEffect, change, expiry);

            ApplyStatChange(statEffect.StatToAffect, change, sourceFighter, itemUsed, position);
        }

        public void ApplyElementalEffect(IEffect elementalEffect, ItemBase itemUsed, IFighter sourceFighter, Vector3? position)
        {
            TakeDamage(sourceFighter, itemUsed, position);

            //todo: ApplyElementalEffect
            Debug.LogWarning("Not yet implemented elemental effects");
        }

        public List<ActiveEffect> GetActiveEffects()
        {
            return _activeEffects;
        }

        private void ApplyStatChange(
            AffectableStat stat,
            int change,
            IFighter sourceFighter,
            ItemBase itemUsed,
            Vector3? position)
        {
            switch (stat)
            {
                case AffectableStat.Energy:
                    ApplyEnergyChange(change);
                    return;

                case AffectableStat.Health:
                    ApplyHealthChange(change, sourceFighter, itemUsed, position);
                    return;

                case AffectableStat.Mana:
                    ApplyManaChange(change);
                    return;

                case AffectableStat.Stamina:
                    ApplyStaminaChange(change);
                    return;

                default:
                    throw new ArgumentException("Unexpected AffectableStat: " + stat);
            }
        }

        //private int GetStatVariableMax(AffectableStat stat)
        //{
        //    switch (stat)
        //    {
        //        case AffectableStat.Energy: return GetEnergyMax();
        //        case AffectableStat.Health: return GetHealthMax();
        //        case AffectableStat.Mana: return GetManaMax();
        //        case AffectableStat.Stamina: return GetStaminaMax();
        //        default:
        //            throw new ArgumentException("Unexpected AffectableStat: " + stat);
        //    }
        //}

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

        private void RemoveExpiredEffects()
        {
            //Note: .ToList() needed to avoid the "Collection was modified" exception
            var expiredEffects = _activeEffects
                .Where(x => x.Expiry < DateTime.Now)
                .ToList();

            foreach (var activeEffect in expiredEffects)
            {
                _activeEffects.Remove(activeEffect);
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

        public void UpdateUiHealthAndDefenceValues()
        {
            if (!IsServer && IsOwner)
            {
                return;
            }

            var health = GetHealth();
            var maxHealth = GetHealthMax();
            var defence = _inventory.GetDefenseValue();
            var values = _gameManager.GetUserInterface().HudOverlay.GetHealthValues(health, maxHealth, defence);
            HealthStatSlider.SetValues(values);
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
