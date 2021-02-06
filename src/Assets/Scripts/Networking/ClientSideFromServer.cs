using System;

// https://docs.unity3d.com/2018.4/Documentation/ScriptReference/Networking.ClientRpcAttribute.html
// This function will now be run on clients when it is called on the server. 

namespace Assets.Scripts.Networking
{
#pragma warning disable CS0618 // Type or member is obsolete
    public class ClientSideFromServer : UnityEngine.Networking.ClientRpcAttribute
#pragma warning restore CS0618 // Type or member is obsolete
    {
    }
}
