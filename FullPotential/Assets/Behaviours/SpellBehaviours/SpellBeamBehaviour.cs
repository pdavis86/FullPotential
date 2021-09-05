using FullPotential.Assets.Behaviours.SpellBehaviours;
using FullPotential.Assets.Core.Helpers;
using FullPotential.Assets.Core.Registry.Types;
using MLAPI;
using MLAPI.NetworkVariable;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Local

public class SpellBeamBehaviour : NetworkBehaviour, ISpellBehaviour
{
    public NetworkVariable<ulong> PlayerClientId;
    public NetworkVariable<string> SpellId;

    private GameObject _sourcePlayer;
    private Spell _spell;
    private float _timeSinceLastEffective;
    private float _timeBetweenEffects;

    private void Start()
    {
        if (!IsServer)
        {
            //No need to Debug.LogError(). We only want this behaviour on the server
            return;
        }

        _sourcePlayer = NetworkManager.Singleton.ConnectedClients[PlayerClientId.Value].PlayerObject.gameObject;

        _spell = _sourcePlayer.GetComponent<PlayerState>().Inventory.GetItemWithId<Spell>(SpellId.Value);

        if (_spell == null)
        {
            Debug.LogError($"No spell found in player inventory with ID {SpellId.Value}");
            return;
        }

        _timeBetweenEffects = 0.5f;

        //Move the tip to the starting position
        transform.position += (transform.up * transform.localScale.y / 2);
    }

    private void FixedUpdate()
    {
        const int maxBeamLength = 10;

        var startPosition = transform.position - (transform.up * (transform.localScale.y));

        //Standing still sometimes makes the Raycast go in the wrong direction!
        var rayStart = startPosition + (transform.up * 0.1f);

        //Vector3 endPosition;
        float beamLength;
        if (Physics.Raycast(rayStart, transform.up, out var hit, maxDistance: maxBeamLength))
        {
            if (hit.transform.gameObject == _sourcePlayer.gameObject)
            {
                //Debug.LogWarning("Beam is hitting the source player!");
                return;
            }

            //Debug.Log("Beam is hitting the object " + hit.transform.name);

            if (_timeSinceLastEffective < _timeBetweenEffects)
            {
                _timeSinceLastEffective += Time.deltaTime;
            }
            else
            {
                _timeSinceLastEffective = 0;

                Debug.Log($"Player {_sourcePlayer.name} is hitting {hit.transform.gameObject.name} with beam spell {_spell.Name} at distance {hit.distance}");

                ApplySpellEffects(hit.transform.gameObject, hit.point);
            }

            //endPosition = hit.point;
            beamLength = hit.distance;
        }
        else
        {
            //endPosition = transform.position + (transform.up * maxBeamLength / 2);
            beamLength = maxBeamLength;
        }

        //Debug.DrawLine(rayStart, endPosition, Color.cyan, 1f);
        //Debug.DrawRay(rayStart, transform.up, Color.cyan, 1f);

        transform.localScale = new Vector3(transform.localScale.x, beamLength / 2, transform.localScale.z);
        transform.position = startPosition + (transform.up * transform.localScale.y);
    }

    public void ApplySpellEffects(GameObject target, Vector3? position)
    {
        AttackHelper.DealDamage(_sourcePlayer, _spell, target, position);
    }

}
