using Assets.Scripts.Attributes;
using Assets.Scripts.Crafting.Results;
using System;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

[RequireComponent(typeof(PlayerController))]
public class PlayerCast : MonoBehaviour
{
    public Camera PlayerCamera;

    private PlayerController _playerController;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        try
        {
            //todo: fire1 and fire2 instead?
            if (Input.GetMouseButtonDown(0))
            {
                CastSpell();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                CastSpell(true);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    [ServerSideOnlyTemp]
    private Spell GetPlayerActiveSpell()
    {
        //todo: check the player has a spell active and can cast it
        return new Spell
        {
            Name = "test spell",
            Targeting = Spell.TargetingOptions.Projectile,
            Attributes = new Attributes
            {
                Strength = 50
            },
            Effects = new List<string> { Spell.ElementalEffects.Fire },
            Shape = Spell.ShapeOptions.Wall
        };
    }

    [ServerSideOnlyTemp]
    private void CastSpell(bool leftHand = false)
    {
        if (_playerController.HasMenuOpen)
        {
            return;
        }

        var activeSpell = GetPlayerActiveSpell();

        if (activeSpell == null)
        {
            return;
        }

        switch (activeSpell.Targeting)
        {
            case Spell.TargetingOptions.Projectile:
                var startPos = transform.position + PlayerCamera.transform.forward + new Vector3(leftHand ? -0.15f : 0.15f, -0.1f, 0);
                var spell = Instantiate(ObjectAccess.Instance.PrefabSpell, startPos, transform.rotation);
                spell.SetActive(true);

                //todo: force should vary
                var spellRb = spell.GetComponent<Rigidbody>();
                spellRb.AddForce(PlayerCamera.transform.forward * 20f, ForceMode.VelocityChange);

                var spellScript = spell.GetComponent<SpellBehaviour>();
                spellScript.PlayerCamera = PlayerCamera;
                spellScript.Spell = activeSpell;

                break;

            //todo: other spell targeting options
            //case Spell.TargetingOptions.Self:
            //case Spell.TargetingOptions.Touch:
            //case Spell.TargetingOptions.Beam:
            //case Spell.TargetingOptions.Cone:

            default:
                throw new Exception("Unexpected spell targeting: " + activeSpell.Targeting);
        }
    }

}
