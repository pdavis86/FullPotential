using System;

using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnassignedField.Global

namespace FullPotential.Core.GameManagement
{
    public class Prefabs : MonoBehaviour
    {
        public GameObject Player;
        public EnvironmentObjects Environment;

        [Serializable]
        public class EnvironmentObjects
        {
            public GameObject LootChest;
        }
    }
}
