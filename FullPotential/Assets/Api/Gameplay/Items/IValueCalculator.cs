using System;
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
        float GetEffectTimeBetween(ItemBase itemUsed, float min = 0.5f, float max = 1.5f);
        float GetEffectDuration(ItemBase itemUsed);
        float GetMovementForceValue(ItemBase itemUsed, bool adjustForGravity);
        (int Change, DateTime Expiry) GetStatChangeAndExpiry(ItemBase itemUsed, IStatEffect statEffect);
        (int Change, DateTime Expiry, float delay) GetStatChangeExpiryAndDelay(ItemBase itemUsed, IStatEffect statEffect);
        (int Change, DateTime Expiry) GetAttributeChangeAndExpiry(ItemBase itemUsed, IAttributeEffect attributeEffect);
    }
}