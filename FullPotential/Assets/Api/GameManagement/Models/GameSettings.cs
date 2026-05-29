using System;

namespace FullPotential.Api.GameManagement.Models
{
    [Serializable]
    public class GameSettings
    {
        public string LastSigninUsername;

        public string LastSigninToken;

        public string Culture;

        public float FieldOfView;

        public float LookSensitivity;

        public float LookSmoothness;
        
        public string ManagementApiAddress;
    }
}