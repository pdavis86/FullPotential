// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedField.Global

using System;

namespace FullPotential.Assets.Core.Data
{
    [System.Serializable]
    public class FragmentedMessage
    {
        public string GroupId;
        public string GroupStartDateTime;
        public int SequenceId;
        public bool IsLastMessage;
        public string Payload;
    }
}
