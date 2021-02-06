using UnityEngine.Networking;

namespace Assets.Scripts.Networking
{
#pragma warning disable CS0618 // Type or member is obsolete
    public class NetworkServer2
    {

        public static void Spawn(UnityEngine.GameObject gameobject)
        {
            NetworkServer.Spawn(gameobject);
        }

    }
#pragma warning restore CS0618 // Type or member is obsolete
}