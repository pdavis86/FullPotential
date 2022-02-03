using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnassignedField.Global

namespace FullPotential.Core.Behaviours.GameManagement
{
    public class Prefabs : MonoBehaviour
    {
        public GameObject Player;
        public CombatObjects Combat;
        public EnvironmentObjects Environment;

        [System.Serializable]
        public class CombatObjects
        {
            public GameObject ProjectileWithTrail;
            public GameObject SpellInHand;
            public GameObject SpellProjectile;
            public GameObject SpellSelf;
            public GameObject SpellBeam;
            public GameObject SpellWall;
            public GameObject SpellZone;
        }

        [System.Serializable]
        public class EnvironmentObjects
        {
            public GameObject LootChest;
        }

    }
}