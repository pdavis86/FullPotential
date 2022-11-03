﻿using System;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry.Effects;
using UnityEngine;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Gameplay.Items
{
    public interface IValueCalculator
    {
        int AddVariationToValue(double basicValue);
        int GetDamageValueFromAttack(ItemBase itemUsed, int targetDefense, bool addVariation = true);
        int GetDamageValueFromVelocity(Vector3 velocity);
        float GetEffectTimeBetween(Attributes attributes, float min = 0.5f, float max = 1.5f);
        float GetEffectDuration(Attributes attributes);
        float GetMovementForceValue(Attributes attributes, bool adjustForGravity);
        (int Change, DateTime Expiry) GetStatChangeAndExpiry(Attributes attributes, IStatEffect statEffect);
        (int Change, DateTime Expiry, float delay) GetStatChangeExpiryAndDelay(Attributes attributes, IStatEffect statEffect);
        (int Change, DateTime Expiry) GetAttributeChangeAndExpiry(Attributes attributes, IAttributeEffect attributeEffect);
    }
}