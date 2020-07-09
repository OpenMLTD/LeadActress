using UnityEngine;

namespace LeadActress.Runtime {
    [AddComponentMenu("MLTD/Force 60 FPS")]
    public class Fps60 : MonoBehaviour {

        private void Awake() {
            Application.targetFrameRate = 60;
        }

    }
}
