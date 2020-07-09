using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Imas;
using JetBrains.Annotations;
using UnityEngine;

namespace LeadActress.Runtime.Dancing {
    public static class CameraAnimation {

        [NotNull]
        public static AnimationClip CreateFrom([NotNull] CharacterImasMotionAsset motion, [NotNull] string name) {
            // var frames = ComputeKeyFrames(motion);
            // var clip = CreateClipFromFrames(frames, name);
            //
            // return clip;
            return CreateClipDirect(motion, name);
        }

        private static AnimationClip CreateClipDirect([NotNull] CharacterImasMotionAsset motion, [NotNull] string name) {
            var focalLengthCurve = motion.curves.FirstOrDefault(curve => curve.path == "CamBase" && curve.GetPropertyName() == "focalLength");
            var camCutCurve = motion.curves.FirstOrDefault(curve => curve.path == "CamBase" && curve.GetPropertyName() == "camCut");
            var angleXCurve = motion.curves.FirstOrDefault(curve => curve.path == "CamBaseS" && curve.GetPropertyType() == PropertyType.AngleX);
            var angleYCurve = motion.curves.FirstOrDefault(curve => curve.path == "CamBaseS" && curve.GetPropertyType() == PropertyType.AngleY);
            var angleZCurve = motion.curves.FirstOrDefault(curve => curve.path == "CamBaseS" && curve.GetPropertyType() == PropertyType.AngleZ);
            var posXCurve = motion.curves.FirstOrDefault(curve => curve.path == "CamBaseS" && curve.GetPropertyType() == PropertyType.PositionX);
            var posYCurve = motion.curves.FirstOrDefault(curve => curve.path == "CamBaseS" && curve.GetPropertyType() == PropertyType.PositionY);
            var posZCurve = motion.curves.FirstOrDefault(curve => curve.path == "CamBaseS" && curve.GetPropertyType() == PropertyType.PositionZ);
            var targetXCurve = motion.curves.FirstOrDefault(curve => curve.path == "CamTgtS" && curve.GetPropertyType() == PropertyType.PositionX);
            var targetYCurve = motion.curves.FirstOrDefault(curve => curve.path == "CamTgtS" && curve.GetPropertyType() == PropertyType.PositionY);
            var targetZCurve = motion.curves.FirstOrDefault(curve => curve.path == "CamTgtS" && curve.GetPropertyType() == PropertyType.PositionZ);

            var focalLength = CreateCurve(focalLengthCurve);
            var angleZ = CreateCurve(angleZCurve);
            var posX = CreateCurve(posXCurve);
            var posY = CreateCurve(posYCurve);
            var posZ = CreateCurve(posZCurve);
            var targetX = CreateCurve(targetXCurve);
            var targetY = CreateCurve(targetYCurve);
            var targetZ = CreateCurve(targetZCurve);

            var clip = new AnimationClip();
            clip.frameRate = FrameRate.Mltd;
            clip.legacy = true;
            clip.name = name;

            clip.SetCurve(string.Empty, typeof(IndirectCamera), "focalLength", focalLength);
            clip.SetCurve(string.Empty, typeof(IndirectCamera), "angleZ", angleZ);
            clip.SetCurve(string.Empty, typeof(IndirectCamera), "positionX", posX);
            clip.SetCurve(string.Empty, typeof(IndirectCamera), "positionY", posY);
            clip.SetCurve(string.Empty, typeof(IndirectCamera), "positionZ", posZ);
            clip.SetCurve(string.Empty, typeof(IndirectCamera), "targetX", targetX);
            clip.SetCurve(string.Empty, typeof(IndirectCamera), "targetY", targetY);
            clip.SetCurve(string.Empty, typeof(IndirectCamera), "targetZ", targetZ);

            return clip;
        }

        [NotNull]
        private static AnimationCurve CreateCurve([NotNull] Curve curve) {
            Keyframe[] frames;

            switch (curve.GetKeyType()) {
                case KeyType.Const: {
                    frames = new[] {
                        new Keyframe(0, curve.values[0]),
                    };
                    break;
                }
                case KeyType.Discrete: {
                    throw new NotImplementedException();
                }
                case KeyType.FullFrame: {
                    var valueCount = curve.values.Length;
                    frames = new Keyframe[valueCount];

                    for (var i = 0; i < valueCount; ++i) {
                        frames[i] = new Keyframe(i / (float)FrameRate.Mltd, curve.values[i]);
                    }

                    break;
                }
                case KeyType.FCurve: {
                    Debug.Assert(curve.values.Length % 4 == 0);
                    var valueCount = curve.values.Length / 4;

                    frames = new Keyframe[valueCount];

                    for (var i = 0; i < valueCount; i += 1) {
                        frames[i] = new Keyframe(curve.values[4 * i], curve.values[4 * i + 1], curve.values[4 * i + 2], curve.values[4 * i + 3]);
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var result = new AnimationCurve(frames);
            return result;
        }

        [NotNull, ItemNotNull]
        private static CameraKeyFrame[] ComputeKeyFrames([NotNull] CharacterImasMotionAsset motion) {
            var focalLengthCurve = motion.curves.FirstOrDefault(curve => curve.path == "CamBase" && curve.GetPropertyName() == "focalLength");
            var camCutCurve = motion.curves.FirstOrDefault(curve => curve.path == "CamBase" && curve.GetPropertyName() == "camCut");
            var angleXCurve = motion.curves.FirstOrDefault(curve => curve.path == "CamBaseS" && curve.GetPropertyType() == PropertyType.AngleX);
            var angleYCurve = motion.curves.FirstOrDefault(curve => curve.path == "CamBaseS" && curve.GetPropertyType() == PropertyType.AngleY);
            var angleZCurve = motion.curves.FirstOrDefault(curve => curve.path == "CamBaseS" && curve.GetPropertyType() == PropertyType.AngleZ);
            var posXCurve = motion.curves.FirstOrDefault(curve => curve.path == "CamBaseS" && curve.GetPropertyType() == PropertyType.PositionX);
            var posYCurve = motion.curves.FirstOrDefault(curve => curve.path == "CamBaseS" && curve.GetPropertyType() == PropertyType.PositionY);
            var posZCurve = motion.curves.FirstOrDefault(curve => curve.path == "CamBaseS" && curve.GetPropertyType() == PropertyType.PositionZ);
            var targetXCurve = motion.curves.FirstOrDefault(curve => curve.path == "CamTgtS" && curve.GetPropertyType() == PropertyType.PositionX);
            var targetYCurve = motion.curves.FirstOrDefault(curve => curve.path == "CamTgtS" && curve.GetPropertyType() == PropertyType.PositionY);
            var targetZCurve = motion.curves.FirstOrDefault(curve => curve.path == "CamTgtS" && curve.GetPropertyType() == PropertyType.PositionZ);

            var allCameraCurves = new[] {
                focalLengthCurve, camCutCurve, angleXCurve, angleYCurve, angleZCurve, posXCurve, posYCurve, posZCurve, targetXCurve, targetYCurve, targetZCurve
            };

            if (AnyoneIsNull(allCameraCurves)) {
                throw new ApplicationException("Invalid camera motion file.");
            }

            Debug.Assert(focalLengthCurve != null, nameof(focalLengthCurve) + " != null");
            Debug.Assert(camCutCurve != null, nameof(camCutCurve) + " != null");
            Debug.Assert(angleXCurve != null, nameof(angleXCurve) + " != null");
            Debug.Assert(angleYCurve != null, nameof(angleYCurve) + " != null");
            Debug.Assert(angleZCurve != null, nameof(angleZCurve) + " != null");
            Debug.Assert(posXCurve != null, nameof(posXCurve) + " != null");
            Debug.Assert(posYCurve != null, nameof(posYCurve) + " != null");
            Debug.Assert(posZCurve != null, nameof(posZCurve) + " != null");
            Debug.Assert(targetXCurve != null, nameof(targetXCurve) + " != null");
            Debug.Assert(targetYCurve != null, nameof(targetYCurve) + " != null");
            Debug.Assert(targetZCurve != null, nameof(targetZCurve) + " != null");

            if (!AllUseFCurve(allCameraCurves)) {
                throw new ApplicationException("Invalid key type.");
            }

            const float frameDuration = 1.0f / FrameRate.Mltd;
            var totalDuration = GetMaxDuration(allCameraCurves);
            var frameCount = (int)Math.Round(totalDuration / frameDuration);

            var cameraFrames = new CameraKeyFrame[frameCount];

            for (var i = 0; i < frameCount; ++i) {
                var frame = new CameraKeyFrame(i);

                frame.FocalLength = GetInterpolatedValue(focalLengthCurve, frame.Time);
                frame.Cut = (int)GetLowerClampedValue(camCutCurve, frame.Time);
                frame.AngleX = GetInterpolatedValue(angleXCurve, frame.Time);
                frame.AngleY = GetInterpolatedValue(angleYCurve, frame.Time);
                frame.AngleZ = GetInterpolatedValue(angleZCurve, frame.Time);
                frame.PositionX = GetInterpolatedValue(posXCurve, frame.Time);
                frame.PositionY = GetInterpolatedValue(posYCurve, frame.Time);
                frame.PositionZ = GetInterpolatedValue(posZCurve, frame.Time);
                frame.TargetX = GetInterpolatedValue(targetXCurve, frame.Time);
                frame.TargetY = GetInterpolatedValue(targetYCurve, frame.Time);
                frame.TargetZ = GetInterpolatedValue(targetZCurve, frame.Time);

                cameraFrames[i] = frame;
            }

            return cameraFrames;
        }

        [NotNull]
        private static AnimationClip CreateClipFromFrames([NotNull, ItemNotNull] CameraKeyFrame[] frames, [NotNull] string clipName) {
            var clip = new AnimationClip();

            clip.frameRate = FrameRate.Mltd;
            clip.name = clipName;
            clip.legacy = true;

            var focalLengthKeyFrames = new Keyframe[frames.Length];
            var posXKeyFrames = new Keyframe[frames.Length];
            var posYKeyFrames = new Keyframe[frames.Length];
            var posZKeyFrames = new Keyframe[frames.Length];
            var rotXKeyFrames = new Keyframe[frames.Length];
            var rotYKeyFrames = new Keyframe[frames.Length];
            var rotZKeyFrames = new Keyframe[frames.Length];
            var rotWKeyFrames = new Keyframe[frames.Length];

            for (var i = 0; i < frames.Length; i += 1) {
                var frame = frames[i];

                focalLengthKeyFrames[i] = new Keyframe(frame.Time, frame.FocalLength);
                posXKeyFrames[i] = new Keyframe(frame.Time, frame.PositionX);
                posYKeyFrames[i] = new Keyframe(frame.Time, frame.PositionY);
                posZKeyFrames[i] = new Keyframe(frame.Time, frame.PositionZ);

                var pos = new Vector3(frame.PositionX, frame.PositionY, frame.PositionZ);
                var tgt = new Vector3(frame.TargetX, frame.TargetY, frame.TargetZ);

                var lookAt = Matrix4x4.LookAt(pos, tgt, Vector3.up);
                var rot0 = lookAt.rotation.eulerAngles;
                var rotZ = frame.AngleZ;
                var q = Quaternion.Euler(rot0.x, rot0.y, rotZ);

                rotXKeyFrames[i] = new Keyframe(frame.Time, q.x);
                rotYKeyFrames[i] = new Keyframe(frame.Time, q.y);
                rotZKeyFrames[i] = new Keyframe(frame.Time, q.z);
                rotWKeyFrames[i] = new Keyframe(frame.Time, q.w);
            }

            var focalLengthCurve = new AnimationCurve(focalLengthKeyFrames);
            var posXCurve = new AnimationCurve(posXKeyFrames);
            var posYCurve = new AnimationCurve(posYKeyFrames);
            var posZCurve = new AnimationCurve(posZKeyFrames);
            var rotXCurve = new AnimationCurve(rotXKeyFrames);
            var rotYCurve = new AnimationCurve(rotYKeyFrames);
            var rotZCurve = new AnimationCurve(rotZKeyFrames);
            var rotWCurve = new AnimationCurve(rotWKeyFrames);

            clip.SetCurve(string.Empty, typeof(Camera), "m_FocalLength", focalLengthCurve);
            clip.SetCurve(string.Empty, typeof(Transform), "localPosition.x", posXCurve);
            clip.SetCurve(string.Empty, typeof(Transform), "localPosition.y", posYCurve);
            clip.SetCurve(string.Empty, typeof(Transform), "localPosition.z", posZCurve);
            clip.SetCurve(string.Empty, typeof(Transform), "localRotation.x", rotXCurve);
            clip.SetCurve(string.Empty, typeof(Transform), "localRotation.y", rotYCurve);
            clip.SetCurve(string.Empty, typeof(Transform), "localRotation.z", rotZCurve);
            clip.SetCurve(string.Empty, typeof(Transform), "localRotation.w", rotWCurve);

            return clip;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AnyoneIsNull<T>([NotNull, ItemCanBeNull] params T[] objects) {
            return objects.Any(x => ReferenceEquals(x, null));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AllUseFCurve([NotNull, ItemNotNull] params Curve[] curves) {
            return curves.All(curve => curve.GetKeyType() == KeyType.FCurve);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetMaxDuration([NotNull, ItemNotNull] params Curve[] curves) {
            float duration = 0;

            foreach (var curve in curves) {
                Debug.Assert(curve.values.Length % 4 == 0);

                var frameCount = curve.values.Length / 4;

                for (var i = 0; i < frameCount; ++i) {
                    var time = curve.values[i * 4];

                    if (time > duration) {
                        duration = time;
                    }
                }
            }

            return duration;
        }

        private static float GetInterpolatedValue([NotNull] Curve curve, float time) {
            var valueCount = curve.values.Length;

            Debug.Assert(valueCount % 4 == 0);

            valueCount = valueCount / 4;

            for (var i = 0; i < valueCount; ++i) {
                if (i < valueCount - 1) {
                    var nextTime = curve.values[(i + 1) * 4];

                    if (time > nextTime) {
                        continue;
                    }

                    var curTime = curve.values[i * 4];
                    var curValue = curve.values[i * 4 + 1];
                    var nextValue = curve.values[(i + 1) * 4 + 1];
                    var tan1 = curve.values[i * 4 + 3];
                    var tan2 = curve.values[(i + 1) * 4 + 2];

                    // suspect:
                    // +2: tan(in)
                    // +3: tan(out)

                    var dt = nextTime - curTime;
                    var t = (time - curTime) / dt;

                    // TODO: use Unity's FCurve support in Keyframe instead
                    return ComputeFCurveNaive(curValue, nextValue, tan1, tan2, dt, t);
                } else {
                    return curve.values[i * 4 + 1];
                }
            }

            throw new ArgumentException("Maybe time is invalid.");
        }

        private static float GetLowerClampedValue([NotNull] Curve curve, float time) {
            var valueCount = curve.values.Length;

            Debug.Assert(valueCount % 4 == 0);

            valueCount = valueCount / 4;

            for (var i = 0; i < valueCount; ++i) {
                if (i < valueCount - 1) {
                    var nextTime = curve.values[(i + 1) * 4];

                    if (time > nextTime) {
                        continue;
                    }

                    var curValue = curve.values[i * 4 + 1];

                    return curValue;
                } else {
                    return curve.values[i * 4 + 1];
                }
            }

            throw new ArgumentException("Maybe time is invalid.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Lerp(float from, float to, float t) {
            return from * (1 - t) + to * t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ComputeFCurveNaive(float value1, float value2, float tan1, float tan2, float dt, float t) {
            bool isInf1 = float.IsInfinity(tan1), isInf2 = float.IsInfinity(tan2);
            float factor = dt;

            if (isInf1) {
                if (isInf2) {
                    return Lerp(value1, value2, t);
                } else {
                    var cp = value2 - tan2 / 3 * factor;
                    return Bezier(value1, cp, value2, t);
                }
            } else {
                if (isInf2) {
                    var cp = value1 + tan1 / 3 * factor;
                    return Bezier(value1, cp, value2, t);
                } else {
                    var cp1 = value1 + tan1 / 3 * factor;
                    var cp2 = value2 - tan2 / 3 * factor;
                    return Bezier(value1, cp1, cp2, value2, t);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Bezier(float p1, float cp, float p2, float t) {
            return (1 - t) * (1 - t) * p1 + 2 * t * (1 - t) * cp + t * t * p2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Bezier(float p1, float cp1, float cp2, float p2, float t) {
            var tt = 1 - t;
            var tt2 = tt * tt;
            var t2 = t * t;

            return tt * tt2 * p1 + 3 * tt2 * t * cp1 + 3 * tt * t2 * cp2 + t * t2 * p2;
        }

    }
}
