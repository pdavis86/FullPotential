using Assets.Scripts.Attributes;
using Assets.Scripts.Crafting.Results;
using Assets.Scripts.Networking;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

[RequireComponent(typeof(PlayerController))]
public class PlayerCast : NetworkBehaviour2
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
            if (Input.GetMouseButtonDown(0))
            {
                CmdCastSpell();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                CmdCastSpell(true);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    //todo: move this
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

    [ServerSideOnly]
    private void CmdCastSpell(bool leftHand = false)
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
                var spell = Instantiate(GameManager.Instance.GameObjects.PrefabSpell, startPos, transform.rotation);
                //todo: set parent
                spell.SetActive(true);

                var castSpeed = activeSpell.Attributes.Speed / 50f;
                if (castSpeed < 0.5)
                {
                    castSpeed = 0.5f;
                }

                var spellRb = spell.GetComponent<Rigidbody>();
                spellRb.AddForce(PlayerCamera.transform.forward * 20f * castSpeed, ForceMode.VelocityChange);

                var spellScript = spell.GetComponent<SpellBehaviour>();
                spellScript.PlayerCamera = PlayerCamera;
                spellScript.Spell = activeSpell;

#pragma warning disable CS0618 // Type or member is obsolete
                spell.AddComponent<NetworkIdentity>();
#pragma warning restore CS0618 // Type or member is obsolete

                NetworkServer2.Spawn(spell);

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
