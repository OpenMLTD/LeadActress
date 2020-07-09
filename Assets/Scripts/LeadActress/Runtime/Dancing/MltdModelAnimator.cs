using JetBrains.Annotations;
using LeadActress.Runtime.Loaders;
using UnityEngine;

namespace LeadActress.Runtime.Dancing {
    [AddComponentMenu("MLTD/MLTD Model Animator")]
    public sealed class MltdModelAnimator : MonoBehaviour {

        public ModelLoader modelLoader;

        public DanceAnimationLoader danceLoader;

        [Tooltip("Landscape or portrait scenario, not the main one")]
        public ScenarioLoader scenarioLoader;

        public MltdFacialAnimator facialAnimator;

        public PlayerControl playerControl;

        public int formationNumber {
            get => _formationNumber;
            set => _formationNumber = value;
        }

        private void Awake() {
            if (facialAnimator == null) {
                Debug.LogError("Facial animator cannot be null.");
            }
        }

        private async void Start() {
            if (formationNumber < MltdSimulationConstants.MinDanceFormation || formationNumber > MltdSimulationConstants.MaxDanceFormation) {
                Debug.LogError($"Invalid formation number: {formationNumber}, should be {MltdSimulationConstants.MinDanceFormation} to {MltdSimulationConstants.MaxDanceFormation}.");
                return;
            }

            var loaded = await modelLoader.LoadAndInstantiateAsync();

            loaded.Body.name = GetIdolObjectName(formationNumber);

            var modelAnimator = loaded.Body.GetComponent<Animator>();
            Debug.Assert(modelAnimator != null);

            var modelAnimation = loaded.Body.AddComponent<Animation>();

            var scenario = await scenarioLoader.LoadAsync();

            var clip = await danceLoader.LoadAsync(scenario);

            modelAnimation.AddClip(clip, clip.name);
            modelAnimation.clip = clip;

            _modelAnimation = modelAnimation;

            {
                var face = loaded.Head.transform.Find("obj_head_GP/face").gameObject;
                var meshRenderer = face.GetComponent<SkinnedMeshRenderer>();
                facialAnimator.faceRenderer = meshRenderer;
            }
        }

        [NotNull]
        public static string GetIdolObjectName(int formationNumber) {
            return $"Idol {formationNumber.ToString()}";
        }

        private void Update() {
            if (playerControl.isOnPlaying) {
                _modelAnimation.Play();
            } else if (playerControl.isOnStopping) {
                _modelAnimation.Stop();
            }
        }

        private Animation _modelAnimation;

        [Tooltip("Which position does this idol stand.")]
        [SerializeField]
        [Range(MltdSimulationConstants.MinDanceFormation, MltdSimulationConstants.MaxDanceFormation)]
        private int _formationNumber = MltdSimulationConstants.MinDanceFormation;

    }
}
