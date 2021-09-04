using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

//todo: move UI out of this as it does not need to load on the server

public class Prefabs : MonoBehaviour
{
    public GameObject Player;
    public CombatObjects Combat;
    public EnvironmentObjects Environment;
    public UiObjects Ui;

    [System.Serializable]
    public class CombatObjects
    {
        public GameObject Spell;
        public GameObject SpellInHand;
        public GameObject HitText;
    }

    [System.Serializable]
    public class EnvironmentObjects
    {
        public GameObject LootChest;
    }

    [System.Serializable]
    public class UiObjects
    {
        public GameObject InventoryRowPrefab;
    }

}
