using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;
using Unity.Netcode;
using UnityEngine;

namespace FullPotential.Api.Gameplay.Items
{
    public abstract class ConsumerVisualsBehaviour : NetworkBehaviour, IStoppable
    {
        public IFighter SourceFighter { get; set; }

        public Consumer Consumer { get; set; }

        public Vector3 StartPosition { get; set; }

        public Vector3 Direction { get; set; }

        public abstract void Stop();
    }
}
