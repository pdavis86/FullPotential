using System;
using FullPotential.Api.Gameplay.Combat.EventArgs;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry;
using FullPotential.Api.Ui;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.WeaponExtras
{
    public class ShotFiredEventHandler : IEventHandler
    {
        private const string BulletTrailPrefabAddress = "Standard/Prefabs/Combat/BulletTrail.prefab";

        private readonly ITypeRegistry _typeRegistry;

        public NetworkLocation Location => NetworkLocation.Client;

        public Action<IEventHandlerArgs> BeforeHandler => null;

        public Action<IEventHandlerArgs> AfterHandler => HandleAfterBulletFired;

        public ShotFiredEventHandler(ITypeRegistry typeRegistry)
        {
            _typeRegistry = typeRegistry;
        }

        private void HandleAfterBulletFired(IEventHandlerArgs eventArgs)
        {
            var shotFiredArgs = (ShotFiredEventArgs)eventArgs;

            var item = shotFiredArgs.Fighter.Inventory.GetItemInSlot(shotFiredArgs.IsLeftHand ? HandSlotIds.LeftHand : HandSlotIds.RightHand);

            if (item is not Weapon weapon || !weapon.IsRanged)
            {
                return;
            }

            _typeRegistry.LoadAddessable<GameObject>(BulletTrailPrefabAddress, prefab =>
            {
                var projectile = UnityEngine.Object.Instantiate(
                    prefab,
                    shotFiredArgs.StartPosition,
                    Quaternion.identity);

                var projectileScript = projectile.GetComponent<ProjectileWithTrail>();
                projectileScript.TargetPosition = shotFiredArgs.EndPosition;
                projectileScript.Speed = 500;
                projectileScript.ObjectHit = shotFiredArgs.ObjectHit;
            });
        }
    }
}
