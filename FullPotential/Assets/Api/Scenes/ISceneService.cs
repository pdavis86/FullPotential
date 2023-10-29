using UnityEngine;

namespace FullPotential.Api.Scenes
{
    public interface ISceneService
    {
        Vector3 GetPositionAboveGround(Vector3 startingPoint);

        Vector3 GetPositionAboveGround(Vector3 startingPoint, GameObject gameObject);

        Vector3 GetPositionAboveGround(Vector3 startingPoint, Collider collider);

        Vector3 GetPositionAboveGround(Vector3 startingPoint, float gameObjectHeight);
    }
}
