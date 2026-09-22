using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Obsolete.Items.Types;
using FullPotential.Api.Registry;
using FullPotential.Api.Input;

using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.WeaponExtras
{
    public class ShotFiredEventHandler : IEventHandler<ShotFiredEventArgs>
    {
        private const string BulletTrailPrefabAddress = "Standard/Prefabs/Combat/BulletTrail.prefab";

        private readonly ITypeRegistry _typeRegistry;

        public NetworkLocation Location => NetworkLocation.Client;

        public Timing Timing => Timing.After;

        public Func<ShotFiredEventArgs, UniTask<HandlerResult>> HandlerAsync => HandleAfterBulletFiredAsync;

        public ShotFiredEventHandler(ITypeRegistry typeRegistry)
        {
            _typeRegistry = typeRegistry;
        }

        private UniTask<HandlerResult> HandleAfterBulletFiredAsync(ShotFiredEventArgs eventArgs)
        {
            var item = eventArgs.Fighter.Inventory.GetItemInSlot(eventArgs.SlotId);

            if (item is not Weapon weapon || !weapon.IsRanged)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            _typeRegistry.LoadAddessable<GameObject>(BulletTrailPrefabAddress, prefab =>
            {
                var projectile = UnityEngine.Object.Instantiate(
                    prefab,
                    eventArgs.StartPosition.Value,
                    Quaternion.identity);

                var projectileScript = projectile.GetComponent<ProjectileWithTrail>();
                projectileScript.TargetPosition = eventArgs.EndPosition.Value;
                projectileScript.Speed = 500;
                projectileScript.ObjectHit = eventArgs.ObjectHit;
            });

            return UniTask.FromResult(new HandlerResult());
        }
    }
}
