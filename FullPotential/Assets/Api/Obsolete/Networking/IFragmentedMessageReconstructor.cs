using System.Collections.Generic;
using FullPotential.Api.Obsolete.Networking.Data;

namespace FullPotential.Api.Obsolete.Networking
{
    public interface IFragmentedMessageReconstructor
    {
        IEnumerable<string> GetFragmentedMessages(object payload, int chunkSize = 1000);

        void AddMessage(FragmentedMessage fragmentedMessage);

        bool HaveAllMessages(string groupId);

        string Reconstruct(string groupId);
    }
}