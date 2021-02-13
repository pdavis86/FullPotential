using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class HostGame : MonoBehaviour
{
    private NetworkManager _networkManager;
    private string _roomname;

    void Start()
    {
        _networkManager = NetworkManager.singleton;
        if (_networkManager.matchMaker == null)
        {
            _networkManager.StartMatchMaker();
        }
    }

    public void SetRoomName(string roomName)
    {
        _roomname = roomName;
    }

    public void CreateRoom()
    {
        var matchSize = 4U;
        var matchAdvertise = true;
        var matchPassword = "";
        var publicClientAddress = "";
        var privateClientAddress = "";
        var eloScoreForMatch = 0;
        var requestDomain = 0;

        _networkManager.matchMaker.CreateMatch(
            _roomname ?? "My Room Name",
            matchSize,
            matchAdvertise,
            matchPassword,
            publicClientAddress,
            privateClientAddress,
            eloScoreForMatch,
            requestDomain,
            _networkManager.OnMatchCreate
            );
    }

}
