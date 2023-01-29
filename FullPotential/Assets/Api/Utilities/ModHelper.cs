﻿using FullPotential.Api.GameManagement;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Api.Utilities
{
    public class ModHelper : IModHelper
    {
        public virtual IGameManager GetGameManager()
        {
            return UnityEngine.GameObject.Find("GameManager").GetComponent<IGameManager>();
        }
    }

}
