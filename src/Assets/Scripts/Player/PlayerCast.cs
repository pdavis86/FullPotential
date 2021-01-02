using Assets.Scripts.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCast : MonoBehaviour
{
    public GameObject Spell;

    private Camera _camera;

    private void Start()
    {
        _camera = transform.Find("PlayerCamera").GetComponent<Camera>();
    }

    void Update()
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

    void CastSpell(bool leftHand = false)
    {
        var spell = Instantiate(Spell, transform.position + _camera.transform.forward + new Vector3(leftHand ? -0.15f : 0.15f, -0.1f, 0), transform.rotation);
        spell.SetActive(true);

        var spellRb = spell.GetComponent<Rigidbody>();
        spellRb.AddForce(_camera.transform.forward * 20f, ForceMode.VelocityChange);

        //spell.AddComponent<Impact>();
        ////spellRb.useGravity = true;
        ////spellRb.mass = 0.1f;

        spell.AddComponent<Damage>();
        spell.GetComponent<Collider>().isTrigger = true;
    }
}
