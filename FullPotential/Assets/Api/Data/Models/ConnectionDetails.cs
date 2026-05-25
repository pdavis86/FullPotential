using System;

using FullPotential.Api.GameManagement;

// ReSharper disable UnassignedField.Global

namespace FullPotential.Api.Data.Models
{
    [Serializable]
    public class ConnectionDetails
    {
        public InstanceState Status;
        public string Address;
        public int Port;
    }
}
