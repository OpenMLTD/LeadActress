using Imas.Live;
using LeadActress.Runtime.Loaders;
using UnityEngine;

namespace LeadActress.Runtime.Dancing {
    [AddComponentMenu("MLTD/Indirect Camera")]
    public class IndirectCamera : MonoBehaviour {

        [Tooltip("The target camera, which must have " + nameof(Animator) + " and " + nameof(Animation) + " attached.")]
        public Camera targetCamera;

        public PlayerControl playerControl;

        [Tooltip("The " + nameof(ScenarioEventSignal) + " to use, usually a landscape or portrait one.")]
        public ScenarioEventSignal scenarioSignal;

        [Tooltip("The stage " + nameof(GameObject) + ".")]
        public GameObject stageObject;

        [Tooltip("Whether camera parameter override (appeared in scenario signals) should be enabled.")]
        public bool enableOverride = true;

        #region Animated Properties

        [ReadOnlyInInspector]
        public float focalLength = 50;

        [ReadOnlyInInspector]
        public float angleZ;

        [ReadOnlyInInspector]
        public float positionX;

        [ReadOnlyInInspector]
        public float positionY;

        [ReadOnlyInInspector]
        public float positionZ;

        [ReadOnlyInInspector]
        public float targetX;

        [ReadOnlyInInspector]
        public float targetY;

        [ReadOnlyInInspector]
        public float targetZ;

        #endregion

        private float inputFocalLength;

        private float inputAngleZ;

        private float inputPositionX;

        private float inputPositionY;

        private float inputPositionZ;

        private float inputTargetX;

        private float inputTargetY;

        private float inputTargetZ;

        private Vector3 outputPosition;

        private Quaternion outputRotation;

        private void Start() {
            scenarioSignal.EventEmitted += OnSignal;
        }

        private void OnDestroy() {
            scenarioSignal.EventEmitted -= OnSignal;
        }

        private void FixedUpdate() {
            if (!playerControl.isPlaying) {
                return;
            }

            CopyParams();

            if (enableOverride) {
                if (_overrideType != OverrideType.None) {
                    PreprocessOverride();
                }
            }

            var t = targetCamera.gameObject.transform;

            var pos = new Vector3(inputPositionX, inputPositionY, inputPositionZ);
            var tgt = new Vector3(inputTargetX, inputTargetY, inputTargetZ);

            outputPosition = pos;

            var rot0 = SolveLookAtRotation(pos, tgt);
            var rotZ = inputAngleZ;
            outputRotation = Quaternion.Euler(rot0.x, rot0.y, rotZ);

            if (enableOverride) {
                if (_overrideType != OverrideType.None) {
                    PostprocessOverride();
                }
            }

            t.localPosition = outputPosition;
            t.localRotation = outputRotation;
            targetCamera.focalLength = inputFocalLength;
        }

        private void OnSignal(object sender, ScenarioSignalEventArgs e) {
            var ev = e.Data;

            if (ev.type == (int)ScenarioDataType.CameraOverride) {
                Debug.Assert(ev.camCut >= 0);

                if (ev.param == 0) {
                    _overrideType = OverrideType.FaceCloseUp;
                } else if (ev.param == 1) {
                    _overrideType = OverrideType.UpperBodyCloseUp;
                } else if (ev.param == 2) {
                    _overrideType = OverrideType.Unknown1;
                } else {
                    _overrideType = OverrideType.None;
                }

                _currentEvent = ev;
            }
        }

        private void CopyParams() {
            inputFocalLength = focalLength;
            inputAngleZ = angleZ;
            inputPositionX = positionX;
            inputPositionY = positionY;
            inputPositionZ = positionZ;
            inputTargetX = targetX;
            inputTargetY = targetY;
            inputTargetZ = targetZ;
        }

        private void PreprocessOverride() {
            if (_overrideType == OverrideType.None) {
                return;
            }

            var ev = _currentEvent;

            if (ev == null) {
                return;
            }

            if (_overrideType == OverrideType.FaceCloseUp) {
                Debug.Assert(ev.idol > 0);

                var idolObjName = MltdModelAnimator.GetIdolObjectName(ev.idol);
                var idolTransform = stageObject.transform.Find(idolObjName);
                Debug.Assert(idolTransform.IsNotNull());

                const string facePath = "MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/KUBI/ATAMA/" + ModelLoader.CharaHeadObjectName + "/KUBI/ATAMA";
                var faceTransform = idolTransform.Find(facePath);
                Debug.Assert(faceTransform.IsNotNull());

                var tgt = faceTransform.position;
                (inputTargetX, inputTargetY, inputTargetZ) = (tgt.x, tgt.y, tgt.z);

                // var forward = -faceTransform.right;
                //
                // var offset = forward;
                // var pos = tgt + offset;
                // (inputPositionX, inputPositionY, inputPositionZ) = (pos.x, pos.y, pos.z);
            } else if (_overrideType == OverrideType.UpperBodyCloseUp) {
                if (ev.idol > 0) {
                    var idolObjName = MltdModelAnimator.GetIdolObjectName(ev.idol);
                    var idolTransform = stageObject.transform.Find(idolObjName);
                    Debug.Assert(idolTransform.IsNotNull());

                    const string chestPath = "MODEL_00/BODY_SCALE/BASE/MUNE1";
                    var faceTransform = idolTransform.Find(chestPath);
                    Debug.Assert(faceTransform.IsNotNull());

                    var tgt = faceTransform.position;
                    (inputTargetX, inputTargetY, inputTargetZ) = (tgt.x, tgt.y, tgt.z);
                }
            }
        }

        private void PostprocessOverride() {
            // if (_overrideType == OverrideType.None) {
            //     return;
            // }
            //
            // if (_overrideType == OverrideType.CloseUp) {
            //     var inputTarget = new Vector3(inputTargetX, inputTargetY, inputTargetZ);
            //     var inputDelta = new Vector3(inputTargetX - inputPositionX, inputTargetY - inputPositionY, inputTargetZ - inputPositionZ);
            //     var inputRotationBase = SolveLookAtRotation(new Vector3(positionX, positionY, positionZ), new Vector3(targetX, targetY, targetZ));
            //     var outRot = Quaternion.Euler(inputRotationBase) * outputRotation;
            //     var outEuler = outRot.eulerAngles;
            //     outEuler.y = -(180 - outEuler.y);
            //     outputRotation = Quaternion.Euler(outEuler);
            //     var outDiff = outputRotation * Vector3.forward;
            //     outputPosition = inputTarget - inputDelta.magnitude * outDiff;
            // }
        }

        private static Vector3 SolveLookAtRotation(in Vector3 position, in Vector3 target) {
            var lookAt = Matrix4x4.LookAt(position, target, Vector3.up);
            return lookAt.rotation.eulerAngles;
        }

        private EventScenarioData[] _controlFrames;

        private OverrideType _overrideType;

        private enum OverrideType {

            None = 0,

            // A close-up shot
            FaceCloseUp = 1,

            UpperBodyCloseUp = 2,

            Unknown1 = 3,

        }

        private EventScenarioData _currentEvent;

    }
}
