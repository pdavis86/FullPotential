using System;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using UnityEngine;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Gameplay.Combat
{
    public interface IValueCalculator
    {
        int AddVariationToValue(double basicValue);
        int GetAttackValue(Attributes? attributes, int targetDefense);
        float GetReloadTime(Attributes attributes);
        int GetVelocityDamage(Vector3 velocity);
        float GetProjectileRange(Attributes attributes);
        float GetContinuousRange(Attributes attributes);
        int GetAmmoMax(Attributes attributes);
        float GetTimeBetweenEffects(Attributes attributes, float min = 0.5f, float max = 1.5f);
        float GetProjectileSpeed(Attributes attributes);
        float GetShapeLifetime(Attributes attributes);
        float GetDuration(Attributes attributes);
        float GetForceValue(Attributes attributes, bool adjustForGravity);
        (int Change, DateTime Expiry) GetStatChangeAndExpiry(Attributes attributes, IStatEffect statEffect);
        (int Change, DateTime Expiry, float delay) GetStatChangeExpiryAndDelay(Attributes attributes, IStatEffect statEffect);
        (int Change, DateTime Expiry) GetAttributeChangeAndExpiry(Attributes attributes, IAttributeEffect attributeEffect);
    }
}