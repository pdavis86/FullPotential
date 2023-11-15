using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;
using UnityEngine;

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Gameplay.Shapes
{
    //todo: If modders can't define an IShapeBehaviour why is it in API instead of Core?
    public interface IShapeBehaviour
    {
        public IFighter SourceFighter { get; set; }

        public Consumer Consumer { get; set; }

        public Vector3 Direction { get; set; }
    }
}
