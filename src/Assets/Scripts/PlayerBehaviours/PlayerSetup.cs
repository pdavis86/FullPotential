using Assets.Scripts.Crafting.Results;
using Assets.Scripts.Data;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private TextMeshProUGUI _nameTag;
    [SerializeField] private MeshRenderer _mainMesh;
    [SerializeField] private MeshRenderer _leftMesh;
    [SerializeField] private MeshRenderer _rightMesh;

    [SyncVar]
    public string Username;

    [SyncVar]
    public string TextureUri;

    private Camera _sceneCamera;

    //todo: when is this false?
    private bool _debugging = true;

    private void Start()
    {
        _sceneCamera = Camera.main;

        gameObject.name = "Player ID " + netId.Value;

        if (!isLocalPlayer)
        {
            gameObject.GetComponent<PlayerController>().enabled = false;

            _nameTag.text = string.IsNullOrWhiteSpace(Username) ? "Player " + netId.Value : Username;

            if (!string.IsNullOrWhiteSpace(TextureUri))
            {
                SetPlayerTexture(TextureUri);
            }

            return;
        }

        if (_sceneCamera != null)
        {
            _sceneCamera.gameObject.SetActive(false);
        }

        _playerCamera.gameObject.SetActive(true);

        GameManager.Instance.MainCanvasObjects.Hud.SetActive(true);

        var pm = gameObject.AddComponent<PlayerMovement>();
        pm.PlayerCamera = _playerCamera;

        //Done on network manager now
        //ClientScene.RegisterPrefab(_sceneObjects.PrefabSpell);

        _nameTag.text = null;

        if (!string.IsNullOrWhiteSpace(GameManager.Instance.PlayerSkinUrl))
        {
            string filePath;
            if (!GameManager.Instance.PlayerSkinUrl.StartsWith("http"))
            {
                //todo: upload file?
                filePath = GameManager.Instance.PlayerSkinUrl;
            }
            else
            {
                //todo: download file
                filePath = GameManager.Instance.PlayerSkinUrl;
            }

            if (System.IO.File.Exists(filePath))
            {
                SetPlayerTexture(filePath);
                TextureUri = filePath;
            }
        }

        CmdHeresMyJoiningDetails(GameManager.Instance.PlayerName, GameManager.Instance.PlayerSkinUrl);

        Load();
    }

    private void OnDisable()
    {
        Save();

        if (GameManager.Instance.MainCanvasObjects.Hud != null) { GameManager.Instance.MainCanvasObjects.Hud.SetActive(false); }
        if (_sceneCamera != null) { _sceneCamera.gameObject.SetActive(true); }
    }

    void SetPlayerTexture(string playerSkinUri)
    {
        //todo: download file
        var filePath = playerSkinUri;

        var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
        tex.LoadImage(System.IO.File.ReadAllBytes(filePath));
        var newMat = new Material(_mainMesh.material.shader);
        newMat.mainTexture = tex;

        _mainMesh.material = newMat;

        if (isLocalPlayer)
        {
            _leftMesh.material = newMat;
            _rightMesh.material = newMat;
        }
    }

    [Command]
    void CmdHeresMyJoiningDetails(string playerName, string playerSkinUri)
    {
        if (!string.IsNullOrWhiteSpace(playerName)) { Username = playerName; }
        if (!string.IsNullOrWhiteSpace(playerSkinUri)) { TextureUri = playerSkinUri; }
        RpcSetPlayerDetails(playerName, playerSkinUri);
    }

    [ClientRpc]
    void RpcSetPlayerDetails(string playerName, string playerSkinUri)
    {
        if (!isLocalPlayer)
        {
            _nameTag.text = string.IsNullOrWhiteSpace(playerName) ? "Player " + netId.Value : playerName;
            if (!string.IsNullOrWhiteSpace(playerSkinUri)) { SetPlayerTexture(playerSkinUri); }
        }
    }

    [ServerCallback]
    private void Load()
    {
        var inv = GetComponent<Inventory>();

        //todo: finish this
        var loadJson = System.IO.File.ReadAllText(@"D:\temp\playerguid.json");
        var loadData = JsonUtility.FromJson<PlayerSave>(loadJson);

        if (loadData.Loot != null) { inv.Items.AddRange(loadData.Loot); }
        if (loadData.Accessories != null) { inv.Items.AddRange(loadData.Accessories); }
        if (loadData.Armor != null) { inv.Items.AddRange(loadData.Armor); }
        if (loadData.Spells != null) { inv.Items.AddRange(loadData.Spells); }
        if (loadData.Weapons != null) { inv.Items.AddRange(loadData.Weapons); }

        Debug.Log($"There are {inv.Items.Count} item in the inventory after loading");
    }

    [ServerCallback]
    private void Save()
    {
        var inv = GetComponent<Inventory>();



        //todo: remove this
        inv.Add(GameManager.Instance.ResultFactory.GetLootDrop());
        inv.Add(GameManager.Instance.ResultFactory.GetLootDrop());
        var weapon = GameManager.Instance.ResultFactory.GetMeleeWeapon("Sword", inv.Items, false);
        inv.Add(weapon);



        var groupedItems = inv.Items.GroupBy(x => x.GetType());

        var saveData = new PlayerSave
        {
            Loot = groupedItems.FirstOrDefault(x => x.Key == typeof(ItemBase))?.ToArray(),
            Accessories = groupedItems.FirstOrDefault(x => x.Key == typeof(Accessory))?.Select(x => x as Accessory).ToArray() as Accessory[],
            Armor = groupedItems.FirstOrDefault(x => x.Key == typeof(Armor))?.Select(x => x as Armor).ToArray() as Armor[],
            Spells = groupedItems.FirstOrDefault(x => x.Key == typeof(Spell))?.Select(x => x as Spell).ToArray() as Spell[],
            Weapons = groupedItems.FirstOrDefault(x => x.Key == typeof(Weapon))?.Select(x => x as Weapon).ToArray() as Weapon[]
        };

        var saveJson = JsonUtility.ToJson(saveData, _debugging);
        System.IO.File.WriteAllText(@"D:\temp\playerguid.json", saveJson);
    }

}
