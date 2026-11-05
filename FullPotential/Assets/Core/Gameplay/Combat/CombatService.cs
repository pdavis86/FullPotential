using System;
using System.Collections.Generic;
using System.Linq;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Attributes;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Logging;
using FullPotential.Api.Networking;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Player;

using Unity.Netcode;

using UnityEngine;

namespace FullPotential.Core.Gameplay.Combat
{
    public class CombatService : ICombatService
    {
        private readonly IAuditor _logger;
        private readonly ITypeRegistry _typeRegistry;
        private readonly IMovementEffectType _pushEffect;
        private readonly IEffectType _hurtEffect;
        private readonly CombatCalculator _combatCalculator = new CombatCalculator();
        private readonly IReadOnlyList<ICombatEffectHandler> _effectHandlers;

        public CombatService(
            IAuditorFactory auditorFactory,
            ITypeRegistry typeRegistry,
            IRpcService rpcService)
        {
            _logger = auditorFactory.Create(this);
            _typeRegistry = typeRegistry;
            _pushEffect = typeRegistry.GetRegisteredByTypeId<IEffectType>(EffectTypeIds.PushId) as IMovementEffectType;
            _hurtEffect = typeRegistry.GetRegisteredByTypeId<IEffectType>(EffectTypeIds.HurtId);
            _effectHandlers = new ICombatEffectHandler[]
            {
                new MovementEffectHandler(auditorFactory, rpcService),
                new ValuePoolEffectHandler(auditorFactory),
                new AttributeEffectHandler(auditorFactory, this)
            };
        }

        public void ApplyEffects(
            FighterBase sourceFighter,
            ICombatItem itemUsed,
            GameObject target,
            Vector3? position)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                _logger.Warn("ApplyEffects was not called on the server");
                return;
            }

            if (itemUsed == null)
            {
                var pushContext = new CombatEffectContext(sourceFighter, null, target, position);
                ApplyEffect(pushContext, _pushEffect);
            }

            var itemUsedEffects = itemUsed?.EffectTypes ?? new List<IEffectType>();

            if (!itemUsedEffects.Any())
            {
                itemUsedEffects.Add(_hurtEffect);
            }

            foreach (var effect in itemUsedEffects)
            {
                if (!IsEffectAllowed(target, effect))
                {
                    _logger.Debug("Effect {0} is not permitted against target {1}", effect.TypeId, target);
                    continue;
                }

                _logger.Debug("Applying effect {0} to {1}", effect.TypeId, target.name);

                var context = new CombatEffectContext(sourceFighter, itemUsed, target, position);
                ApplyEffect(context, effect);

                if (sourceFighter != null && effect is IHasSideEffect withSideEffect)
                {
                    var sideEffect = _typeRegistry.GetRegisteredByTypeId<IEffectType>(withSideEffect.SideEffectTypeId);
                    var sideEffectContext = new CombatEffectContext(null, itemUsed, sourceFighter.GameObject, position);
                    ApplyEffect(sideEffectContext, sideEffect);
                }
            }
        }

        private bool IsEffectAllowed(GameObject target, IEffectType effect)
        {
            if (effect is IMovementEffectType movementEffect
                && movementEffect.Direction == MovementDirection.MaintainDistance)
            {
                if (target.GetComponent<MaintainDistance>() != null)
                {
                    _logger.Debug("Continuing to hold target");
                    return false;
                }

                if (target.GetComponent<PlayerFighter>() != null)
                {
                    _logger.Debug("Cannot target players");
                    return false;
                }
            }

            return true;
        }

        private void ApplyEffect(CombatEffectContext context, IEffectType effect)
        {
            var handler = _effectHandlers.FirstOrDefault(x => x.CanHandle(effect));
            if (handler == null)
            {
                _logger.Error("Not implemented handling for effect {0}", effect);
                return;
            }

            handler.Apply(context, effect);
        }

        public float GetEffectBaseChange(FighterBase sourceFighter, ICombatItem itemUsed, IEffectType effect, bool isCriticalHit)
        {
            if (itemUsed == null && sourceFighter == null)
            {
                return 1;
            }

            var isPeriodic = effect is IValuePoolEffectType { EffectActionType: EffectActionType.PeriodicDecrease or EffectActionType.PeriodicIncrease };
            var baseValue = itemUsed == null
                ? LivingEntityBase.GetAdjustedStrength(sourceFighter.GetAdjustedAttributeValue(Strength.Id))
                : isPeriodic ? itemUsed.GetPeriodicEffectValueChange() : itemUsed.GetSingleEffectValueChange();
            var isWeapon = itemUsed != null && itemUsed.RegistryType is IWeaponType;
            var isMeleeWeapon = isWeapon && ((IWeaponType)itemUsed.RegistryType).AmmunitionTypeId == null;
            var ammoPerSecond = isWeapon && !isMeleeWeapon ? itemUsed.GetAmmoPerSecond() : 1;

            return _combatCalculator.GetEffectBaseChange(
                baseValue,
                isWeapon,
                isMeleeWeapon,
                ammoPerSecond,
                itemUsed?.IsTwoHanded ?? false,
                isCriticalHit);
        }

        public CombatResult GetCombatResult(FighterBase sourceFighter, ICombatItem itemUsed, IEffectType effect, LivingEntityBase targetLivingEntity)
        {
            var criticalChance = sourceFighter == null
                ? -1
                : _combatCalculator.GetCriticalHitChance(sourceFighter.GetAdjustedAttributeValue(Luck.Id));
            var effectivePercentage = GetEffectivePercentage(targetLivingEntity);

            return _combatCalculator.GetCombatResult(
                criticalChance,
                GetEffectBaseChange(sourceFighter, itemUsed, effect, false),
                effectivePercentage,
                effect);
        }

        private float GetEffectivePercentage(LivingEntityBase targetLivingEntity)
        {
            //todo: zzz v0.8 - replace with sum of Strength of items with resistance to Hurt

            var armorItems = _typeRegistry
                .GetRegisteredTypes<IArmorType>()
                .Select(x => targetLivingEntity.Inventory.GetItemInSlot<ItemBase>(new SlotId(x.TypeId)));

            var sum = armorItems
                .Where(x => x != null)
                .Sum(a => a.Attributes.GetValueOrZero(Strength.Id));

            var targetDefenceValue = (int)Math.Floor((float)sum / armorItems.Count());

            var targetDefence = targetLivingEntity != null ? targetDefenceValue : 0;
            return _combatCalculator.GetEffectivePercentage(targetDefence);
        }

        public void SpawnTargetingGameObject(FighterBase sourceFighter, IConsumer consumer, Vector3 startPosition, Vector3 direction)
        {
            if (consumer.TargetingType.NetworkPrefabAddress.IsNullOrWhiteSpace())
            {
                return;
            }

            _typeRegistry.LoadAddessable<GameObject>(
                consumer.TargetingType.NetworkPrefabAddress,
                prefab =>
                {
                    var targetingGameObject = UnityEngine.Object.Instantiate(prefab, startPosition, Quaternion.identity);

                    var targetingBehaviour = targetingGameObject.GetComponent<ITargetingBehaviour>();
                    targetingBehaviour.SourceFighter = sourceFighter;
                    targetingBehaviour.Consumer = consumer;
                    targetingBehaviour.Direction = direction;

                    targetingGameObject.NetworkSpawn(_logger);

                    if (consumer.TargetingType.IsContinuous)
                    {
                        targetingGameObject.transform.parent = sourceFighter.Transform;
                    }

                    consumer.Stoppables.Add(new DestroyStoppable(targetingGameObject));
                });
        }

        public void SpawnShapeGameObject(FighterBase sourceFighter, IConsumer consumer, GameObject target, Vector3 fallbackPosition, Vector3 lookDirection)
        {
            if (consumer.ShapeType == null)
            {
                _logger.Debug("Cancelling SpawnShapeGameObject because there was no Shape");
                return;
            }

            Vector3 spawnPosition;
            if (target == null || !target.CompareTagAny(Tags.Player, Tags.Enemy))
            {
                spawnPosition = fallbackPosition;
            }
            else
            {
                var pointUnderTarget = new Vector3(target.transform.position.x, -100, target.transform.position.z);
                var feetOfTarget = target.GetComponent<Collider>().ClosestPointOnBounds(pointUnderTarget);

                spawnPosition = Physics.Raycast(feetOfTarget, Vector3.down, out var hit)
                    ? hit.point
                    : fallbackPosition;
            }

            var rotation = Quaternion.LookRotation(lookDirection);
            rotation.x = 0;
            rotation.z = 0;

            _typeRegistry.LoadAddessable<GameObject>(
                consumer.ShapeType.NetworkPrefabAddress,
                prefab =>
                {
                    var shapeGameObject = UnityEngine.Object.Instantiate(prefab, spawnPosition, rotation);
                    var sceneService = GameManager.Instance.GetSceneBehaviour().GetSceneService();

                    shapeGameObject.transform.position = sceneService.GetHeightAdjustedPosition(spawnPosition, shapeGameObject);

                    shapeGameObject.layer = LayerMask.NameToLayer(Layers.NonSolid);

                    var shapeBehaviour = shapeGameObject.GetComponent<IShapeBehaviour>();
                    shapeBehaviour.SourceFighter = sourceFighter;
                    shapeBehaviour.Consumer = consumer;
                    shapeBehaviour.Direction = lookDirection;

                    shapeGameObject.NetworkSpawn(_logger);
                });
        }
    }
}
