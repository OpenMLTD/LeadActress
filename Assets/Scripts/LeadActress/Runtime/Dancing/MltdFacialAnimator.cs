using System;
using System.Collections.Generic;
using Imas.Live;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace LeadActress.Runtime.Dancing {
    [AddComponentMenu("MLTD/MLTD Facial Animator")]
    public class MltdFacialAnimator : MonoBehaviour {

        [Tooltip("Scenario event signals to listen to")]
        public ScenarioEventSignal[] eventSignals;

        public PlayerControl playerControl;

        public SkinnedMeshRenderer faceRenderer { get; set; }

        public ModelPlacement placement;

        public TextAsset expressionMapAsset;

        private void Awake() {
            _defaultExpressionMap = GetDefaultExpressionMap();
            _currFacialExpression = new float[BlendShapeCount - LipSyncCount];
            _prevFacialExpression = new float[BlendShapeCount - LipSyncCount];
        }

        private void Start() {
            if (placement.formationNumber < MltdSimulationConstants.MinDanceFormation || placement.formationNumber > MltdSimulationConstants.MaxDanceFormation) {
                Debug.LogError($"Invalid formation number: {placement.formationNumber}, should be {MltdSimulationConstants.MinDanceFormation} to {MltdSimulationConstants.MaxDanceFormation}.");
                return;
            }

            foreach (var signal in eventSignals) {
                signal.EventEmitted += OnScenarioEventEmitted;
            }

            {
                var exprMap = JsonConvert.DeserializeObject<ExpressionMap>(expressionMapAsset.text);
                var maps = new Dictionary<int, Dictionary<string, float>>();

                foreach (var expr in exprMap.Expressions) {
                    maps.Add(expr.Key, expr.Table);
                }

                _expressionMaps = maps;
            }
        }

        private void OnDestroy() {
            foreach (var signal in eventSignals) {
                signal.EventEmitted -= OnScenarioEventEmitted;
            }
        }

        private void Update() {
            if (playerControl.isOnPlaying) {
                InitStates();
            } else if (playerControl.isOnStopping) {
                ResetStates();
            }

            if (playerControl.isPlaying && faceRenderer.IsNotNull()) {
                AnimateFace(false);
            }
        }

        private void InitStates() {
            _currentTick = 0;

            _isSinging = true;

            _currLipDeform = LipDeform.Silence();
            _prevLipDeform = LipDeform.Silence();

            Array.Clear(_currFacialExpression, 0, _currFacialExpression.Length);
            Array.Clear(_prevFacialExpression, 0, _prevFacialExpression.Length);
            _currFacialExprKey = 0;
            _prevFacialExprKey = 0;

            _prevEyeClosed = false;
            _currEyeClosed = false;

            Array.Clear(_currBlendShapeValues, 0, _currBlendShapeValues.Length);

            AnimateFace(true);
        }

        private void ResetStates() {
        }

        private void AnimateFace(bool direct) {
            UpdateLipSyncFrame(direct);
            UpdateFacialExpressionFrame(direct);
            ApplyBlendShapeWeights();
        }

        private void UpdateLipSyncFrame(bool direct) {
            if (_isSinging) {
                var currentTime = playerControl.relativeTime;
                LipDeform deform;

                if (direct) {
                    deform = _currLipDeform;
                } else {
                    if (currentTime < _lipSyncStartTime + LipTransitionTime) {
                        var t = (currentTime - _lipSyncStartTime) / LipTransitionTime;
                        deform = LipDeform.Lerp(_prevLipDeform, _currLipDeform, t);
                    } else {
                        deform = _currLipDeform;
                    }
                }

                PutLipFrame(in deform);
            } else {
                PutLipFrame(LipDeform.Silence());
            }
        }

        private void UpdateFacialExpressionFrame(bool direct) {
            var currentTime = playerControl.relativeTime;

            var faceFrame = new float[BlendShapeCount - LipSyncCount];

            {
                if (direct || _currentTick == 0) {
                    Array.Copy(_currFacialExpression, faceFrame, BlendShapeCount - LipSyncCount);
                } else {
                    if (currentTime < _expressionStartTime + FacialExpressionTransitionTime) {
                        var t = (currentTime - _expressionStartTime) / FacialExpressionTransitionTime;

                        for (var i = 0; i < BlendShapeCount - LipSyncCount; i += 1) {
                            faceFrame[i] = Mathf.Lerp(_prevFacialExpression[i], _currFacialExpression[i], t);
                        }
                    } else {
                        Array.Copy(_currFacialExpression, faceFrame, BlendShapeCount - LipSyncCount);
                    }
                }
            }

            {
                const int eyeCloseR = 24;
                const int eyeCloseL = 25;

                EyeCloseRatio ratio;

                if (direct || _currentTick == 0) {
                    ratio = EyeCloseRatio.FromEyeClosed(_currEyeClosed);
                } else {
                    if (currentTime < _eyeCloseStartTime + HalfEyeBlinkTime) {
                        var t = (currentTime - _eyeCloseStartTime) / HalfEyeBlinkTime;
                        ratio = EyeCloseRatio.Lerp(EyeCloseRatio.FromEyeClosed(_prevEyeClosed), EyeCloseRatio.FromEyeClosed(_currEyeClosed), t);
                    } else {
                        ratio = EyeCloseRatio.FromEyeClosed(_currEyeClosed);
                    }
                }

                faceFrame[eyeCloseL - LipSyncCount] = ratio.L;
                faceFrame[eyeCloseR - LipSyncCount] = ratio.R;
            }

            PutFacialFrame(faceFrame);
        }

        private void ApplyBlendShapeWeights() {
            if (faceRenderer.IsNull()) {
                return;
            }

            for (var i = 0; i < BlendShapeCount; i += 1) {
                faceRenderer.SetBlendShapeWeight(i, _currBlendShapeValues[i]);
            }
        }

        private void OnScenarioEventEmitted(object sender, [NotNull] ScenarioSignalEventArgs e) {
            var ev = e.Data;

            _currentTick = ev.tick;

            switch (ev.type) {
                case (int)ScenarioDataType.SingControl: {
                    if (ev.mute.Length >= placement.formationNumber) {
                        _isSinging = ev.mute[placement.formationNumber - 1];
                    }

                    break;
                }
                case (int)ScenarioDataType.LipSync: {
                    if (_isSinging) {
                        var lipCode = (LipCode)ev.param;

                        if (lipCode.IsVoice()) {
                            _prevLipDeform = _currLipDeform;
                            _currLipDeform = LipDeform.FromLipCode(lipCode);
                            _lipSyncStartTime = playerControl.relativeTime;
                        }
                    }

                    break;
                }
                case (int)ScenarioDataType.FacialExpression: {
                    if (ev.idol == placement.formationNumber - 1) {
                        ProcessFacialExpression(ev);
                    }

                    break;
                }
                default:
                    break;
            }
        }

        private void ProcessFacialExpression([NotNull] EventScenarioData ev) {
            var currentTime = playerControl.relativeTime;

            if (ev.param != _currFacialExprKey) {
#if UNITY_EDITOR
                string message;

                if (_expressionMaps.ContainsKey(ev.param)) {
                    message = $"Setting expr {ev.param.ToString()} for idol {placement.formationNumber.ToString()} @ {ev.absTime.ToString()}";
                } else {
                    message = $"Unknown expr {ev.param.ToString()} for idol {placement.formationNumber.ToString()} @ {ev.absTime.ToString()}";
                }

                Debug.Log(message);
#endif

                _prevFacialExprKey = _currFacialExprKey;
                _currFacialExprKey = ev.param;
                _expressionStartTime = currentTime;

                var currMap = FindExpressionOrDefault(_currFacialExprKey);
                var prevMap = FindExpressionOrDefault(_prevFacialExprKey);

                FacialFrameToBuffer(currMap, _currFacialExpression);
                FacialFrameToBuffer(prevMap, _prevFacialExpression);
            }

            // TODO: eye movement collision prevention (e.g. E_wink_* & E_metoji_*)?
            if (ev.eyeclose != _currEyeClosed) {
                if (!IsWinking()) {
                    _prevEyeClosed = _currEyeClosed;
                    _currEyeClosed = ev.eyeclose;
                    _eyeCloseStartTime = currentTime;
                }
            }
        }

        private bool IsWinking() {
            var i1 = Array.IndexOf(_blendShapeNamesFastLookup, "E_wink_l") - LipSyncCount;
            var i2 = Array.IndexOf(_blendShapeNamesFastLookup, "E_wink_r") - LipSyncCount;

            return _currFacialExpression[i1] > 0 || _currFacialExpression[i2] > 0;
        }

        [NotNull]
        private Dictionary<string, float> FindExpressionOrDefault(int key) {
            if (_expressionMaps.TryGetValue(key, out var map)) {
                return map;
            } else {
                return _defaultExpressionMap;
            }
        }

        private void FacialFrameToBuffer([NotNull] Dictionary<string, float> map, [NotNull] float[] buffer) {
            var currentTime = playerControl.relativeTime;

            foreach (var kv in map) {
                var index = Array.IndexOf(_blendShapeNamesFastLookup, kv.Key);

                if (index >= 0) {
                    Debug.Assert(index >= LipSyncCount);

                    float value;

                    do {
                        if (EyesFilteredMorphs.Contains(kv.Key)) {
                            value = 0;
                            break;
                        }

                        if (EyesAffectedMorphs.Contains(kv.Key)) {
                            if (_currEyeClosed) {
                                value = 0;
                                break;
                            } else if (currentTime < _eyeCloseStartTime + HalfEyeBlinkTime) {
                                value = 0;
                                break;
                            }
                        }

                        value = kv.Value * 100;
                    } while (false);

                    buffer[index - LipSyncCount] = value;
                }
            }
        }

        [NotNull]
        private Dictionary<string, float> GetDefaultExpressionMap() {
            var result = new Dictionary<string, float>();

            for (var i = LipSyncCount; i < BlendShapeCount; i += 1) {
                result.Add(_blendShapeNamesFastLookup[i], 0);
            }

            return result;
        }

        private void PutLipFrame(in LipDeform deform) {
            _currBlendShapeValues[0] = deform.a;
            _currBlendShapeValues[1] = deform.i;
            _currBlendShapeValues[2] = deform.u;
            _currBlendShapeValues[3] = deform.e;
            _currBlendShapeValues[4] = deform.o;
            _currBlendShapeValues[5] = deform.n;
        }

        private void PutFacialFrame([NotNull] float[] frame) {
            for (var i = LipSyncCount; i < BlendShapeCount; i += 1) {
                _currBlendShapeValues[i] = frame[i - LipSyncCount];
            }
        }

        private bool _isSinging;

        private long _currentTick;

        private LipDeform _currLipDeform;

        private LipDeform _prevLipDeform;

        private float _lipSyncStartTime;

        private float[] _currFacialExpression;

        private float[] _prevFacialExpression;

        private int _currFacialExprKey;

        private int _prevFacialExprKey;

        private float _expressionStartTime;

        private bool _currEyeClosed;

        private bool _prevEyeClosed;

        private float _eyeCloseStartTime;

        private Dictionary<int, Dictionary<string, float>> _expressionMaps;

        private const int BlendShapeCount = 32;

        private const int LipSyncCount = 6;

        private readonly float[] _currBlendShapeValues = new float[BlendShapeCount];

        private const float LipTransitionTime = 0.1f;

        private const float HalfEyeBlinkTime = 0.128f / 2;

        private const float FacialExpressionTransitionTime = 0.1f;

        [NotNull, ItemNotNull]
        private static readonly HashSet<string> EyesFilteredMorphs = new HashSet<string> {
            "E_metoji_l",
            "E_metoji_r",
        };

        // Used to skip generating animation frame while eyes are closed.
        [NotNull, ItemNotNull]
        private static readonly HashSet<string> EyesAffectedMorphs = new HashSet<string> {
            "E_wink_l",
            "E_wink_r",
            "E_open_l",
            "E_open_r",
        };

        private Dictionary<string, float> _defaultExpressionMap;

        // Usually prefixed with "blendShape1.", but some models differ.
        [NotNull, ItemNotNull]
        private readonly string[] _blendShapeNamesFastLookup = {
            "M_a",
            "M_i",
            "M_u",
            "M_e",
            "M_o",
            "M_n",
            "M_egao",
            "M_shinken",
            "M_wide",
            "M_up",
            "M_n2",
            "M_down",
            "M_odoroki",
            "M_narrow",
            "B_v_r",
            "B_v_l",
            "B_hati_r",
            "B_hati_l",
            "B_agari_r",
            "B_agari_l",
            "B_odoroki_r",
            "B_odoroki_l",
            "B_down",
            "B_yori",
            "E_metoji_r",
            "E_metoji_l",
            "E_wink_r",
            "E_wink_l",
            "E_open_r",
            "E_open_l",
            "EL_wide",
            "EL_up",
        };

        private struct EyeCloseRatio {

            public float L;

            public float R;

            public static EyeCloseRatio FromEyeClosed(bool closed) {
                return closed ? Closed() : Open();
            }

            private static EyeCloseRatio New(float ratio) {
                return new EyeCloseRatio {
                    L = ratio,
                    R = ratio,
                };
            }

            public static EyeCloseRatio Lerp(in EyeCloseRatio v1, in EyeCloseRatio v2, float t) {
                return new EyeCloseRatio {
                    L = Mathf.Lerp(v1.L, v2.L, t),
                    R = Mathf.Lerp(v1.R, v2.R, t),
                };
            }

            public static EyeCloseRatio Closed() {
                return New(100);
            }

            public static EyeCloseRatio Open() {
                return New(0);
            }

        }

    }
}
