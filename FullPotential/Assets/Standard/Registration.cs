using System.Collections.Generic;

using FullPotential.Api.Modding;

using UnityEngine;

// ReSharper disable UnusedType.Global

namespace FullPotential.Standard
{
    public class Registration : MonoBehaviour, IMod
    {
        public IEnumerable<string> GetNetworkPrefabAddresses()
        {
            return new[]
            {
                Shapes.Wall.AddressablePath,
                Shapes.Zone.AddressablePath,
                Targeting.PointToPoint.AddressablePath,
                Targeting.Projectile.AddressablePath,
            };
        }
    }
}
