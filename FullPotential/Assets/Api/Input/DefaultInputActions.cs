//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.4.3
//     from Assets/Core/Input/DefaultInputActions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @DefaultInputActions : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @DefaultInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""DefaultInputActions"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""815c6745-320b-4a33-8bf6-14d253e288ba"",
            ""actions"": [
                {
                    ""name"": ""Look"",
                    ""type"": ""Value"",
                    ""id"": ""8289bb25-2c68-490f-a791-4005c62a9000"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""a83e53cd-9ebb-468f-b7c2-1a7b0b16ac07"",
                    ""expectedControlType"": ""Analog"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""678f0c82-a744-4f1d-831e-30c9ad8a0726"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Interact"",
                    ""type"": ""Button"",
                    ""id"": ""3cafea86-b641-4062-9003-ef6df722819f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Cancel"",
                    ""type"": ""Button"",
                    ""id"": ""56a60f89-c9c4-4cc7-9f3c-e34efd3291c7"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""OpenCharacterMenu"",
                    ""type"": ""Button"",
                    ""id"": ""2c760578-4c23-4b5a-8f53-020d9e870699"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""AttackLeft"",
                    ""type"": ""Button"",
                    ""id"": ""230e98f7-4efe-4afe-9f23-7da36c1da81d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""AttackRight"",
                    ""type"": ""Button"",
                    ""id"": ""3ca23353-87c5-4063-a6c0-aba3637af680"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Pointer"",
                    ""type"": ""Value"",
                    ""id"": ""383f1ec7-05c9-4ef2-bd04-7c16efae4855"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""SprintStart"",
                    ""type"": ""Button"",
                    ""id"": ""06a053fb-6340-40f2-8c1c-fff457c3ca95"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""SprintStop"",
                    ""type"": ""Button"",
                    ""id"": ""6e38d72c-a28b-4502-91b6-faec96ec7dbd"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ShowCursorStart"",
                    ""type"": ""Button"",
                    ""id"": ""004e0517-c6bc-454f-bf81-fb2ca264fe5b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ShowCursorStop"",
                    ""type"": ""Button"",
                    ""id"": ""dc0f2407-36f9-44eb-bd2c-f488d938c055"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ReloadLeft"",
                    ""type"": ""Button"",
                    ""id"": ""9ba6aec8-c21a-49f9-b7c0-076a771875a7"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ReloadRight"",
                    ""type"": ""Button"",
                    ""id"": ""990555f7-2ddc-4f96-ac91-460c7ba1e885"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""AttackHoldLeft"",
                    ""type"": ""Button"",
                    ""id"": ""ea72a802-1f50-42e7-8b37-c3b1ff9c7962"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""AttackHoldRight"",
                    ""type"": ""Button"",
                    ""id"": ""c7d8554a-f191-41b4-bdac-d53116682867"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""88bd065f-44a9-4d6c-8290-c3240898ec26"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""78ca14de-9446-4569-a572-3f69c79c2e9f"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""52f4b7ba-7ca2-4067-a447-343b2a4e54f6"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Cancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""WSAD"",
                    ""id"": ""e216e539-fb3a-4fee-b379-9d8db82da33c"",
                    ""path"": ""2DVector(mode=1)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""9aa5466a-90b8-44db-8664-653900f632ab"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""48a99df9-2803-40d8-bb95-ac8c9fa33f08"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""14b0b0d8-8edf-4229-8e76-81fbab7277a1"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""f7269b67-1f7a-4a64-b292-7f99dd6a324d"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""c752d48a-7a24-4d74-b2d0-718756e70179"",
                    ""path"": ""<Pointer>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""57d68182-f32d-4810-af47-be964e543362"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""OpenCharacterMenu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""930b91fe-ef5b-4942-b230-e5a5a1e6ab48"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": ""Press(behavior=1)"",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""AttackLeft"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""443a47d7-6407-4439-8c16-ce1bb48e45e1"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": ""Press(behavior=1)"",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""AttackRight"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bded04b1-98c5-4a04-85cb-36ca47642b73"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8fa93374-6699-4211-8788-40031e358a06"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": ""Press(pressPoint=0.2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SprintStart"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8d35512f-3d44-41fc-b2dd-a2dccde05430"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": ""Press(behavior=1)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SprintStop"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1a94b1b7-7f27-4f2b-a556-5ec24ed85d1d"",
                    ""path"": ""<Keyboard>/leftCtrl"",
                    ""interactions"": ""Press(pressPoint=0.2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ShowCursorStart"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6f444393-f47b-4ef7-8f11-6771cd4983a7"",
                    ""path"": ""<Keyboard>/leftCtrl"",
                    ""interactions"": ""Press(behavior=1)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ShowCursorStop"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a36a97d4-d708-4d8c-9744-7a1fcf3201e1"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ReloadLeft"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""916cc168-04f3-47ca-9fc2-bb5556adf18b"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ReloadRight"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d80428bb-3829-4d20-94c7-00e0104f73b0"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": ""Hold"",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""AttackHoldLeft"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d606c90f-5be5-427b-adce-44de9ef69f78"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": ""Hold"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""AttackHoldRight"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard and Mouse"",
            ""bindingGroup"": ""Keyboard and Mouse"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<VirtualMouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Player
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_Look = m_Player.FindAction("Look", throwIfNotFound: true);
        m_Player_Move = m_Player.FindAction("Move", throwIfNotFound: true);
        m_Player_Jump = m_Player.FindAction("Jump", throwIfNotFound: true);
        m_Player_Interact = m_Player.FindAction("Interact", throwIfNotFound: true);
        m_Player_Cancel = m_Player.FindAction("Cancel", throwIfNotFound: true);
        m_Player_OpenCharacterMenu = m_Player.FindAction("OpenCharacterMenu", throwIfNotFound: true);
        m_Player_AttackLeft = m_Player.FindAction("AttackLeft", throwIfNotFound: true);
        m_Player_AttackRight = m_Player.FindAction("AttackRight", throwIfNotFound: true);
        m_Player_Pointer = m_Player.FindAction("Pointer", throwIfNotFound: true);
        m_Player_SprintStart = m_Player.FindAction("SprintStart", throwIfNotFound: true);
        m_Player_SprintStop = m_Player.FindAction("SprintStop", throwIfNotFound: true);
        m_Player_ShowCursorStart = m_Player.FindAction("ShowCursorStart", throwIfNotFound: true);
        m_Player_ShowCursorStop = m_Player.FindAction("ShowCursorStop", throwIfNotFound: true);
        m_Player_ReloadLeft = m_Player.FindAction("ReloadLeft", throwIfNotFound: true);
        m_Player_ReloadRight = m_Player.FindAction("ReloadRight", throwIfNotFound: true);
        m_Player_AttackHoldLeft = m_Player.FindAction("AttackHoldLeft", throwIfNotFound: true);
        m_Player_AttackHoldRight = m_Player.FindAction("AttackHoldRight", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Player
    private readonly InputActionMap m_Player;
    private IPlayerActions m_PlayerActionsCallbackInterface;
    private readonly InputAction m_Player_Look;
    private readonly InputAction m_Player_Move;
    private readonly InputAction m_Player_Jump;
    private readonly InputAction m_Player_Interact;
    private readonly InputAction m_Player_Cancel;
    private readonly InputAction m_Player_OpenCharacterMenu;
    private readonly InputAction m_Player_AttackLeft;
    private readonly InputAction m_Player_AttackRight;
    private readonly InputAction m_Player_Pointer;
    private readonly InputAction m_Player_SprintStart;
    private readonly InputAction m_Player_SprintStop;
    private readonly InputAction m_Player_ShowCursorStart;
    private readonly InputAction m_Player_ShowCursorStop;
    private readonly InputAction m_Player_ReloadLeft;
    private readonly InputAction m_Player_ReloadRight;
    private readonly InputAction m_Player_AttackHoldLeft;
    private readonly InputAction m_Player_AttackHoldRight;
    public struct PlayerActions
    {
        private @DefaultInputActions m_Wrapper;
        public PlayerActions(@DefaultInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Look => m_Wrapper.m_Player_Look;
        public InputAction @Move => m_Wrapper.m_Player_Move;
        public InputAction @Jump => m_Wrapper.m_Player_Jump;
        public InputAction @Interact => m_Wrapper.m_Player_Interact;
        public InputAction @Cancel => m_Wrapper.m_Player_Cancel;
        public InputAction @OpenCharacterMenu => m_Wrapper.m_Player_OpenCharacterMenu;
        public InputAction @AttackLeft => m_Wrapper.m_Player_AttackLeft;
        public InputAction @AttackRight => m_Wrapper.m_Player_AttackRight;
        public InputAction @Pointer => m_Wrapper.m_Player_Pointer;
        public InputAction @SprintStart => m_Wrapper.m_Player_SprintStart;
        public InputAction @SprintStop => m_Wrapper.m_Player_SprintStop;
        public InputAction @ShowCursorStart => m_Wrapper.m_Player_ShowCursorStart;
        public InputAction @ShowCursorStop => m_Wrapper.m_Player_ShowCursorStop;
        public InputAction @ReloadLeft => m_Wrapper.m_Player_ReloadLeft;
        public InputAction @ReloadRight => m_Wrapper.m_Player_ReloadRight;
        public InputAction @AttackHoldLeft => m_Wrapper.m_Player_AttackHoldLeft;
        public InputAction @AttackHoldRight => m_Wrapper.m_Player_AttackHoldRight;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterface != null)
            {
                @Look.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnLook;
                @Look.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnLook;
                @Look.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnLook;
                @Move.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Jump.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @Interact.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInteract;
                @Interact.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInteract;
                @Interact.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInteract;
                @Cancel.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCancel;
                @Cancel.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCancel;
                @Cancel.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCancel;
                @OpenCharacterMenu.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnOpenCharacterMenu;
                @OpenCharacterMenu.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnOpenCharacterMenu;
                @OpenCharacterMenu.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnOpenCharacterMenu;
                @AttackLeft.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttackLeft;
                @AttackLeft.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttackLeft;
                @AttackLeft.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttackLeft;
                @AttackRight.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttackRight;
                @AttackRight.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttackRight;
                @AttackRight.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttackRight;
                @Pointer.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPointer;
                @Pointer.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPointer;
                @Pointer.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPointer;
                @SprintStart.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSprintStart;
                @SprintStart.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSprintStart;
                @SprintStart.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSprintStart;
                @SprintStop.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSprintStop;
                @SprintStop.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSprintStop;
                @SprintStop.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSprintStop;
                @ShowCursorStart.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnShowCursorStart;
                @ShowCursorStart.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnShowCursorStart;
                @ShowCursorStart.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnShowCursorStart;
                @ShowCursorStop.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnShowCursorStop;
                @ShowCursorStop.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnShowCursorStop;
                @ShowCursorStop.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnShowCursorStop;
                @ReloadLeft.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnReloadLeft;
                @ReloadLeft.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnReloadLeft;
                @ReloadLeft.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnReloadLeft;
                @ReloadRight.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnReloadRight;
                @ReloadRight.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnReloadRight;
                @ReloadRight.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnReloadRight;
                @AttackHoldLeft.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttackHoldLeft;
                @AttackHoldLeft.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttackHoldLeft;
                @AttackHoldLeft.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttackHoldLeft;
                @AttackHoldRight.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttackHoldRight;
                @AttackHoldRight.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttackHoldRight;
                @AttackHoldRight.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttackHoldRight;
            }
            m_Wrapper.m_PlayerActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Look.started += instance.OnLook;
                @Look.performed += instance.OnLook;
                @Look.canceled += instance.OnLook;
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
                @Interact.started += instance.OnInteract;
                @Interact.performed += instance.OnInteract;
                @Interact.canceled += instance.OnInteract;
                @Cancel.started += instance.OnCancel;
                @Cancel.performed += instance.OnCancel;
                @Cancel.canceled += instance.OnCancel;
                @OpenCharacterMenu.started += instance.OnOpenCharacterMenu;
                @OpenCharacterMenu.performed += instance.OnOpenCharacterMenu;
                @OpenCharacterMenu.canceled += instance.OnOpenCharacterMenu;
                @AttackLeft.started += instance.OnAttackLeft;
                @AttackLeft.performed += instance.OnAttackLeft;
                @AttackLeft.canceled += instance.OnAttackLeft;
                @AttackRight.started += instance.OnAttackRight;
                @AttackRight.performed += instance.OnAttackRight;
                @AttackRight.canceled += instance.OnAttackRight;
                @Pointer.started += instance.OnPointer;
                @Pointer.performed += instance.OnPointer;
                @Pointer.canceled += instance.OnPointer;
                @SprintStart.started += instance.OnSprintStart;
                @SprintStart.performed += instance.OnSprintStart;
                @SprintStart.canceled += instance.OnSprintStart;
                @SprintStop.started += instance.OnSprintStop;
                @SprintStop.performed += instance.OnSprintStop;
                @SprintStop.canceled += instance.OnSprintStop;
                @ShowCursorStart.started += instance.OnShowCursorStart;
                @ShowCursorStart.performed += instance.OnShowCursorStart;
                @ShowCursorStart.canceled += instance.OnShowCursorStart;
                @ShowCursorStop.started += instance.OnShowCursorStop;
                @ShowCursorStop.performed += instance.OnShowCursorStop;
                @ShowCursorStop.canceled += instance.OnShowCursorStop;
                @ReloadLeft.started += instance.OnReloadLeft;
                @ReloadLeft.performed += instance.OnReloadLeft;
                @ReloadLeft.canceled += instance.OnReloadLeft;
                @ReloadRight.started += instance.OnReloadRight;
                @ReloadRight.performed += instance.OnReloadRight;
                @ReloadRight.canceled += instance.OnReloadRight;
                @AttackHoldLeft.started += instance.OnAttackHoldLeft;
                @AttackHoldLeft.performed += instance.OnAttackHoldLeft;
                @AttackHoldLeft.canceled += instance.OnAttackHoldLeft;
                @AttackHoldRight.started += instance.OnAttackHoldRight;
                @AttackHoldRight.performed += instance.OnAttackHoldRight;
                @AttackHoldRight.canceled += instance.OnAttackHoldRight;
            }
        }
    }
    public PlayerActions @Player => new PlayerActions(this);
    private int m_KeyboardandMouseSchemeIndex = -1;
    public InputControlScheme KeyboardandMouseScheme
    {
        get
        {
            if (m_KeyboardandMouseSchemeIndex == -1) m_KeyboardandMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard and Mouse");
            return asset.controlSchemes[m_KeyboardandMouseSchemeIndex];
        }
    }
    public interface IPlayerActions
    {
        void OnLook(InputAction.CallbackContext context);
        void OnMove(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
        void OnInteract(InputAction.CallbackContext context);
        void OnCancel(InputAction.CallbackContext context);
        void OnOpenCharacterMenu(InputAction.CallbackContext context);
        void OnAttackLeft(InputAction.CallbackContext context);
        void OnAttackRight(InputAction.CallbackContext context);
        void OnPointer(InputAction.CallbackContext context);
        void OnSprintStart(InputAction.CallbackContext context);
        void OnSprintStop(InputAction.CallbackContext context);
        void OnShowCursorStart(InputAction.CallbackContext context);
        void OnShowCursorStop(InputAction.CallbackContext context);
        void OnReloadLeft(InputAction.CallbackContext context);
        void OnReloadRight(InputAction.CallbackContext context);
        void OnAttackHoldLeft(InputAction.CallbackContext context);
        void OnAttackHoldRight(InputAction.CallbackContext context);
    }
}