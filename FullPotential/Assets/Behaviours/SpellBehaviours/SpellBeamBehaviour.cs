using FullPotential.Assets.Behaviours.SpellBehaviours;
using FullPotential.Assets.Core.Helpers;
using FullPotential.Assets.Core.Registry.Types;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Local

public class SpellBeamBehaviour : NetworkBehaviour, ISpellBehaviour
{
    public readonly NetworkVariable<ulong> PlayerClientId = new NetworkVariable<ulong>();
    public readonly NetworkVariable<FixedString32Bytes> SpellId = new NetworkVariable<FixedString32Bytes>();
    public readonly NetworkVariable<bool> IsLeftHand = new NetworkVariable<bool>();

    private GameObject _sourcePlayer;
    private Spell _spell;
    private float _timeBetweenEffects;
    private float _timeSinceLastEffective;
    private Transform _cylinderParentTransform;
    private Transform _cylinderTransform;

    private void Awake()
    {
        _cylinderParentTransform = transform.GetChild(0);
        _cylinderTransform = _cylinderParentTransform.GetChild(0);
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

        _spell = playerState.Inventory.GetItemWithId<Spell>(SpellId.Value.ToString());

        if (_spell == null)
        {
            Debug.LogError($"No spell found in player inventory with ID {SpellId.Value}");
            Destroy(gameObject);
            return;
        }

        _timeBetweenEffects = 0.5f;
        _timeSinceLastEffective = _timeBetweenEffects;

        _cylinderParentTransform.parent = playerState.PlayerCamera.transform;

        if (PlayerClientId.Value == NetworkManager.Singleton.LocalClientId)
        {
            //Move it a little forwards
            _cylinderParentTransform.position += transform.forward;

            //Move it a little sideways
            _cylinderParentTransform.position += (IsLeftHand.Value ? 0.1f : -0.1f) * _cylinderParentTransform.right;
        }

        //Move the tip to the middle
        _cylinderTransform.position += (_cylinderTransform.up * _cylinderTransform.localScale.y);
    }

    private void FixedUpdate()
    {
        const int maxBeamLength = 10;

        //Vector3 endPosition;
        float beamLength;
        if (Physics.Raycast(transform.position, transform.forward, out var hit, maxBeamLength, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
        {
            if (hit.transform.gameObject == _sourcePlayer)
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

                ApplySpellEffects(hit.transform.gameObject, hit.point);
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

        if (!MathsHelper.AreRoughlyEqual(_cylinderTransform.localScale.y * 2, beamLength))
        {
            _cylinderTransform.localScale = new Vector3(_cylinderTransform.localScale.x, beamLength / 2, _cylinderTransform.localScale.z);
            _cylinderTransform.position = _cylinderParentTransform.position + (_cylinderTransform.up * _cylinderTransform.localScale.y);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        Destroy(_cylinderTransform.gameObject);
    }

    public void ApplySpellEffects(GameObject target, Vector3? position)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        AttackHelper.DealDamage(_sourcePlayer, _spell, target, position);
    }

}
