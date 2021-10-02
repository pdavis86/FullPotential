using FullPotential.Assets.Behaviours.SpellBehaviours;
using FullPotential.Assets.Core.Constants;
using FullPotential.Assets.Core.Helpers;
using FullPotential.Assets.Core.Registry.Types;
using FullPotential.Assets.Core.Spells.Shapes;
using FullPotential.Assets.Extensions;
using MLAPI;
using MLAPI.NetworkVariable;
using System;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global

public class SpellProjectileBehaviour : NetworkBehaviour, ISpellBehaviour
{
    public NetworkVariable<ulong> PlayerClientId;
    public NetworkVariable<string> SpellId;
    public NetworkVariable<Vector3> SpellDirection;

    private GameObject _sourcePlayer;
    private PlayerState _platerState;
    private Spell _spell;
    private Type _shapeType;

    private void Start()
    {
        if (!IsServer)
        {
            //No need to Debug.LogError(). We only want this behaviour on the server
            return;
        }

        Destroy(gameObject, 3f);

        _sourcePlayer = NetworkManager.Singleton.ConnectedClients[PlayerClientId.Value].PlayerObject.gameObject;

        Physics.IgnoreCollision(GetComponent<Collider>(), _sourcePlayer.GetComponent<Collider>());

        _platerState = _sourcePlayer.GetComponent<PlayerState>();

        _spell = _platerState.Inventory.GetItemWithId<Spell>(SpellId.Value);

        if (_spell == null)
        {
            Debug.LogError($"No spell found in player inventory with ID {SpellId.Value}");
            return;
        }

        var castSpeed = _spell.Attributes.Speed / 50f;
        if (castSpeed < 0.5)
        {
            castSpeed = 0.5f;
        }

        var affectedByGravity = _spell.Shape != null;

        _shapeType = _spell.Shape?.GetType();

        var rigidBody = GetComponent<Rigidbody>();

        rigidBody.AddForce(SpellDirection.Value * 20f * castSpeed, ForceMode.VelocityChange);

        if (affectedByGravity)
        {
            rigidBody.useGravity = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
        {
            return;
        }

        //Debug.Log("Collided with " + other.gameObject.name);

        if (!other.gameObject.CompareTagAny(Tags.Player, Tags.Enemy, Tags.Ground))
        {
            return;
        }

        ApplySpellEffects(other.gameObject, other.ClosestPointOnBounds(transform.position));
    }

    public void ApplySpellEffects(GameObject target, Vector3? position)
    {
        if (_shapeType == null)
        {
            AttackHelper.DealDamage(_sourcePlayer, _spell, target, position);
        }
        else
        {
            Vector3 spawnPosition;
            if (!target.CompareTagAny(Tags.Player, Tags.Enemy))
            {
                spawnPosition = position.Value;
            }
            else
            {
                var pointUnderTarget = new Vector3(target.transform.position.x, -100, target.transform.position.z);
                var targetsFeet = target.GetComponent<Collider>().ClosestPointOnBounds(pointUnderTarget);
                if (Physics.Raycast(targetsFeet, transform.up * -1, out var hit))
                {
                    spawnPosition = hit.point;
                }
                else
                {
                    spawnPosition = position.Value;
                }
            }

            if (_shapeType == typeof(Wall))
            {
                var rotation = Quaternion.LookRotation(SpellDirection.Value);
                rotation.x = 0;
                rotation.z = 0;
                _platerState.SpawnSpellWall(_spell, spawnPosition, rotation, PlayerClientId.Value);
            }
            else if (_shapeType == typeof(Zone))
            {
                _platerState.SpawnSpellZone(_spell, spawnPosition, PlayerClientId.Value);
            }
            else
            {
                Debug.LogError($"Unexpected secondary effect for spell {_spell.Id} '{_spell.Name}'");
            }
        }

        Destroy(gameObject);
    }

}
