using System;
using Imas.Live;
using LeadActress.Runtime.Loaders;
using LeadActress.Utilities;
using UnityEditor.Animations;
using UnityEngine;

namespace LeadActress.Runtime.Dancing {
    [AddComponentMenu("MLTD/MLTD Camera Animator")]
    public class MltdCameraAnimator : MonoBehaviour {

        public CommonResourceProperties commonResourceProperties;

        [Tooltip("The " + nameof(Camera) + " to operate. It should have a " + nameof(Animator) + " attached.")]
        public Camera targetCamera;

        [Tooltip("The " + nameof(IndirectCamera) + " to operate. It should have a " + nameof(Animator) + " attached.")]
        public IndirectCamera targetIndirectCamera;

        public ScenarioEventSignal mainScenarioSignal;

        public PlayerControl playerControl;

        public CameraAnimationLoader cameraLoader;

        public CameraControlMode cameraControlMode = CameraControlMode.Direct;

        private void Awake() {
            mainScenarioSignal.EventEmitted += OnScenarioEvent;
        }

        private void OnDestroy() {
            mainScenarioSignal.EventEmitted -= OnScenarioEvent;
        }

        private async void Start() {
            Animator cameraAnimator;

            switch (cameraControlMode) {
                case CameraControlMode.Direct:
                    cameraAnimator = targetCamera.GetComponent<Animator>();
                    break;
                case CameraControlMode.Indirect:
                    cameraAnimator = targetIndirectCamera.GetComponent<Animator>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Debug.Assert(cameraAnimator != null);

            var controller = await cameraLoader.LoadAsync(cameraControlMode);
            cameraAnimator.runtimeAnimatorController = controller;

            cameraAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            cameraAnimator.enabled = false;

            _cameraAnimator = cameraAnimator;
            _animatorController = controller;
        }

        private void Update() {
            if (playerControl.isOnPlaying) {
                _cameraAnimator.Rebind();
                _cameraAnimator.enabled = true;
            } else if (playerControl.isOnStopping) {
                _cameraAnimator.enabled = false;
                _cameraAnimator.ResetAllParameters();
            }
        }

        private void OnScenarioEvent(object sender, ScenarioSignalEventArgs e) {
            var ev = e.Data;

            var appealType = commonResourceProperties.appealType;

            switch (ev.type) {
                case (int)ScenarioDataType.AppealStart: {
                    if (appealType == AppealType.None) {
                        break;
                    }

                    var triggerName = CommonAnimationControllerBuilder.GetEnterTriggerNameFromAppealType(appealType);
                    _cameraAnimator.SetTrigger(triggerName);
                    _cameraAnimator.SetLayerWeight((int)appealType, 1);
                    Debug.Log($"Enter appeal: {triggerName}");

                    break;
                }
                case (int)ScenarioDataType.AppealEnd: {
                    if (appealType == AppealType.None) {
                        break;
                    }

                    var triggerName = CommonAnimationControllerBuilder.GetExitTriggerNameFromAppealType(appealType);
                    _cameraAnimator.SetTrigger(triggerName);
                    _cameraAnimator.SetLayerWeight((int)appealType, 0);
                    Debug.Log($"Exit appeal: {triggerName}");

                    break;
                }
                default:
                    break;
            }
        }

        private Animator _cameraAnimator;

        private AnimatorController _animatorController;

    }
}
