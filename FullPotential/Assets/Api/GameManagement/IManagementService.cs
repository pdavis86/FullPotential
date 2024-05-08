namespace FullPotential.Api.GameManagement
{
    using System;
    using System.Collections;
    using System.Threading.Tasks;
    using FullPotential.Api.GameManagement.JsonModels;

    public interface IManagementService
    {
        string SignInWithExistingToken();

        IEnumerator SignInWithPasswordCoroutine(string username, string password, Action<string> successCallback, Action<bool> failureCallback);

        IEnumerator ConnectionDetailsCoroutine(Action<ConnectionDetails> successCallback, Action failureCallback);

        IEnumerator SignOutCoroutine(Action successCallback, Action failureCallback);
        
        Task<bool> ValidateCredentials(string username, string token);
    }
}
