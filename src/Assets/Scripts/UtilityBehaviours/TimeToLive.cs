using UnityEngine;

public class TimeToLive : MonoBehaviour
{
    public GameObject GameObjectToDestroy;
    public float AllowedTime = 3f;

    private float _timeAlive;

    void Update()
    {
        _timeAlive += Time.deltaTime;
        if (_timeAlive >= AllowedTime)
        {
            Destroy(GameObjectToDestroy);
        }
    }

}
