using UnityEngine;

namespace Assets.Scripts.Spells
{
    public abstract class SpellBehaviourBase : MonoBehaviour
    {
        const float _timeToLive = 3f;
        private float _timeAlive;

        internal void UpdateTimeAlive(GameObject spell)
        {
            _timeAlive += Time.deltaTime;
            if (_timeAlive >= _timeToLive)
            {
                Destroy(spell);
            }
        }
    }
}
