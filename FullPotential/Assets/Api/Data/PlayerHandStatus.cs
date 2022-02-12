﻿using FullPotential.Api.Registry.Spells;
using UnityEngine;

namespace FullPotential.Api.Data
{
    public class PlayerHandStatus
    {
        public bool IsReloading;
        public int Ammo;
        public int AmmoMax;
        public Spell SpellBeingCast;
        public GameObject SpellBeingCastGameObject;
    }
}