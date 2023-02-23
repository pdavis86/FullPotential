using System;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry.Consumers;
using FullPotential.Api.Registry.Shapes;
using UnityEngine;

namespace FullPotential.Standard.SpellsAndGadgets.Shapes
{
    public class Zone : IShape
    {
        public Guid TypeId => new Guid("142aeb3b-84b1-43c6-ae91-388b0901fa52");

        public string TypeName => nameof(Zone);

        public string PrefabAddress => "Standard/Prefabs/SpellOrGadget/Zone.prefab";

        public IStoppable SetBehaviourVariables(Consumer consumer, IFighter sourceFighter, Vector3 startPosition, Quaternion rotation)
        {
            throw new NotImplementedException();
        }
    }
}
