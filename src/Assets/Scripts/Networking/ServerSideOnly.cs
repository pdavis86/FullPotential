using System;

//todo: anything using this needs a "Cmd" prefix

// https://docs.unity3d.com/2018.4/Documentation/ScriptReference/Networking.ServerAttribute.html
// A Custom Attribute that can be added to member functions of NetworkBehaviour scripts, to make them only run on servers.

namespace Assets.Scripts.Attributes
{
#pragma warning disable CS0618 // Type or member is obsolete
    public class ServerSideOnly : UnityEngine.Networking.CommandAttribute
#pragma warning restore CS0618 // Type or member is obsolete
    {
    }
}
