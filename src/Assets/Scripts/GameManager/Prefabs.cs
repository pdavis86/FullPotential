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
    public CombatClass Combat;
    public WeaponsClass Weapons;

    [System.Serializable]
    public class CombatClass
    {
        public GameObject Spell;
        public GameObject HitText;
    }

    [System.Serializable]
    public class WeaponsClass
    {
        public GameObject Dagger;
        public GameObject Axe1;
        public GameObject Axe2;
        public GameObject Sword1;
        public GameObject Sword2;
        public GameObject Hammer1;
        public GameObject Hammer2;
        public GameObject Staff;
        public GameObject Bow;
        public GameObject Crossbow;
        public GameObject Gun1;
        public GameObject Gun2;
        public GameObject Shield;
    }
}
