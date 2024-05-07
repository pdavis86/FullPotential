namespace FullPotential.Api.GameManagement
{
    using System;
    using System.Collections;
    using FullPotential.Api.GameManagement.JsonModels;

    public interface IManagementService
    {
        IEnumerator SignInWithPasswordEnumerator(string userName, string password, Action<string> successCallback, Action failureCallback);

        IEnumerator ConnectionDetailsCoroutine(Action<ConnectionDetails> successCallback, Action failureCallback);
    }
}
