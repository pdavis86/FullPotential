using FullPotential.Api.GameManagement.Models;

using UnityEngine;

namespace FullPotential.Core.GameManagement.Data
{
    public class LocalGameData
    {
        public SignInResult? SignInResult { get; set; }

        public GameObject PlayerGameObject { get; set; }

        public bool HasDisconnected { get; set; }

        public string DisconnectReason { get; set; }
    }
}
