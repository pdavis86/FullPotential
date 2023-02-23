using System;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry.Consumers;
using FullPotential.Api.Registry.Shapes;
using UnityEngine;

namespace FullPotential.Standard.SpellsAndGadgets.Shapes
{
    public class Wall : IShape
    {
        public Guid TypeId => new Guid("4aad2866-5903-4b79-bda2-e3dcab920d9e");

        public string TypeName => nameof(Wall);

        public string PrefabAddress => "Standard/Prefabs/SpellOrGadget/Wall.prefab";

        public IStoppable SetBehaviourVariables(Consumer consumer, IFighter sourceFighter, Vector3 startPosition, Quaternion rotation)
        {
            throw new NotImplementedException();
        }
    }
}
