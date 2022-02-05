using System.Collections.Generic;
using System.Linq;
using FullPotential.Core.Behaviours.Ui;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Behaviours.GameManagement
{
    public class MainCanvasObjects : MonoBehaviour
    {
        // ReSharper disable UnassignedField.Global
        // ReSharper disable MemberCanBePrivate.Global

        //Overlays
        public GameObject TooltipOverlay;
        public GameObject DebuggingOverlay;
        public GameObject HitNumberContainer;
        public GameObject InteractionBubble;
        public GameObject Hud;
        public GameObject Respawn;

        //Menus
        public GameObject EscMenu;
        public GameObject CharacterMenu;
        public GameObject SettingsUi;
        public GameObject DrawingPad;

        // ReSharper enable MemberCanBePrivate.Global
        // ReSharper enable UnassignedField.Global

        //Behaviours
        private Hud _hud;
        private CharacterMenuUi _characterMenuUi;
        private CharacterMenuUiCraftingTab _characterMenuUiCraftingTab;
        private CharacterMenuUiEquipmentTab _characterMenuUiEquipmentTab;

        private List<GameObject> _menus;

        private static MainCanvasObjects _instance;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            TooltipOverlay.SetActive(true);

            //NOTE: Be sure to add any new menus!
            _menus = new List<GameObject>
            {
                EscMenu,
                CharacterMenu,
                SettingsUi,
                DrawingPad
            };
        }

        public void HideAllMenus()
        {
            foreach (var menu in _menus)
            {
                menu.SetActive(false);
            }
        }

        public bool IsAnyMenuOpen()
        {
            return _menus.Any(x => x.activeSelf);
        }

        public void HideOthersOpenThis(GameObject ui)
        {
            HideAllMenus();
            ui.SetActive(true);
        }

        public Hud GetHud()
        {
            if (_hud == null)
            {
                _hud = Hud.GetComponent<Hud>();
            }

            return _hud;
        }

        private CharacterMenuUi GetCharacterMenuUi()
        {
            if (_characterMenuUi == null)
            {
                _characterMenuUi = CharacterMenu.GetComponent<CharacterMenuUi>();
            }

            return _characterMenuUi;
        }

        public CharacterMenuUiCraftingTab GetCharacterMenuUiCraftingTab()
        {
            if (_characterMenuUiCraftingTab == null)
            {
                _characterMenuUiCraftingTab = GetCharacterMenuUi().Crafting.GetComponent<CharacterMenuUiCraftingTab>();
            }

            return _characterMenuUiCraftingTab;
        }

        public CharacterMenuUiEquipmentTab GetCharacterMenuUiEquipmentTab()
        {
            if (_characterMenuUiEquipmentTab == null)
            {
                _characterMenuUiEquipmentTab = GetCharacterMenuUi().Equipment.GetComponent<CharacterMenuUiEquipmentTab>();
            }

            return _characterMenuUiEquipmentTab;
        }

    }
}
