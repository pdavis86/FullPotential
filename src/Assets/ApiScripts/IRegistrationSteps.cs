using System;
using System.Collections.Generic;

namespace Assets.ApiScripts
{
    public interface IRegistrationSteps
    {
        IEnumerable<Type> GetRegisterables();
    }
}
