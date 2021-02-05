using UnityEngine;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

public class ObjectAccess : MonoBehaviour
{
    public GameObject PrefabSpell;
    public GameObject PrefabHitText;
    public GameObject UiDamageNumbers;
    public GameObject UiHud;
    public GameObject UiCrafting;

    private static ObjectAccess _instance;

    public static ObjectAccess Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
}
