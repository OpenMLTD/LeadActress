using LeadActress.Runtime.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LeadActress.Runtime {
    [AddComponentMenu("MLTD/Player Control")]
    public class PlayerControl : MonoBehaviour, DefaultControls.IDefaultGameplayActions {

        public bool isPlaying { get; private set; }

        public bool isOnPlaying { get; private set; }

        public bool isOnStopping { get; private set; }

        public float relativeTime { get; private set; }

        private void OnEnable() {
            if (_controls == null) {
                _controls = new DefaultControls();
                _controls.DefaultGameplay.SetCallbacks(this);
            }

            _controls.DefaultGameplay.Enable();
        }

        private void OnDisable() {
            _controls.DefaultGameplay.Disable();
        }

        private void Update() {
            if (_isPlayingJustChanged) {
                if (isPlaying) {
                    isOnPlaying = true;
                } else {
                    isOnStopping = true;
                }

                _isPlayingJustChanged = false;
            } else {
                isOnPlaying = false;
                isOnStopping = false;
            }

            if (isPlaying) {
                relativeTime = Time.time - _startTime;
            }
        }

        void DefaultControls.IDefaultGameplayActions.OnTogglePlayback(InputAction.CallbackContext context) {
            FlipPlayingState();
        }

        private void FlipPlayingState() {
            isPlaying = !isPlaying;

            if (isPlaying) {
                _startTime = Time.time;
            } else {
                relativeTime = 0;
            }

            _isPlayingJustChanged = true;
        }

        private float _startTime;

        private bool _isPlayingJustChanged;

        private DefaultControls _controls;

    }
}
