using UnityEngine;

// ReSharper disable once UnusedType.Global
// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Core.Behaviours.Combat
{
    public class ProjectileWithTrail : MonoBehaviour
    {
        public Vector3 TargetPosition;
        public float Speed;

        private float _startTime;
        private Vector3 _startPosition;
        private float _journeyLength;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            _startTime = Time.time;
            _startPosition = transform.position;
            _journeyLength = Vector3.Distance(_startPosition, TargetPosition);
        }

        // ReSharper disable once UnusedMember.Local
        private void Update()
        {
            var timeTaken = Time.time - _startTime;

            var distCovered = timeTaken * Speed;
            var fractionOfJourney = distCovered / _journeyLength;
            transform.position = Vector3.Lerp(_startPosition, TargetPosition, fractionOfJourney);

            if (Vector3.Distance(TargetPosition, transform.position) < 0.01)
            {
                //Debug.Log("Destroying at destination");
                Destroy(gameObject);
            }
        }

    }
}
