// GENERATED AUTOMATICALLY FROM 'Assets/Config/DefaultControls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace LeadActress.Runtime.Input
{
    public class @DefaultControls : IInputActionCollection, IDisposable
    {
        public InputActionAsset asset { get; }
        public @DefaultControls()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""DefaultControls"",
    ""maps"": [
        {
            ""name"": ""DefaultGameplay"",
            ""id"": ""ebfff3d1-f6ad-4b99-ae36-c7705fe7f042"",
            ""actions"": [
                {
                    ""name"": ""Toggle Playback"",
                    ""type"": ""Button"",
                    ""id"": ""30943a07-a415-481a-84f3-9e2af6fc9f92"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""219b7e68-32f5-4f75-8ab4-93b74596728e"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard and Mouse"",
                    ""action"": ""Toggle Playback"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d18a8285-f898-4ab0-a8dc-2bb99ca7ce5e"",
                    ""path"": ""<Touchscreen>/primaryTouch/tap"",
                    ""interactions"": ""Hold"",
                    ""processors"": """",
                    ""groups"": ""Touchscreen"",
                    ""action"": ""Toggle Playback"",
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
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": true,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Touchscreen"",
            ""bindingGroup"": ""Touchscreen"",
            ""devices"": [
                {
                    ""devicePath"": ""<Touchscreen>"",
                    ""isOptional"": true,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
            // DefaultGameplay
            m_DefaultGameplay = asset.FindActionMap("DefaultGameplay", throwIfNotFound: true);
            m_DefaultGameplay_TogglePlayback = m_DefaultGameplay.FindAction("Toggle Playback", throwIfNotFound: true);
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

        // DefaultGameplay
        private readonly InputActionMap m_DefaultGameplay;
        private IDefaultGameplayActions m_DefaultGameplayActionsCallbackInterface;
        private readonly InputAction m_DefaultGameplay_TogglePlayback;
        public struct DefaultGameplayActions
        {
            private @DefaultControls m_Wrapper;
            public DefaultGameplayActions(@DefaultControls wrapper) { m_Wrapper = wrapper; }
            public InputAction @TogglePlayback => m_Wrapper.m_DefaultGameplay_TogglePlayback;
            public InputActionMap Get() { return m_Wrapper.m_DefaultGameplay; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(DefaultGameplayActions set) { return set.Get(); }
            public void SetCallbacks(IDefaultGameplayActions instance)
            {
                if (m_Wrapper.m_DefaultGameplayActionsCallbackInterface != null)
                {
                    @TogglePlayback.started -= m_Wrapper.m_DefaultGameplayActionsCallbackInterface.OnTogglePlayback;
                    @TogglePlayback.performed -= m_Wrapper.m_DefaultGameplayActionsCallbackInterface.OnTogglePlayback;
                    @TogglePlayback.canceled -= m_Wrapper.m_DefaultGameplayActionsCallbackInterface.OnTogglePlayback;
                }
                m_Wrapper.m_DefaultGameplayActionsCallbackInterface = instance;
                if (instance != null)
                {
                    @TogglePlayback.started += instance.OnTogglePlayback;
                    @TogglePlayback.performed += instance.OnTogglePlayback;
                    @TogglePlayback.canceled += instance.OnTogglePlayback;
                }
            }
        }
        public DefaultGameplayActions @DefaultGameplay => new DefaultGameplayActions(this);
        private int m_KeyboardandMouseSchemeIndex = -1;
        public InputControlScheme KeyboardandMouseScheme
        {
            get
            {
                if (m_KeyboardandMouseSchemeIndex == -1) m_KeyboardandMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard and Mouse");
                return asset.controlSchemes[m_KeyboardandMouseSchemeIndex];
            }
        }
        private int m_TouchscreenSchemeIndex = -1;
        public InputControlScheme TouchscreenScheme
        {
            get
            {
                if (m_TouchscreenSchemeIndex == -1) m_TouchscreenSchemeIndex = asset.FindControlSchemeIndex("Touchscreen");
                return asset.controlSchemes[m_TouchscreenSchemeIndex];
            }
        }
        public interface IDefaultGameplayActions
        {
            void OnTogglePlayback(InputAction.CallbackContext context);
        }
    }
}
