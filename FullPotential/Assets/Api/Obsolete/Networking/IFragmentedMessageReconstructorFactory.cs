using System;

namespace FullPotential.Api.Obsolete.Networking
{
    [Obsolete]
    public interface IFragmentedMessageReconstructorFactory
    {
        IFragmentedMessageReconstructor Create();
    }
}
