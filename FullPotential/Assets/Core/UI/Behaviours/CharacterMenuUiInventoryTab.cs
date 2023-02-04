using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Player;
using FullPotential.Core.Ui.Behaviours;
using FullPotential.Core.Ui.Components;
using FullPotential.Core.UI.Events;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.UI.Behaviours
{
    public class CharacterMenuUiInventoryTab : MonoBehaviour
    {
        private const string EventSource = nameof(CharacterMenuUiInventoryTab);

#pragma warning disable 0649
        [SerializeField] private GameObject _componentsContainer;
        [SerializeField] private GameObject _inventoryRowPrefab;
#pragma warning restore 0649

        //Services
        private ILocalizer _localizer;

        private PlayerState _playerState;
        private CharacterMenuUi _characterMenuUi;
        private DrawingPadUi _drawingPadUi;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();

            _playerState = GameManager.Instance.LocalGameDataStore.PlayerGameObject.GetComponent<PlayerState>();

            _characterMenuUi = GameManager.Instance.UserInterface.CharacterMenu.GetComponent<CharacterMenuUi>();
            _drawingPadUi = GameManager.Instance.UserInterface.DrawingPad.GetComponent<DrawingPadUi>();

            _drawingPadUi.OnDrawingStop += HandleOnDrawingStop;
        }

        // ReSharper disable once UnusedMember.Local
        private void OnEnable()
        {
            LoadInventory();
        }

        private void LoadInventory()
        {
            InventoryItemsList.LoadInventoryItems(
                null,
                _componentsContainer,
                _inventoryRowPrefab,
                _playerState.Inventory,
                null,
                null,
                true,
                HandleAssignedShapeButtonClick
            );
        }

        private void HandleAssignedShapeButtonClick(IPlayerInventory playerInventory, string itemId, InventoryUiRow rowScript)
        {
            var assignedShape = playerInventory.GetAssignedShape(itemId);

            if (assignedShape.IsNullOrWhiteSpace())
            {
                _drawingPadUi.InitialiseForAssign(EventSource, itemId);
                _characterMenuUi.DarkOverlay.SetActive(true);
                _drawingPadUi.gameObject.SetActive(true);
            }
            else
            {
                playerInventory.SetAssignedShape(itemId, null);
                rowScript.AssignedShapeText.text = InventoryItemsList.AssignedShapeNone;
            }
        }

        private void HandleOnDrawingStop(object sender, OnDrawingStopEventArgs e)
        {
            if (e.EventSource != EventSource)
            {
                return;
            }

            _characterMenuUi.DarkOverlay.SetActive(false);
            _drawingPadUi.gameObject.SetActive(false);

            var success = _playerState.Inventory.SetAssignedShape(e.ItemId, e.DrawnShape);

            if (!success)
            {
                //todo: alert goes behind UI
                GameManager.Instance.GetUserInterface().HudOverlay.ShowAlert(_localizer.Translate("ui.drawingpad.alreadyinuse"));
                return;
            }

            LoadInventory();
        }
    }
}
