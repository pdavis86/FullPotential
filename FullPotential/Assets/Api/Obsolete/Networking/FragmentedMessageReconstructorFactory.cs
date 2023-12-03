namespace FullPotential.Api.Obsolete.Networking
{
    public class FragmentedMessageReconstructorFactory : IFragmentedMessageReconstructorFactory
    {
        public IFragmentedMessageReconstructor Create()
        {
            return new FragmentedMessageReconstructor();
        }
    }
}
