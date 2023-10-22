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
        public ShapeObjects Shapes;

        [System.Serializable]
        public class CombatObjects
        {
            public GameObject SpellInHand;
            public GameObject GadgetInHand;
            public GameObject Projectile;
            public GameObject PointToPoint;
            public GameObject BulletTrail;
            public GameObject BulletHole;
        }

        [System.Serializable]
        public class EnvironmentObjects
        {
            public GameObject LootChest;
        }

        [System.Serializable]
        public class ShapeObjects
        {
            public GameObject Wall;
            public GameObject Zone;
        }

    }
}
