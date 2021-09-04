using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

public class Prefabs : MonoBehaviour
{
    public GameObject Player;
    public CombatObjects Combat;
    public EnvironmentObjects Environment;

    [System.Serializable]
    public class CombatObjects
    {
        public GameObject SpellInHand;
        public GameObject SpellProjectile;
        public GameObject SpellSelf;
    }

    [System.Serializable]
    public class EnvironmentObjects
    {
        public GameObject LootChest;
    }

}
