using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Combat.Events;
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

        public Func<IEventHandlerArgs, UniTask> BeforeHandlerAsync => null;

        public Func<IEventHandlerArgs, UniTask> AfterHandlerAsync => HandleAfterBulletFiredAsync;

        public ShotFiredEventHandler(ITypeRegistry typeRegistry)
        {
            _typeRegistry = typeRegistry;
        }

        private UniTask HandleAfterBulletFiredAsync(IEventHandlerArgs eventArgs)
        {
            var shotFiredArgs = (ShotFiredEventArgs)eventArgs;

            var item = shotFiredArgs.Fighter.Inventory.GetItemInSlot(shotFiredArgs.SlotId);

            if (item is not Weapon weapon || !weapon.IsRanged)
            {
                return UniTask.CompletedTask;
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
        
                return UniTask.CompletedTask;
        }
    }
}
