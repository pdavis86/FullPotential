using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnassignedField.Global

namespace FullPotential.Core.GameManagement
{
    public class Prefabs : MonoBehaviour
    {
        public GameObject Player;
        public CombatObjects Combat;
        public EnvironmentObjects Environment;

        [System.Serializable]
        public class CombatObjects
        {
            public GameObject BulletTrail;
            public GameObject BulletHole;
        }

        [System.Serializable]
        public class EnvironmentObjects
        {
            public GameObject LootChest;
        }

    }
}
