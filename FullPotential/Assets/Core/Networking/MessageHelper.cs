using FullPotential.Core.Data;
using FullPotential.Core.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullPotential.Core.Networking
{
    public static class MessageHelper
    {
        public static IEnumerable<string> GetFragmentedMessages(object payload, int chunkSize = 1000)
        {
            var json = JsonUtility.ToJson(payload);
            var groupId = Guid.NewGuid().ToMinimisedString();
            var groupStartDateTime = DateTime.UtcNow.ToString("u");
            var fragmentCount = (int)Math.Ceiling((float)json.Length / 1000);

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

    }
}
