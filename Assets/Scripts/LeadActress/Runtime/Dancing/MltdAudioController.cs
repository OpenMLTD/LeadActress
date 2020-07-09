using LeadActress.Runtime.Loaders;
using UnityEngine;

namespace LeadActress.Runtime.Dancing {
    [AddComponentMenu("MLTD/MLTD Audio Controller")]
    public class MltdAudioController : MonoBehaviour {

        [Tooltip(nameof(AudioSource) + " for background music")]
        public AudioSource bgm;

        public PlayerControl playerControl;

        public ExternalAudioLoader externalAudio;

        private async void Start() {
            if (externalAudio != null && externalAudio.enabled) {
                var clip = await externalAudio.LoadAsync();
                bgm.clip = clip;
            }
        }

        private void Update() {
            if (playerControl.isOnPlaying) {
                bgm.Play();
            } else if (playerControl.isOnStopping) {
                bgm.Stop();
            }
        }

    }
}
