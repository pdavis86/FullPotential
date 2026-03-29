using System;
using System.Collections;

using FullPotential.Api.GameManagement.JsonModels;
using FullPotential.Api.Data;

namespace FullPotential.Core.Persistence.Local
{
    public class InstanceManagement : IInstanceManagement
    {
        public IEnumerator ConnectionDetailsEnumerator(Action<ConnectionDetails> successCallback, Action failureCallback)
        {
            throw new NotImplementedException();
        }
    }
}
