using UnityEngine;


namespace FullPotential.Core.Utilities.UtilityBehaviours
{
    public class DontDestroy : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}