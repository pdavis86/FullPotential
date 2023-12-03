using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FullPotential.Api.Obsolete.Networking.Data;
using FullPotential.Api.Utilities.Extensions;
using UnityEngine;

namespace FullPotential.Api.Obsolete.Networking
{
    public class FragmentedMessageReconstructor : IFragmentedMessageReconstructor
    {
        private readonly Dictionary<string, List<FragmentedMessage>> _messageGroups = new Dictionary<string, List<FragmentedMessage>>();

        public IEnumerable<string> GetFragmentedMessages(object payload, int chunkSize = 1000)
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

        private void ClearOldMessageGroups()
        {
            var oldGroups = _messageGroups
                .Where(x =>
                    x.Value.Count == 0
                    || GetGroupDateTimeStart(x.Value.First().GroupId) < DateTime.UtcNow.AddMinutes(-1))
                .ToList();

            foreach (var group in oldGroups)
            {
                _messageGroups.Remove(group.Key);
            }
        }

        public void AddMessage(FragmentedMessage fragmentedMessage)
        {
            ClearOldMessageGroups();

            if (!_messageGroups.ContainsKey(fragmentedMessage.GroupId))
            {
                _messageGroups.Add(fragmentedMessage.GroupId, new List<FragmentedMessage>());
            }

            _messageGroups[fragmentedMessage.GroupId].Add(fragmentedMessage);
        }

        public bool HaveAllMessages(string groupId)
        {
            return _messageGroups[groupId].Count > 0
                && _messageGroups[groupId].Count == _messageGroups[groupId].First().FragmentCount;
        }

        public string Reconstruct(string groupId)
        {
            var sb = new StringBuilder();

            foreach (var message in _messageGroups[groupId].OrderBy(x => x.SequenceId))
            {
                sb.Append(message.Payload);
            }

            _messageGroups.Remove(groupId);
            ClearOldMessageGroups();

            return sb.ToString();
        }

        private DateTime? GetGroupDateTimeStart(string groupId)
        {
            if (_messageGroups[groupId].Count == 0)
            {
                return null;
            }

            return DateTime.Parse(_messageGroups[groupId].First().GroupStartDateTime);
        }
    }
}
