using UnityEngine;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Registry.Consumers
{
    public interface IConsumerBehaviour
    {
        void Stop();
        
        void ApplyEffects(GameObject target, Vector3? position);
    }
}
