using FullPotential.Assets.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FullPotential.Assets.Core.Networking
{
    public class FragmentedMessageReconstructor
    {
        private Dictionary<string, List<FragmentedMessage>> _messageGroups = new Dictionary<string, List<FragmentedMessage>>();

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
            return _messageGroups[groupId].Any(x => x.IsLastMessage);
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
