using FullPotential.Assets.Core.Registry.Types;
using MLAPI;
using MLAPI.NetworkVariable;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global

public class SpellSelfBehaviour : NetworkBehaviour
{
    const float _distanceBeforeReturning = 8f;

    public NetworkVariable<ulong> PlayerClientId;
    public NetworkVariable<string> SpellId;
    public NetworkVariable<Vector3> SpellDirection;

    private GameObject _sourcePlayer;
    private Spell _spell;
    private float _castSpeed;
    private Rigidbody _rigidbody;
    private bool _returningToPlayer;

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

        _castSpeed = _spell.Attributes.Speed / 50f;
        if (_castSpeed < 0.5)
        {
            _castSpeed = 0.5f;
        }

        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.AddForce(SpellDirection.Value * 20f * _castSpeed, ForceMode.VelocityChange);
    }

    private void FixedUpdate()
    {
        var distanceFromPlayer = Vector3.Distance(transform.position, _sourcePlayer.transform.position);

        if (!_returningToPlayer)
        {
            if (distanceFromPlayer >= _distanceBeforeReturning)
            {
                Debug.LogError("Far enough away, now returning");
                _returningToPlayer = true;
                ClearForce();
            }

            return;
        }

        if (distanceFromPlayer > 0.1f)
        {
            ClearForce();
            var playerDirection = (_sourcePlayer.transform.position - transform.position).normalized;
            _rigidbody.AddForce(playerDirection * 20f * _castSpeed, ForceMode.VelocityChange);
            return;
        }

        Debug.LogError("Finally got back to the player!");
        Destroy(gameObject);
    }

    private void ClearForce()
    {
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }

}
