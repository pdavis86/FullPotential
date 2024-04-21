using System;

namespace FullPotential.Api.Data
{
    [Serializable]
    public class GameSettings
    {
        public string LastSigninUsername;

        public string Culture;

        public float FieldOfView;

        public float LookSensitivity;

        public float LookSmoothness;
    }
}