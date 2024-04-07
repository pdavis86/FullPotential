using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Standard.Targeting
{
    public class PointToPoint : ITargeting
    {
        public const string TypeIdString = "3c528d09-bd4f-4211-af17-d8721975fef1";
        public const string AddressablePath = "Standard/Prefabs/Targeting/PointToPoint.prefab";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public bool CanHaveShape => false;

        public bool IsContinuous => true;

        public string NetworkPrefabAddress => AddressablePath;

        public IEnumerable<ViableTarget> GetTargets(FighterBase sourceFighter, Consumer consumer)
        {
            return null;
        }
    }
}
