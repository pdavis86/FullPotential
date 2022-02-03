using System;

namespace FullPotential.Core.Data
{
    [Serializable]
    public struct FragmentedMessage
    {
        public string GroupId;
        public string GroupStartDateTime;
        public int FragmentCount;
        public int SequenceId;
        public string Payload;
    }
}
