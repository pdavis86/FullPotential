using UnityEngine;

namespace FullPotential.Api.Scenes
{
    public interface ISceneService
    {
        Vector3 GetPositionOnSolidObject(Vector3 startingPoint);

        Vector3 GetHeightAdjustedPosition(Vector3 startingPoint, GameObject gameObject);

        Vector3 GetHeightAdjustedPosition(Vector3 startingPoint, Collider collider);

        Vector3 GetHeightAdjustedPosition(Vector3 startingPoint, float gameObjectHeight);
    }
}
