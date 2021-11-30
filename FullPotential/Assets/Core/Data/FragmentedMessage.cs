// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedField.Global

using System;

namespace FullPotential.Assets.Core.Data
{
    [Serializable]
    public class FragmentedMessage
    {
        public string GroupId;
        public string GroupStartDateTime;
        public int FragmentCount;
        public int SequenceId;
        public string Payload;
    }
}
