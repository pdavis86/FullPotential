using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Input;
using FullPotential.Api.Logging;
using FullPotential.Api.Obsolete.Items.Types;
using FullPotential.Api.Ui;

using Unity.Netcode;

using UnityEngine;

namespace FullPotential.Standard.Weapons.Helpers
{
    public class AttackHelper : IAttackHelper
    {
        private const int MeleeRangeLimit = 8;
        private const int ConsumerRangeLimit = 50;
        private const int MaximumRange = 100;

        private IAuditor _logger;
        private ICombatService _combatService;
        private IEventBus _eventBus;

        public AttackHelper(
            IAuditorFactory auditorFactory,
            ICombatService combatService,
            IEventBus eventBus)
        {
            _logger = auditorFactory.Create(this);
            _combatService = combatService;
            _eventBus = eventBus;
        }

        public void AttackWithItemInHand(FighterBase fighter, string slotId, bool isAutoFire = false)
        {
            if (fighter.AliveState != LivingEntityState.Alive)
            {
                return;
            }

            var itemInHand = fighter.Inventory.GetItemInSlot(slotId);

            switch (itemInHand)
            {
                case null:
                    Punch(fighter);
                    break;

                case Consumer consumer:
                    UseConsumer(fighter, slotId, consumer);
                    break;

                case Weapon weaponInHand:
                    UseWeapon(fighter, slotId, weaponInHand, isAutoFire);
                    break;

                default:
                    _logger.Warn("Not implemented attack for " + itemInHand.Name + " yet");
                    return;
            }
        }

        private void Punch(FighterBase fighter)
        {
            // todo: zzz v0.8 - Punch animation

            if (NetworkManager.Singleton.IsServer)
            {
                if (Physics.Raycast(fighter.LookTransform.position, fighter.LookTransform.forward, out var hit, MeleeRangeLimit))
                {
                    _combatService.ApplyEffects(fighter, null, hit.transform.gameObject, hit.point);
                }
            }
        }

        private void UseWeapon(FighterBase fighter, string slotId, Weapon weaponInHand, bool isAutoFire)
        {
            var slotStatus = fighter.GetSlotStatus(slotId);

            if (!isAutoFire && slotStatus.IsIntraActionLooping)
            {
                slotStatus.StopIntraActionLoop();
            }

            if (weaponInHand.IsRanged)
            {
                UseRangedWeapon(fighter, slotId, slotStatus, weaponInHand);
                return;
            }

            UseMeleeWeapon(fighter, slotId, weaponInHand);
        }

        private void UseRangedWeapon(FighterBase fighter, string slotId, SlotStatus slotStatus, Weapon weaponInHand)
        {
            if (weaponInHand.Ammo == 0 || slotStatus.IsBusy)
            {
                _logger.Debug($"Cancelling UseRangedWeapon(). Ammo: {weaponInHand.Ammo}, IsBusy: {slotStatus.IsBusy}");
                return;
            }

            var handPosition = slotId == HandSlotIds.LeftHand
                ? fighter.Positions.LeftHand.position
                : fighter.Positions.RightHand.position;

            var shotDirection = weaponInHand.GetShotDirection(fighter.LookTransform.forward);

            var endPos = Physics.Raycast(fighter.LookTransform.position, shotDirection, out var rangedHit, MaximumRange)
                ? rangedHit.point
                : handPosition + shotDirection * MaximumRange;

            var ammoUsed = Math.Min(
                1 + weaponInHand.Attributes.ExtraAmmoPerShot,
                weaponInHand.Ammo);

            var eventArgs = new ShotFiredEventArgs(fighter, slotId, handPosition, endPos, ammoUsed, rangedHit.transform?.gameObject);
            _eventBus.PublishAsync(eventArgs).Forget();

            if (rangedHit.transform == null)
            {
                _logger.Debug($"Cancelling UseRangedWeapon(). Nothing was hit");
                return;
            }

            if (NetworkManager.Singleton.IsServer)
            {
                for (var i = 0; i < ammoUsed; i++)
                {
                    _combatService.ApplyEffects(fighter, weaponInHand, rangedHit.transform.gameObject, rangedHit.point);
                }
            }
        }

        private void UseMeleeWeapon(FighterBase fighter, string slotId, Weapon weaponInHand)
        {
            var slotStatus = fighter.GetSlotStatus(slotId);

            if (weaponInHand.ChargePercentage < 100)
            {
                slotStatus.StopChargeUpLoop();
                slotStatus.StartCooldownLoopAsync(weaponInHand).Forget();
                return;
            }

            if (NetworkManager.Singleton.IsServer)
            {
                if (Physics.Raycast(fighter.LookTransform.position, fighter.LookTransform.forward, out var meleeHit, MeleeRangeLimit))
                {
                    _combatService.ApplyEffects(fighter, weaponInHand, meleeHit.transform.gameObject, meleeHit.point);
                }
            }

            slotStatus.StartCooldownLoopAsync(weaponInHand).Forget();
        }

        private void UseConsumer(FighterBase fighter, string slotId, Consumer consumer)
        {
            if (consumer == null)
            {
                _logger.Error("UseConsumer was called with no consumer");
                return;
            }

            var slotStatus = fighter.GetSlotStatus(slotId);

            if (consumer.ChargePercentage < 100 || slotStatus.StopActiveConsumerBehaviour())
            {
                slotStatus.StopChargeUpLoop();
                slotStatus.StartCooldownLoopAsync(consumer).Forget();
                return;
            }

            slotStatus.StartCooldownLoopAsync(consumer).Forget();

            if (!fighter.ConsumeResource(consumer, isTest: true))
            {
                _logger.Error("Cancelling UseConsumer as there was not enough of the resource");
                return;
            }

            if (consumer.Targeting.IsContinuous)
            {
                slotStatus.IsConsumingResource = true;
            }

            fighter.ConsumeResource(consumer);

            if (NetworkManager.Singleton.IsServer)
            {
                SpawnConsumerGameObjects(fighter, slotId, consumer);
            }
        }

        private void SpawnConsumerGameObjects(FighterBase fighter, string slotId, Consumer consumer)
        {
            var targets = consumer.Targeting.GetTargets(fighter, consumer);

            if (targets == null || consumer.Shape == null)
            {
                var handPosition = slotId == HandSlotIds.LeftHand
                    ? fighter.Positions.LeftHand.position
                    : fighter.Positions.RightHand.position;

                var attackDirection = Physics.Raycast(fighter.LookTransform.position, fighter.LookTransform.forward, out var hit, ConsumerRangeLimit)
                    ? (hit.point - handPosition).normalized
                    : fighter.LookTransform.forward;

                _combatService.SpawnTargetingGameObject(fighter, consumer, handPosition, attackDirection);

                if (targets != null)
                {
                    foreach (var target in targets)
                    {
                        _combatService.ApplyEffects(fighter, consumer, target.GameObject, target.Position);
                    }
                }
            }
            else if (consumer.Shape != null)
            {
                _combatService.SpawnShapeGameObject(fighter, consumer, null, fighter.Transform.position, fighter.Transform.forward);
            }
        }
    }
}
