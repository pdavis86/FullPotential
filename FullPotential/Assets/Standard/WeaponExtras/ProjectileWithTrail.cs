using System;
using System.Linq;
using FullPotential.Api.Ioc;
using FullPotential.Api.Registry;
using FullPotential.Api.Unity.Extensions;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.WeaponExtras
{
    public class ProjectileWithTrail : MonoBehaviour
    {
        private const string BulletHolePrefabAddress = "Standard/Prefabs/Combat/BulletHole.prefab";

        public Vector3 TargetPosition;
        public float Speed;
        public GameObject ObjectHit;

        private ITypeRegistry _typeRegistry;

        private float _startTime;
        private Vector3 _startPosition;
        private float _journeyLength;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            _typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();

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
                SpawnBulletHole(ObjectHit, TargetPosition);
                Destroy(gameObject);
            }
        }

        private void SpawnBulletHole(GameObject target, Vector3? position)
        {
            if (!position.HasValue || target == null)
            {
                return;
            }

            var targetCollider = target.GetComponent<BoxCollider>();

            if (targetCollider == null)
            {
                return;
            }

            var vertices = targetCollider.GetBoxColliderVertices();

            var matchesX = vertices.Where(v => Mathf.Approximately(v.x, position.Value.x)).ToList();
            var matchesZ = vertices.Where(v => Mathf.Approximately(v.z, position.Value.z)).ToList();

            var points = matchesX.Count > 0
                ? matchesX
                : matchesZ;

            //Debug.DrawRay(points[0], Vector3.up, Color.cyan, 5);
            //Debug.DrawRay(points[1], Vector3.up, Color.cyan, 5);

            if (points.Count == 0)
            {
                return;
            }

            var vec1 = points[0] - position.Value;
            var vec2 = points[1] - position.Value;

            var norm = Vector3.Cross(vec1, vec2).normalized;

            var otherPoints = matchesX.Count > 0
                ? vertices.Where(v => Math.Abs(v.x - position.Value.x) > 0.1).ToList()
                : vertices.Where(v => Math.Abs(v.z - position.Value.z) > 0.1).ToList();

            var directionCheck = points[0] - otherPoints[0];

            if ((matchesX.Count > 0 && directionCheck.x > 0)
                || (matchesZ.Count > 0 && directionCheck.z > 0))
            {
                norm *= -1;
            }

            //Debug.DrawRay(position.Value, norm, Color.cyan, 5);

            var rotation = Quaternion.FromToRotation(-Vector3.forward, norm);

            _typeRegistry.LoadAddessable<GameObject>(BulletHolePrefabAddress, prefab =>
            {
                var bulletHole = Instantiate(prefab, position.Value, rotation);
                bulletHole.NetworkSpawn();
                Destroy(bulletHole, 5);
            });
        }
    }
}
