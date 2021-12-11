using FullPotential.Core.Data;
using FullPotential.Core.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullPotential.Core.Networking
{
    public static class MessageHelper
    {
        public static IEnumerable<string> GetFragmentedMessages(object payload, bool compress = true)
        {
            const int chunkSize = 1000;

            var json = JsonUtility.ToJson(payload);
            var groupId = Guid.NewGuid().ToMinimisedString();
            var groupStartDateTime = DateTime.UtcNow.ToString("u");
            var fragmentCount = (int)Math.Ceiling((float)json.Length / 1000);

            //todo: add compression

            for (var i = 0; i < json.Length; i += chunkSize)
            {
                yield return JsonUtility.ToJson(new FragmentedMessage
                {
                    GroupId = groupId,
                    GroupStartDateTime = groupStartDateTime,
                    FragmentCount = fragmentCount,
                    SequenceId = i,
                    Payload = json.Substring(i, Math.Min(chunkSize, json.Length - i))
                });
            }
        }

        //public const int NonFragmentedMessageMaxSize = 1300;

        //public static void SendMessage(object payload, string messageName, ulong clientId)
        //{
        //    SendMessage(payload, messageName, new List<ulong> { clientId });
        //}

        //public static void SendMessage(object payload, string messageName, List<ulong> clientIds)
        //{
        //    var json = JsonUtility.ToJson(payload);

        //    if (json.Length > NonFragmentedMessageMaxSize)
        //    {
        //        //throw new ArgumentException("Message is too large to be sent without fragmentation");
        //        SendFragmentedMessage(json, messageName, clientIds);
        //        return;
        //    }

        //    SendNamedMessage(json, messageName, clientIds);
        //}

        //public static void SendFragmentedMessage(string json, string messageName, List<ulong> clientIds)
        //{
        //    const int chunkSize = 1000;

        //    for (var i = 0; i < json.Length; i += chunkSize)
        //    {
        //        var partMessage = new PartMessage
        //        {
        //            Id = i,
        //            IsLastMessage = i > json.Length - chunkSize,
        //            Payload = json.Substring(i, Math.Min(chunkSize, json.Length - i))
        //        };
        //        SendNamedMessage(JsonUtility.ToJson(partMessage), messageName, clientIds);
        //    }
        //}

        //private static void SendNamedMessage(string json, string messageName, List<ulong> clientIds)
        //{
        //    using (var writer = new FastBufferWriter(json.Length, Allocator.Temp, int.MaxValue))
        //    {
        //        writer.WriteValueSafe(json);
        //        NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(messageName, clientIds, writer, NetworkDelivery.Reliable);
        //    }
        //}

        //public static FixedString32Bytes[] GetIdArrayForTransport(IEnumerable<string> ids)
        //{
        //    var idArray = new FixedString32Bytes[ids.Count()];
        //    for (var i = 0; i < idArray.Length; i++)
        //    {
        //        idArray[i] = new FixedString32Bytes(ids.ElementAt(i).EmptyIfNull());
        //    }

        //    return idArray;
        //}

        //public static string[] GetIdArray(FixedString32Bytes[] ids)
        //{
        //    var idArray = new string[ids.Count()];
        //    for (var i = 0; i < idArray.Length; i++)
        //    {
        //        var id = ids.ElementAt(i).ToString();
        //        var decoded = Convert.FromBase64String(id);
        //        idArray[i] = decoded.Length > 0
        //            ? new Guid(decoded).ToString()
        //            : string.Empty;
        //    }
        //    return idArray;
        //}

    }
}
