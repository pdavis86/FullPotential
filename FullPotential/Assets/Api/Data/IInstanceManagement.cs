using System;
using System.Collections;

using FullPotential.Api.GameManagement.JsonModels;

namespace FullPotential.Api.Data
{
    public interface IInstanceManagement
    {
        IEnumerator ConnectionDetailsEnumerator(Action<ConnectionDetails> successCallback, Action failureCallback);
    }
}
