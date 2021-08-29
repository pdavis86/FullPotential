using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;
using System.Collections.Generic;
using UnityEngine;

namespace FullPotential.Assets.Helpers
{
    public static class MessageHelper
    {
        public static void SendMessageIfNotHost(object payload, string messageName, ulong clientId)
        {
            if (clientId == 0)
            {
                return;
            }

            SendMessageIfNotHost(payload, messageName, new List<ulong> { clientId });
        }

        public static void SendMessageIfNotHost(object payload, string messageName, List<ulong> clientIds = null)
        {
            //NOTE: payload size limited to 65527
            //todo: use compression for messages? - var jsonCompressed = Assets.Core.Helpers.CompressionHelper.CompressString(json);

            var json = JsonUtility.ToJson(payload);
            var stream = PooledNetworkBuffer.Get();
            using (PooledNetworkWriter writer = PooledNetworkWriter.Get(stream))
            {
                writer.WriteString(json);
                CustomMessagingManager.SendNamedMessage(messageName, clientIds, stream);
            }
        }

    }
}
