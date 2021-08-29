using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;
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

            //NOTE: payload size limited to 65527
            //todo: use compression for messages? - var jsonCompressed = Assets.Core.Helpers.CompressionHelper.CompressString(json);

            var json = JsonUtility.ToJson(payload);
            var stream = PooledNetworkBuffer.Get();
            using (PooledNetworkWriter writer = PooledNetworkWriter.Get(stream))
            {
                writer.WriteString(json);
                CustomMessagingManager.SendNamedMessage(messageName, clientId, stream);
            }
        }

    }
}
