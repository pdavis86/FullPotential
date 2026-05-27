using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry;
using FullPotential.Api.Ui;

using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.WeaponExtras
{
    [RegisterEvent(FighterBase.ShotFiredEventId)]
    public class ShotFiredEventHandler : IEventHandler<ShotFiredEventArgs>
    {
        private const string BulletTrailPrefabAddress = "Standard/Prefabs/Combat/BulletTrail.prefab";

        private readonly ITypeRegistry _typeRegistry;

        public NetworkLocation Location => NetworkLocation.Client;

        public Func<ShotFiredEventArgs, UniTask> BeforeHandlerAsync => null;

        public Func<ShotFiredEventArgs, UniTask> AfterHandlerAsync => HandleAfterBulletFiredAsync;

        public ShotFiredEventHandler(ITypeRegistry typeRegistry)
        {
            _typeRegistry = typeRegistry;
        }

        private UniTask HandleAfterBulletFiredAsync(ShotFiredEventArgs eventArgs)
        {
            var item = eventArgs.Fighter.Inventory.GetItemInSlot(eventArgs.SlotId);

            if (item is not Weapon weapon || !weapon.IsRanged)
            {
                return UniTask.CompletedTask;
            }

            _typeRegistry.LoadAddessable<GameObject>(BulletTrailPrefabAddress, prefab =>
            {
                var projectile = UnityEngine.Object.Instantiate(
                    prefab,
                    eventArgs.StartPosition,
                    Quaternion.identity);

                var projectileScript = projectile.GetComponent<ProjectileWithTrail>();
                projectileScript.TargetPosition = eventArgs.EndPosition;
                projectileScript.Speed = 500;
                projectileScript.ObjectHit = eventArgs.ObjectHit;
            });
        
                return UniTask.CompletedTask;
        }
    }
}
