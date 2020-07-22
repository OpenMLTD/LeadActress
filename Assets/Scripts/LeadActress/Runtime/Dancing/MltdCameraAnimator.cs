using System;
using LeadActress.Runtime.Loaders;
using UnityEditor.Animations;
using UnityEngine;

namespace LeadActress.Runtime.Dancing {
    [AddComponentMenu("MLTD/MLTD Camera Animator")]
    public class MltdCameraAnimator : MonoBehaviour {

        [Tooltip("The " + nameof(Camera) + " to operate. It should have a " + nameof(Animator) + " attached.")]
        public Camera targetCamera;

        [Tooltip("The " + nameof(IndirectCamera) + " to operate. It should have a " + nameof(Animator) + " attached.")]
        public IndirectCamera targetIndirectCamera;

        public PlayerControl playerControl;

        public CameraAnimationLoader cameraLoader;

        public CameraControlMode cameraControlMode = CameraControlMode.Direct;

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
            }
        }

        private Animator _cameraAnimator;

        private AnimatorController _animatorController;

    }
}
