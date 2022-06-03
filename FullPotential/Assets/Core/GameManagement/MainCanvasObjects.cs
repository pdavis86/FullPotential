using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.Ui;
using FullPotential.Core.Ui.Behaviours;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.GameManagement
{
    //todo: rename at some point e.g. UserInterface
    public class MainCanvasObjects : MonoBehaviour, IUserInterface
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
        private GameObject _customMenu;

        // ReSharper enable MemberCanBePrivate.Global
        // ReSharper enable UnassignedField.Global

        //Behaviours
        private Hud _hud;
        private CharacterMenuUi _characterMenuUi;
        private CharacterMenuUiCraftingTab _characterMenuUiCraftingTab;
        private CharacterMenuUiEquipmentTab _characterMenuUiEquipmentTab;

        private List<GameObject> _menus;

        private static MainCanvasObjects _instance;

        public IHud HudOverlay
        {
            get
            {
                return _hud ??= Hud.GetComponent<Hud>();
            }
        }

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

            if (_customMenu != null)
            {
                _customMenu.SetActive(false);
                _customMenu = null;
            }
        }

        public bool IsAnyMenuOpen()
        {
            return _menus.Any(x => x.activeSelf) || _customMenu != null;
        }

        public void HideOthersOpenThis(GameObject ui)
        {
            HideAllMenus();
            ui.SetActive(true);
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

        public void OpenCustomMenu(GameObject menuGameObject)
        {
            _customMenu = menuGameObject;
            _customMenu.SetActive(true);
        }

    }
}
