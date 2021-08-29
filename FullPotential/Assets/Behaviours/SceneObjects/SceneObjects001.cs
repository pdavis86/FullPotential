using MLAPI;
using MLAPI.Messaging;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

public class SceneObjects001 : NetworkBehaviour
{
    private NetworkObject _playerPrefabNetObj;
    private List<Transform> _spawnPoints;

    public override void NetworkStart()
    {
        base.NetworkStart();

        _playerPrefabNetObj = GameManager.Instance.Prefabs.Player.GetComponent<NetworkObject>();

        var spawnPointsParent = FullPotential.Assets.Core.Helpers.UnityHelper.GetObjectAtRoot(GameManager.NameSpawnPoints).transform;
        _spawnPoints = new List<Transform>();
        foreach (Transform spawnPoint in spawnPointsParent)
        {
            _spawnPoints.Add(spawnPoint);
        }

        if (NetworkManager.Singleton.IsHost)
        {
            SpawnPlayer(NetworkManager.Singleton.LocalClientId, GameManager.Instance.DataStore.PlayerToken);
        }
        else
        {
            HeresMyJoiningDetailsServerRpc(GameManager.Instance.DataStore.PlayerToken);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void HeresMyJoiningDetailsServerRpc(string token, ServerRpcParams serverRpcParams = default)
    {
        SpawnPlayer(serverRpcParams.Receive.SenderClientId, token);
    }

    private void SpawnPlayer(ulong clientId, string playerToken)
    {
        var chosenSpawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Count)];
        var playerNetObj = Instantiate(_playerPrefabNetObj, chosenSpawnPoint.position, chosenSpawnPoint.rotation);

        var playerState = playerNetObj.GetComponent<PlayerState>();

        playerNetObj.SpawnAsPlayerObject(clientId);

        var playerData = FullPotential.Assets.Core.Registry.UserRegistry.Load(playerToken);
        playerState.LoadFromPlayerData(playerData);
    }

}
