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
    public NetworkVariable<bool> IsLeftHand;

    private GameObject _sourcePlayer;
    private Spell _spell;
    private float _timeSinceLastEffective;
    private float _timeBetweenEffects;
    private Transform _cylinderTransform;

    private void Awake()
    {
        _cylinderTransform = transform.GetChild(0);

        _timeBetweenEffects = 0.5f;

        //Move the tip to the starting position
        _cylinderTransform.position += (_cylinderTransform.up * _cylinderTransform.localScale.y);
    }

    private void Start()
    {
        _sourcePlayer = GameObjectHelper.ClosestParentWithTag(gameObject, FullPotential.Assets.Core.Constants.Tags.Player);

        if (_sourcePlayer == null)
        {
            Debug.LogError("No player found in parents");
            Destroy(gameObject);
            return;
        }

        var playerState = _sourcePlayer.GetComponent<PlayerState>();

        transform.parent = playerState.PlayerCamera.transform;

        _spell = playerState.Inventory.GetItemWithId<Spell>(SpellId.Value);

        if (_spell == null)
        {
            Debug.LogError($"No spell found in player inventory with ID {SpellId.Value}");
            Destroy(gameObject);
            return;
        }

        if (PlayerClientId.Value == NetworkManager.Singleton.LocalClientId)
        {
            transform.position += transform.forward + (transform.right * 0.1f * (IsLeftHand.Value ? 1 : -1));
        }
    }

    private void FixedUpdate()
    {
        const int maxBeamLength = 10;

        //Vector3 endPosition;
        float beamLength;
        if (Physics.Raycast(transform.position, transform.forward, out var hit, maxDistance: maxBeamLength))
        {
            if (hit.transform.gameObject == _sourcePlayer.gameObject)
            {
                Debug.LogWarning("Beam is hitting the source player!");
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

                //Debug.Log($"Player {_sourcePlayer.name} is hitting {hit.transform.gameObject.name} with beam spell {_spell.Name} at distance {hit.distance}");

                if (NetworkManager.Singleton.IsServer)
                {
                    ApplySpellEffects(hit.transform.gameObject, hit.point);
                }
            }

            //endPosition = hit.point;
            beamLength = hit.distance;
        }
        else
        {
            //endPosition = transform.position + (transform.forward * maxBeamLength / 2);
            beamLength = maxBeamLength;
        }

        //Debug.DrawLine(transform.position, endPosition, Color.cyan);
        //Debug.DrawRay(transform.position, transform.forward, Color.cyan);

        if (!AreRoughlyEqual(_cylinderTransform.localScale.y * 2, beamLength))
        {
            _cylinderTransform.localScale = new Vector3(_cylinderTransform.localScale.x, beamLength / 2, _cylinderTransform.localScale.z);
            _cylinderTransform.position = transform.position + (_cylinderTransform.up * _cylinderTransform.localScale.y);
        }
    }

    private bool AreRoughlyEqual(float value1, float value2)
    {
        var rounded1 = Mathf.RoundToInt(value1 * 100);
        var rounded2 = Mathf.RoundToInt(value2 * 100);

        return rounded1 == rounded2;
    }

    public void ApplySpellEffects(GameObject target, Vector3? position)
    {
        AttackHelper.DealDamage(_sourcePlayer, _spell, target, position);
    }

}
