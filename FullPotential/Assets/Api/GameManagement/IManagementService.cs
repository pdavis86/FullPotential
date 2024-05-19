namespace FullPotential.Api.GameManagement
{
    using System;
    using System.Collections;
    using FullPotential.Api.GameManagement.JsonModels;

    public interface IManagementService
    {
        string SignInWithExistingToken();

        IEnumerator SignInWithPasswordEnumerator(string username, string password, Action<string> successCallback, Action<bool> failureCallback);

        IEnumerator ConnectionDetailsEnumerator(Action<ConnectionDetails> successCallback, Action failureCallback);

        IEnumerator SignOutEnumerator(Action successCallback, Action failureCallback);

        IEnumerator ValidateCredentialsEnumerator(string username, string token, Action successCallback, Action failureCallback);
    }
}
