using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Obsolete.Items.Types;
using FullPotential.Api.Registry;
using FullPotential.Standard.Weapons.Events;

using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.WeaponExtras
{
    public class ShotFiredEventHandler : IEventHandler<ShotFiredAfterEvent>
    {
        private const string BulletTrailPrefabAddress = "Standard/Prefabs/Combat/BulletTrail.prefab";

        private readonly ITypeRegistry _typeRegistry;

        public NetworkLocation Location => NetworkLocation.Client;

        public Timing Timing => Timing.Late;

        public ShotFiredEventHandler(ITypeRegistry typeRegistry)
        {
            _typeRegistry = typeRegistry;
        }

        public UniTask<HandlerResult> HandleEventAsync(ShotFiredAfterEvent eventArgs)
        {
            var item = eventArgs.Fighter.Inventory.GetItemInSlot(eventArgs.SlotId);

            if (item is not Weapon weapon || !weapon.IsRanged)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            // todo: zzz v0.7 - Bullet comes from real hand position rather than what I see
            _typeRegistry.LoadAddessable<GameObject>(BulletTrailPrefabAddress, prefab =>
            {
                var projectile = Object.Instantiate(
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
