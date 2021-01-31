using Assets.Scripts.Attributes;
using Assets.Scripts.Crafting.Results;
using System;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global

public class PlayerCast : MonoBehaviour
{
    public GameObject SpellPrefab;
    public GameObject HitTextUiPrefab;
    public Transform DamageNumbersParent;

    private Camera _camera;

    private void Start()
    {
        _camera = transform.Find("PlayerCamera").GetComponent<Camera>();
    }

    void Update()
    {
        try
        {
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
    void CastSpell(bool leftHand = false)
    {
        //todo: check the player has the spell and cna cast it
        if (GetComponent<PlayerToggles>().HasMenuOpen)
        {
            return;
        }

        //todo: which spell is active?
        var activeSpell = new Spell
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

        if (activeSpell.Targeting == Spell.TargetingOptions.Projectile)
        {
            var startPos = transform.position + _camera.transform.forward + new Vector3(leftHand ? -0.15f : 0.15f, -0.1f, 0);
            var spell = Instantiate(SpellPrefab, startPos, transform.rotation);
            spell.SetActive(true);

            var spellRb = spell.GetComponent<Rigidbody>();
            spellRb.AddForce(_camera.transform.forward * 20f, ForceMode.VelocityChange);

            var spellScript = spell.GetComponent<SpellBehaviour>();
            spellScript.HitTextUiPrefab = HitTextUiPrefab;
            spellScript.Player = gameObject;
            spellScript.PlayerCamera = _camera;
            spellScript.DamageNumbersParent = DamageNumbersParent;

            //todo: finish this
            spellScript.Spell = activeSpell;
        }
        else
        {
            //todo: other spell targeting options
        }
    }

}
