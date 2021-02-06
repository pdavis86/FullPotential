using System;

//todo: anything using this needs a "Rpc" prefix

// https://docs.unity3d.com/2018.4/Documentation/ScriptReference/Networking.ClientAttribute.html
//A Custom Attribute that can be added to member functions of NetworkBehaviour scripts, to make them only run on clients.

namespace Assets.Scripts.Networking
{
#pragma warning disable CS0618 // Type or member is obsolete
    public class ClientSideOnly : UnityEngine.Networking.ClientAttribute
#pragma warning restore CS0618 // Type or member is obsolete
    {
    }
}
