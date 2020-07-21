using System;
using System.Collections.Generic;
using System.Linq;
using Imas;
using Imas.Live;
using JetBrains.Annotations;
using UnityEngine;

namespace LeadActress.Runtime.Dancing {
    internal static class DanceAnimation {

        public sealed class CreateConfig {

            public CreateConfig() {
                UseFormation = true;
                FormationNumber = 1;
            }

            public bool UseFormation { get; set; }

            public int FormationNumber { get; set; }

            [NotNull]
            public static CreateConfig Default() {
                return new CreateConfig();
            }

        }

        [NotNull]
        public static AnimationClip CreateFrom([NotNull] CharacterImasMotionAsset motion, [CanBeNull] ScenarioScrObj scenario, [NotNull] string name, [NotNull] CreateConfig config) {
            var frameDict = CreateGroupedFramesByPath(motion);
            var clip = CreateClipFromGroupedFrames(frameDict, scenario, name, config);
            return clip;
        }

        [NotNull]
        private static IReadOnlyDictionary<string, List<DanceKeyFrame>> CreateGroupedFramesByPath([NotNull] CharacterImasMotionAsset motion) {
            var frameCount = motion.curves.Max(curve => curve.values.Length);
            var frameDict = new Dictionary<string, List<DanceKeyFrame>>();

            foreach (var curve in motion.curves) {
                var path = curve.path;

                var keyType = curve.GetKeyType();
                var propertyType = curve.GetPropertyType();
                List<DanceKeyFrame> frameList;

                if (frameDict.ContainsKey(path)) {
                    frameList = frameDict[path];
                } else {
                    frameList = new List<DanceKeyFrame>();
                    frameDict.Add(path, frameList);
                }

                switch (keyType) {
                    case KeyType.Const: {
                        switch (propertyType) {
                            case PropertyType.AngleX:
                                for (var frameIndex = 0; frameIndex < frameCount; ++frameIndex) {
                                    GetOrAddFrame(frameList, frameIndex, path).AngleX = curve.values[0];
                                }

                                break;
                            case PropertyType.AngleY:
                                for (var frameIndex = 0; frameIndex < frameCount; ++frameIndex) {
                                    GetOrAddFrame(frameList, frameIndex, path).AngleY = curve.values[0];
                                }

                                break;
                            case PropertyType.AngleZ:
                                for (var frameIndex = 0; frameIndex < frameCount; ++frameIndex) {
                                    GetOrAddFrame(frameList, frameIndex, path).AngleZ = curve.values[0];
                                }

                                break;
                            case PropertyType.PositionX:
                                for (var frameIndex = 0; frameIndex < frameCount; ++frameIndex) {
                                    GetOrAddFrame(frameList, frameIndex, path).PositionX = curve.values[0];
                                }

                                break;
                            case PropertyType.PositionY:
                                for (var frameIndex = 0; frameIndex < frameCount; ++frameIndex) {
                                    GetOrAddFrame(frameList, frameIndex, path).PositionY = curve.values[0];
                                }

                                break;
                            case PropertyType.PositionZ:
                                for (var frameIndex = 0; frameIndex < frameCount; ++frameIndex) {
                                    GetOrAddFrame(frameList, frameIndex, path).PositionZ = curve.values[0];
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, $"Unknown property type: \"{propertyType}\".");
                        }
                    }
                        break;
                    case KeyType.FullFrame: {
                        var valueCount = curve.values.Length;

                        switch (propertyType) {
                            case PropertyType.AngleX:
                                for (var frameIndex = 0; frameIndex < frameCount; ++frameIndex) {
                                    var index = frameIndex < valueCount ? frameIndex : valueCount - 1;
                                    GetOrAddFrame(frameList, frameIndex, path).AngleX = curve.values[index];
                                }

                                break;
                            case PropertyType.AngleY:
                                for (var frameIndex = 0; frameIndex < frameCount; ++frameIndex) {
                                    var index = frameIndex < valueCount ? frameIndex : valueCount - 1;
                                    GetOrAddFrame(frameList, frameIndex, path).AngleY = curve.values[index];
                                }

                                break;
                            case PropertyType.AngleZ:
                                for (var frameIndex = 0; frameIndex < frameCount; ++frameIndex) {
                                    var index = frameIndex < valueCount ? frameIndex : valueCount - 1;
                                    GetOrAddFrame(frameList, frameIndex, path).AngleZ = curve.values[index];
                                }

                                break;
                            case PropertyType.PositionX:
                                for (var frameIndex = 0; frameIndex < frameCount; ++frameIndex) {
                                    var index = frameIndex < valueCount ? frameIndex : valueCount - 1;
                                    GetOrAddFrame(frameList, frameIndex, path).PositionX = curve.values[index];
                                }

                                break;
                            case PropertyType.PositionY:
                                for (var frameIndex = 0; frameIndex < frameCount; ++frameIndex) {
                                    var index = frameIndex < valueCount ? frameIndex : valueCount - 1;
                                    GetOrAddFrame(frameList, frameIndex, path).PositionY = curve.values[index];
                                }

                                break;
                            case PropertyType.PositionZ:
                                for (var frameIndex = 0; frameIndex < frameCount; ++frameIndex) {
                                    var index = frameIndex < valueCount ? frameIndex : valueCount - 1;
                                    GetOrAddFrame(frameList, frameIndex, path).PositionZ = curve.values[index];
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, $"Unknown property type: \"{((int)propertyType).ToString()}\".");
                        }

                        break;
                    }
                    case KeyType.Discrete: {
                        if ((curve.values.Length % 2) != 0) {
                            throw new ApplicationException($"Length of curve values {curve.values.Length.ToString()} is not a multiple of 2.");
                        }

                        var curveValueCount = curve.values.Length / 2;
                        var curTime = curve.values[0];
                        var curValue = curve.values[1];
                        var nextTime = curve.values[2];
                        var nextValue = curve.values[3];
                        var curIndex = 0;

                        switch (propertyType) {
                            case PropertyType.AngleX:
                                for (var frameIndex = 0; frameIndex < frameCount; ++frameIndex) {
                                    var frame = GetOrAddFrame(frameList, frameIndex, path);
                                    var value = InterpolateValue(frame.Time, curve, ref curIndex, curveValueCount, ref curTime, ref curValue, ref nextTime, ref nextValue);
                                    frame.AngleX = value;
                                }

                                break;
                            case PropertyType.AngleY:
                                for (var frameIndex = 0; frameIndex < frameCount; ++frameIndex) {
                                    var frame = GetOrAddFrame(frameList, frameIndex, path);
                                    var value = InterpolateValue(frame.Time, curve, ref curIndex, curveValueCount, ref curTime, ref curValue, ref nextTime, ref nextValue);
                                    frame.AngleY = value;
                                }

                                break;
                            case PropertyType.AngleZ:
                                for (var frameIndex = 0; frameIndex < frameCount; ++frameIndex) {
                                    var frame = GetOrAddFrame(frameList, frameIndex, path);
                                    var value = InterpolateValue(frame.Time, curve, ref curIndex, curveValueCount, ref curTime, ref curValue, ref nextTime, ref nextValue);
                                    frame.AngleZ = value;
                                }

                                break;
                            case PropertyType.PositionX:
                                for (var frameIndex = 0; frameIndex < frameCount; ++frameIndex) {
                                    var frame = GetOrAddFrame(frameList, frameIndex, path);
                                    var value = InterpolateValue(frame.Time, curve, ref curIndex, curveValueCount, ref curTime, ref curValue, ref nextTime, ref nextValue);
                                    frame.PositionX = value;
                                }

                                break;
                            case PropertyType.PositionY:
                                for (var frameIndex = 0; frameIndex < frameCount; ++frameIndex) {
                                    var frame = GetOrAddFrame(frameList, frameIndex, path);
                                    var value = InterpolateValue(frame.Time, curve, ref curIndex, curveValueCount, ref curTime, ref curValue, ref nextTime, ref nextValue);
                                    frame.PositionY = value;
                                }

                                break;
                            case PropertyType.PositionZ:
                                for (var frameIndex = 0; frameIndex < frameCount; ++frameIndex) {
                                    var frame = GetOrAddFrame(frameList, frameIndex, path);
                                    var value = InterpolateValue(frame.Time, curve, ref curIndex, curveValueCount, ref curTime, ref curValue, ref nextTime, ref nextValue);
                                    frame.PositionZ = value;
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, $"Unknown property type: \"{((int)propertyType).ToString()}\".");
                        }

                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(keyType), keyType, $"Unknown key type: \"{((int)keyType).ToString()}\".");
                }
            }

            return frameDict;
        }

        [NotNull]
        private static DanceKeyFrame GetOrAddFrame([NotNull, ItemNotNull] List<DanceKeyFrame> frameList, int index, [NotNull] string path) {
            DanceKeyFrame frame;

            if (frameList.Count > index) {
                frame = frameList[index];
            } else {
                frame = new DanceKeyFrame(index, path);
                frameList.Add(frame);
            }

            return frame;
        }

        private static float InterpolateValue(float frameTime, [NotNull] Curve curve, ref int curIndex, int curveValueCount, ref float curTime, ref float curValue, ref float nextTime, ref float nextValue) {
            if (curIndex >= curveValueCount - 1) {
                return curValue;
            }

            if (frameTime >= nextTime) {
                curTime = nextTime;
                curValue = nextValue;
                ++curIndex;

                if (curIndex < curveValueCount - 1) {
                    nextTime = curve.values[(curIndex + 1) * 2];
                    nextValue = curve.values[(curIndex + 1) * 2 + 1];
                }
            }

            if (curIndex >= curveValueCount - 1) {
                return curValue;
            }

            var duration = nextTime - curTime;
            var delta = frameTime - curTime;
            var p = delta / duration;

            return curValue * (1 - p) + nextValue * p;
        }

        [NotNull]
        private static AnimationClip CreateClipFromGroupedFrames([NotNull] IReadOnlyDictionary<string, List<DanceKeyFrame>> frameDict, [CanBeNull] ScenarioScrObj scenario, [NotNull] string clipName, [NotNull] CreateConfig config) {
            var clip = new AnimationClip();

            var formationList = new TimedList<int, Vector4[]>();

            if (config.UseFormation) {
                Debug.Assert(scenario != null);

                var formationControls = scenario.scenario.Where(c => c.type == (int)ScenarioDataType.FormationChange).ToArray();

                foreach (var ev in formationControls) {
                    if (ev.layer != (int)AppealType.None) {
                        continue;
                    }

                    var frameIndex = (int)Math.Round(ev.absTime * FrameRate.Mltd);
                    var formations = ev.formation;

                    formationList.AddOrUpdate(frameIndex, formations);
                }
            }

            clip.frameRate = FrameRate.Mltd;
            clip.name = clipName;
            clip.legacy = true;

            foreach (var kv in frameDict) {
                var path = kv.Key;
                var frameList = kv.Value;
                var frameListCount = frameList.Count;

                if (frameListCount > 0 && frameList[0].HasPositions) {
                    var posX = new Keyframe[frameListCount];
                    var posY = new Keyframe[frameListCount];
                    var posZ = new Keyframe[frameListCount];

                    for (var i = 0; i < frameListCount; ++i) {
                        var frame = frameList[i];
                        // ReSharper disable once PossibleInvalidOperationException
                        var tx = frame.PositionX.Value;
                        // ReSharper disable once PossibleInvalidOperationException
                        var ty = frame.PositionY.Value;
                        // ReSharper disable once PossibleInvalidOperationException
                        var tz = frame.PositionZ.Value;

                        if (path == "MODEL_00") {
                            formationList.TryGetCurrentValue(i, out var formations);
                            Vector4 idolOffset;

                            if (formations == null || formations.Length < config.FormationNumber) {
                                idolOffset = Vector4.zero;
                            } else {
                                idolOffset = formations[config.FormationNumber - 1];
                            }

                            var worldRotation = Quaternion.AngleAxis(idolOffset.w, Vector3.up);
                            var newOrigin = worldRotation * new Vector3(tx, ty, tz);

                            tx = newOrigin.x + idolOffset.x;
                            ty = newOrigin.y + idolOffset.y;
                            tz = newOrigin.z + idolOffset.z;
                        }

                        posX[i] = new Keyframe(frame.Time, tx);
                        posY[i] = new Keyframe(frame.Time, ty);
                        posZ[i] = new Keyframe(frame.Time, tz);
                    }

                    var curveX = new AnimationCurve(posX);
                    var curveY = new AnimationCurve(posY);
                    var curveZ = new AnimationCurve(posZ);

                    clip.SetCurve(path, typeof(Transform), "localPosition.x", curveX);
                    clip.SetCurve(path, typeof(Transform), "localPosition.y", curveY);
                    clip.SetCurve(path, typeof(Transform), "localPosition.z", curveZ);
                }

                if (frameListCount > 0 && frameList[0].HasRotations) {
                    var rotX = new Keyframe[frameListCount];
                    var rotY = new Keyframe[frameListCount];
                    var rotZ = new Keyframe[frameListCount];
                    var rotW = new Keyframe[frameListCount];

                    for (var i = 0; i < frameListCount; ++i) {
                        var frame = frameList[i];
                        // ReSharper disable once PossibleInvalidOperationException
                        var rx = frame.AngleX.Value;
                        // ReSharper disable once PossibleInvalidOperationException
                        var ry = frame.AngleY.Value;
                        // ReSharper disable once PossibleInvalidOperationException
                        var rz = frame.AngleZ.Value;

                        if (path == "MODEL_00") {
                            formationList.TryGetCurrentValue(i, out var formations);
                            Vector4 idolOffset;

                            if (formations == null || formations.Length < config.FormationNumber) {
                                idolOffset = Vector4.zero;
                            } else {
                                idolOffset = formations[config.FormationNumber - 1];
                            }

                            ry += idolOffset.w;
                        }

                        var q = Quaternion.Euler(rx, ry, rz);
                        rotX[i] = new Keyframe(frame.Time, q.x);
                        rotY[i] = new Keyframe(frame.Time, q.y);
                        rotZ[i] = new Keyframe(frame.Time, q.z);
                        rotW[i] = new Keyframe(frame.Time, q.w);
                    }

                    var curveX = new AnimationCurve(rotX);
                    var curveY = new AnimationCurve(rotY);
                    var curveZ = new AnimationCurve(rotZ);
                    var curveW = new AnimationCurve(rotW);

                    clip.SetCurve(path, typeof(Transform), "localRotation.x", curveX);
                    clip.SetCurve(path, typeof(Transform), "localRotation.y", curveY);
                    clip.SetCurve(path, typeof(Transform), "localRotation.z", curveZ);
                    clip.SetCurve(path, typeof(Transform), "localRotation.w", curveW);
                }
            }

            return clip;
        }

    }
}
