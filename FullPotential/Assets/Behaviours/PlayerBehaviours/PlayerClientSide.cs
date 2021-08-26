using FullPotential.Assets.Core.Data;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;
using System;
using TMPro;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler
// ReSharper disable UnassignedField.Global

public class PlayerClientSide : NetworkBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Camera _inFrontOfPlayerCamera;
#pragma warning restore 0649

    private bool _hasMenuOpen;
    private MainCanvasObjects _mainCanvasObjects;
    private bool _toggleGameMenu;
    private bool _toggleCharacterMenu;
    private PlayerState _playerState;
    private PlayerMovement _playerMovement;
    private Interactable _focusedInteractable;
    private Hud _hud;
    private Camera _sceneCamera;

    #region Unity event handlers

    private void Awake()
    {
        _playerState = GetComponent<PlayerState>();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        //var localClientIdMatches = _playerState.ClientId.Value == NetworkManager.Singleton.LocalClientId;
        //Debug.LogError($"PlayerClientSide - IsOwner: {IsOwner}, localClientIdMatches: {localClientIdMatches}, IsLocalPlayer: {IsLocalPlayer}");

        if (!IsOwner)
        {
            return;
        }

        _mainCanvasObjects = GameManager.Instance.MainCanvasObjects;
        _mainCanvasObjects.CraftingUi.SetActive(false);

        _hud = _mainCanvasObjects.Hud.GetComponent<Hud>();

        _mainCanvasObjects.Hud.SetActive(true);

        if (Debug.isDebugBuild)
        {
            _mainCanvasObjects.DebuggingOverlay.SetActive(true);
        }

        _sceneCamera = Camera.main;
        if (_sceneCamera != null)
        {
            _sceneCamera.gameObject.SetActive(false);
        }

        //Avoids weapons clipping with other objects
        _playerState.InFrontOfPlayer.transform.parent = _inFrontOfPlayerCamera.transform;
        FullPotential.Assets.Helpers.GameObjectHelper.SetGameLayerRecursive(_playerState.InFrontOfPlayer, FullPotential.Assets.Constants.Layers.InFrontOfPlayer);

        _inFrontOfPlayerCamera.gameObject.SetActive(true);
        _playerCamera.gameObject.SetActive(true);

        CustomMessagingManager.RegisterNamedMessageHandler(nameof(FullPotential.Assets.Core.Networking.MessageType.InventoryChange), OnInventoryChange);
    }

    void OnInteract()
    {
        if (_hasMenuOpen)
        {
            return;
        }

        if (_focusedInteractable == null)
        {
            //todo: play a sound to indicate a failed interaction
            return;
        }

        _playerState.InteractServerRpc(_focusedInteractable.gameObject.name);
    }

    void OnOpenCharacterMenu()
    {
        _toggleCharacterMenu = true;
    }

    void OnCancel()
    {
        _toggleGameMenu = true;
    }

    void OnLeftAttack()
    {
        TryToAttack(true);
    }

    void OnRightAttack()
    {
        TryToAttack(false);
    }

    void Update()
    {
        try
        {
            CheckForInteractable();

            //if (Input.GetKeyDown(mappings.GameMenu)) { _toggleGameMenu = true; }
            //else if (Input.GetKeyDown(mappings.CharacterMenu)) { _toggleCharacterMenu = true; }
            //else if (!HasMenuOpen)
            //{
            //    if (Input.GetKeyDown(mappings.Interact)) { TryToInteract(); }
            //    else if (Input.GetMouseButtonDown(0)) { TryToAttack(true); }
            //    else if (Input.GetMouseButtonDown(1)) { TryToAttack(false); }
            //    else
            //    {
            //        var mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");
            //        if (mouseScrollWheel > 0) { /*todo: scrolled up*/ Debug.Log("Positive mouse scroll"); }
            //        else if (mouseScrollWheel < 0) { /*todo: scrolled down*/ Debug.Log("Negative mouse scroll"); }
            //    }
            //}
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void FixedUpdate()
    {
        try
        {
            if (_toggleGameMenu || _toggleCharacterMenu)
            {
                if (_hasMenuOpen)
                {
                    _mainCanvasObjects.HideAllMenus();
                }
                else if (_toggleGameMenu)
                {
                    _mainCanvasObjects.HideOthersOpenThis(_mainCanvasObjects.EscMenu);
                }
                else if (_toggleCharacterMenu)
                {
                    _mainCanvasObjects.HideOthersOpenThis(_mainCanvasObjects.CharacterMenu);
                }

                Tooltips.HideTooltip();

                _toggleGameMenu = false;
                _toggleCharacterMenu = false;
            }

            _hasMenuOpen = _mainCanvasObjects.IsAnyMenuOpen();
            //Can't see alerts if we diable the hud - _mainCanvasObjects.Hud.SetActive(!_hasMenuOpen);
            _playerMovement.enabled = !_hasMenuOpen;

            if (_hasMenuOpen)
            {
                if (Cursor.lockState != CursorLockMode.None)
                {
                    Cursor.lockState = CursorLockMode.None;
                }
            }
            else if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;

        if (_mainCanvasObjects?.Hud != null)
        {
            _mainCanvasObjects.Hud.SetActive(false);
            _mainCanvasObjects.CraftingUi.SetActive(false);
        }

        if (_sceneCamera != null)
        {
            _sceneCamera.gameObject.SetActive(true);
        }

        if (GameManager.Instance?.MainCanvasObjects?.Hud != null)
        {
            GameManager.Instance.MainCanvasObjects.Hud.SetActive(false);
        }
    }

    private void OnInventoryChange(ulong clientId, System.IO.Stream stream)
    {
        //Debug.LogError("Recieved OnInventoryChange network message");

        string message;
        using (PooledNetworkReader reader = PooledNetworkReader.Get(stream))
        {
            message = reader.ReadString().ToString();
        }

        var changes = JsonUtility.FromJson<InventoryAndRemovals>(message);
        _playerState.Inventory.ApplyInventoryAndRemovals(changes);

        if (changes.IdsToRemove.Length > 0)
        {
            RefreshCraftingWindow();
        }
    }

    #endregion

    private void CheckForInteractable()
    {
        var ray = _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        if (Physics.Raycast(ray, out var hit, maxDistance: 1000))
        {
            var interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                var distance = Vector3.Distance(_playerCamera.transform.position, interactable.transform.position);
                if (distance <= interactable.Radius)
                {
                    if (interactable != _focusedInteractable)
                    {
                        if (_focusedInteractable != null)
                        {
                            _focusedInteractable.OnBlur();
                        }
                        _focusedInteractable = interactable;
                        _focusedInteractable.OnFocus();
                    }

                    return;
                }
            }
        }

        if (_focusedInteractable != null)
        {
            _focusedInteractable.OnBlur();
            _focusedInteractable = null;
        }
    }

    private void TryToAttack(bool leftHand)
    {
        if (_hasMenuOpen)
        {
            return;
        }

        _playerState.CastSpellServerRpc(leftHand, _playerCamera.transform.position, _playerCamera.transform.forward);
    }

    public void ShowAlert(string alertText)
    {
        _hud.ShowAlert(alertText);
    }

    public void ShowDamage(Vector3 position, string damage)
    {
        var hit = Instantiate(GameManager.Instance.Prefabs.Combat.HitText);
        hit.transform.SetParent(GameManager.Instance.MainCanvasObjects.HitNumberContainer.transform, false);
        hit.gameObject.SetActive(true);

        var hitText = hit.GetComponent<TextMeshProUGUI>();
        hitText.text = damage;

        const int maxDistanceForMinFontSize = 40;
        var distance = Vector3.Distance(Camera.main.transform.position, position);
        var fontSize = maxDistanceForMinFontSize - distance;
        if (fontSize < hitText.fontSizeMin) { fontSize = hitText.fontSizeMin; }
        else if (fontSize > hitText.fontSizeMax) { fontSize = hitText.fontSizeMax; }
        hitText.fontSize = fontSize;

        var sticky = hit.GetComponent<StickUiToWorldPosition>();
        sticky.WorldPosition = position;

        Destroy(hit, 1f);
    }

    public void RefreshCraftingWindow()
    {
        var craftingUi = GameManager.Instance.MainCanvasObjects.CraftingUi.GetComponent<CraftingMenuUi>();
        if (craftingUi.gameObject.activeSelf)
        {
            craftingUi.ResetUi();
            craftingUi.LoadInventory();
        }
    }

}
