using System;

namespace FullPotential.Api.GameManagement
{
    [Serializable]
    public class GameSettings
    {
        //todo: zzz v0.5 - Remove GameSettings.Username
        [Obsolete("Use LastSigninUsername instead")]
        public string Username;

        public string LastSigninUsername;

        public string Culture;

        public float FieldOfView;

        public float LookSensitivity;

        public float LookSmoothness;
    }
}