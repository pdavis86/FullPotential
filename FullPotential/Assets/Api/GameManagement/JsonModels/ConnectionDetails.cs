
// ReSharper disable UnassignedField.Global

namespace FullPotential.Api.GameManagement.JsonModels
{
    using System;
    using FullPotential.Api.GameManagement.Enums;

    [Serializable]
    public class ConnectionDetails
    {
        public InstanceState Status;
        public string Address;
        public int Port;
    }
}
