using LeadActress.Runtime.Loaders;
using UnityEngine;

namespace LeadActress.Runtime.Dancing {
    [AddComponentMenu("MLTD/MLTD Camera Animator")]
    public class MltdCameraAnimator : MonoBehaviour {

        public PlayerControl playerControl;

        public CameraAnimationLoader cameraLoader;

        [Tooltip("The animator to animate camera. The " + nameof(Animator) + " should be attached to an object which has a " + nameof(IndirectCamera) + " script.")]
        public Animator cameraAnimator;

        [Tooltip("The animation that the camera plays. The " + nameof(Animation) + " should be attached to an object which has a " + nameof(IndirectCamera) + " script.")]
        public Animation cameraAnimation;

        private async void Start() {
            Debug.Assert(cameraAnimator != null);

            var clip = await cameraLoader.LoadAsync();

            cameraAnimation.AddClip(clip, clip.name);
            cameraAnimation.clip = clip;
        }

        private void Update() {
            if (playerControl.isOnPlaying) {
                cameraAnimation.Play();
            } else if (playerControl.isOnStopping) {
                cameraAnimation.Stop();
            }
        }

    }
}
