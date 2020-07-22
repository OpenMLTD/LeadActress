using Imas.Live;
using JetBrains.Annotations;
using LeadActress.Runtime.Loaders;
using LeadActress.Utilities;
using UnityEditor.Animations;
using UnityEngine;

namespace LeadActress.Runtime.Dancing {
    [AddComponentMenu("MLTD/MLTD Model Animator")]
    public sealed class MltdModelAnimator : MonoBehaviour {

        public CommonResourceProperties commonResourceProperties;

        public ModelLoader modelLoader;

        public DanceAnimationLoader danceLoader;

        public ScenarioEventSignal mainScenarioSignal;

        [Tooltip("Landscape or portrait scenario, not the main one")]
        public ScenarioEventSignal variantScenarioSignal;

        public MltdFacialAnimator facialAnimator;

        public PlayerControl playerControl;

        public ModelPlacement placement;

        private void Awake() {
            if (facialAnimator == null) {
                Debug.LogError("Facial animator cannot be null.");
            }

            mainScenarioSignal.EventEmitted += OnScenarioEvent;
            variantScenarioSignal.EventEmitted += OnScenarioEvent;
        }

        private void OnDestroy() {
            mainScenarioSignal.EventEmitted -= OnScenarioEvent;
            variantScenarioSignal.EventEmitted -= OnScenarioEvent;
        }

        private async void Start() {
            if (placement.formationNumber < MltdSimulationConstants.MinDanceFormation || placement.formationNumber > MltdSimulationConstants.MaxDanceFormation) {
                Debug.LogError($"Invalid formation number: {placement.formationNumber}, should be {MltdSimulationConstants.MinDanceFormation} to {MltdSimulationConstants.MaxDanceFormation}.");
                return;
            }

            var loaded = await modelLoader.LoadAndInstantiateAsync();

            loaded.Body.name = GetIdolObjectName(placement.formationNumber);

            var modelAnimator = loaded.Body.GetComponent<Animator>();
            Debug.Assert(modelAnimator != null);

            modelAnimator.keepAnimatorControllerStateOnDisable = false;

            var controller = await danceLoader.LoadAsync();
            modelAnimator.runtimeAnimatorController = controller;

            // Remember to set this flag otherwise there will be some flickering when switching target idols
            modelAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            modelAnimator.enabled = false;

            _modelRoot = loaded.Body;
            _animatorController = controller;
            _modelAnimator = modelAnimator;

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

        private void OnScenarioEvent(object sender, ScenarioSignalEventArgs e) {
            var ev = e.Data;

            switch (ev.type) {
                case (int)ScenarioDataType.FormationChange: {
                    var appealType = commonResourceProperties.appealType;

                    if (ev.layer == 0 || ev.layer == (int)appealType) {
                        var formation = ev.formation;
                        Vector4 idolOffset;

                        if (formation.Length < placement.formationNumber) {
                            idolOffset = Vector4.zero;
                        } else {
                            idolOffset = formation[placement.formationNumber - 1];
                        }

                        var t = _modelRoot.transform;

                        var euler = t.localEulerAngles;
                        euler.y = idolOffset.w;
                        t.localEulerAngles = euler;

                        t.localPosition = new Vector3(idolOffset.x, idolOffset.y, idolOffset.z);
                    }

                    break;
                }
                case (int)ScenarioDataType.DanceAnimationSeekFrame: {
                    if (ev.idol == placement.formationNumber + 10) {
                        var seconds = (float)ev.seekFrame / FrameRate.Mltd;
                        _modelAnimator.PlayInFixedTime(CommonAnimationControllerBuilder.SeekFrameTargetName, (int)AppealType.None, seconds);
                    }

                    break;
                }
                case (int)ScenarioDataType.AppealStart: {
                    var appealType = commonResourceProperties.appealType;

                    if (appealType == AppealType.None) {
                        break;
                    }

                    var triggerName = CommonAnimationControllerBuilder.GetEnterTriggerNameFromAppealType(appealType);
                    _modelAnimator.SetTrigger(triggerName);
                    _modelAnimator.SetLayerWeight((int)appealType, 1);

                    break;
                }
                case (int)ScenarioDataType.AppealEnd: {
                    var appealType = commonResourceProperties.appealType;

                    if (appealType == AppealType.None) {
                        break;
                    }

                    var triggerName = CommonAnimationControllerBuilder.GetExitTriggerNameFromAppealType(appealType);
                    _modelAnimator.SetTrigger(triggerName);
                    _modelAnimator.SetLayerWeight((int)appealType, 0);

                    break;
                }
                default:
                    break;
            }
        }

        private void Update() {
            if (playerControl.isOnPlaying) {
                _modelAnimator.enabled = true;
            } else if (playerControl.isOnStopping) {
                _modelAnimator.enabled = false;
                _modelAnimator.Rebind();
                _modelAnimator.ResetAllParameters();
            }
        }

        private GameObject _modelRoot;

        private Animator _modelAnimator;

        private AnimatorController _animatorController;

    }
}
