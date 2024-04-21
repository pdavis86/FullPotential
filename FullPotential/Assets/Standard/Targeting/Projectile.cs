using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Standard.Targeting
{
    public class Projectile : ITargetingType
    {
        public const string TypeIdString = "6e41729e-bb21-44f8-8fb9-b9ad48c0e680";
        public const string AddressablePath = "Standard/Prefabs/Targeting/Projectile.prefab";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public bool CanHaveShape => true;

        public bool IsContinuous => false;

        public string NetworkPrefabAddress => AddressablePath;

        public IEnumerable<ViableTarget> GetTargets(FighterBase sourceFighter, Consumer consumer)
        {
            return null;
        }
    }
}
