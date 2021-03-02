using UnityEngine;
using UnityEngine.Networking;

public class JoinOrHostGame : MonoBehaviour
{
    private NetworkManager _networkManager;

    void Start()
    {
        _networkManager = NetworkManager.singleton;
        //if (_networkManager.matchMaker == null)
        //{
        //    _networkManager.StartMatchMaker();
        //}
    }

    //public void CreateRoom()
    //{
    //    var matchSize = 4U;
    //    var matchAdvertise = true;
    //    var matchPassword = "";
    //    var publicClientAddress = "";
    //    var privateClientAddress = "";
    //    var eloScoreForMatch = 0;
    //    var requestDomain = 0;

    //    _networkManager.matchMaker.CreateMatch(
    //        _roomname ?? "My Room Name",
    //        matchSize,
    //        matchAdvertise,
    //        matchPassword,
    //        publicClientAddress,
    //        privateClientAddress,
    //        eloScoreForMatch,
    //        requestDomain,
    //        _networkManager.OnMatchCreate
    //        );
    //}

    public void HostGame()
    {
        _networkManager.networkAddress = "localhost";
        _networkManager.networkPort = 7777;
        _networkManager.StartHost();
    }

    public void JoinGame()
    {
        _networkManager.networkAddress = "localhost";
        _networkManager.networkPort = 7777;
        _networkManager.StartClient();
    }

    public void SetPlayerName(string value)
    {
       GameManager.Instance.PlayerName = value;
    }

    public void SetPlayerSkinUrl(string value)
    {
        GameManager.Instance.PlayerSkinUrl = value;
    }

    public void QuitGame()
    {
        GameManager.Quit();
    }

}
