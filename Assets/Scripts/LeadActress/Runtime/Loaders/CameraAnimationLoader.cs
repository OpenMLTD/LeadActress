using System;
using Cysharp.Threading.Tasks;
using Imas;
using JetBrains.Annotations;
using LeadActress.Runtime.Dancing;
using LeadActress.Utilities;
using UnityEditor.Animations;
using UnityEngine;

namespace LeadActress.Runtime.Loaders {
    [AddComponentMenu("MLTD/Loaders/Camera Animation Loader")]
    public class CameraAnimationLoader : MonoBehaviour {

        public BatchAssetBundleLoader bundleLoader;

        public CommonResourceProperties commonResourceProperties;

        public async UniTask<AnimatorController> LoadAsync(CameraControlMode cameraControlMode) {
            var group = await LoadClipsAsync(cameraControlMode);
            var controller = CommonAnimationControllerBuilder.BuildAnimationController(group, $"cam_{commonResourceProperties.songResourceName}");
            return controller;
        }

        private async UniTask<AnimationGroup> LoadClipsAsync(CameraControlMode cameraControlMode) {
            if (_asyncLoadInfo != null) {
                return await ReturnExistingAsync();
            }

            AsyncLoadInfo<AnimationGroup> info = null;

            lock (this) {
                if (_asyncLoadInfo == null) {
                    info = new AsyncLoadInfo<AnimationGroup>();
                    _asyncLoadInfo = info;
                }
            }

            if (info == null) {
                return await ReturnExistingAsync();
            }

            var songResourceName = commonResourceProperties.songResourceName;

            if (string.IsNullOrWhiteSpace(songResourceName)) {
                info.Fail();
                throw new FormatException("Song resource name is empty.");
            }

            var camAssetName = $"cam_{songResourceName}";

            var camBundle = await bundleLoader.LoadFromRelativePathAsync($"{camAssetName}.imo.unity3d");

            AnimationClip mainCamera;

            {
                var camAssetPath = $"assets/imas/resources/exclude/imo/camera/{songResourceName}/{camAssetName}_cam.imo.asset";
                var mainCamData = camBundle.LoadAsset<CharacterImasMotionAsset>(camAssetPath);

                switch (cameraControlMode) {
                    case CameraControlMode.Direct:
                        mainCamera = CameraAnimation.CreateClipForCamera(mainCamData, camAssetName);
                        break;
                    case CameraControlMode.Indirect:
                        mainCamera = CameraAnimation.CreateIndirectClip(mainCamData, camAssetName);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(cameraControlMode), cameraControlMode, null);
                }
            }

            AnimationClip LoadAppealMotion(string postfix) {
                // For portrait variations: ".../{camAssetName}_tate_{postfix}.imo.asset"
                var assetPath = $"assets/imas/resources/exclude/imo/camera/{songResourceName}/{camAssetName}_{postfix}.imo.asset";

                if (!camBundle.Contains(assetPath)) {
                    return null;
                }

                var motionData = camBundle.LoadAsset<CharacterImasMotionAsset>(assetPath);
                AnimationClip result;

                switch (cameraControlMode) {
                    case CameraControlMode.Direct:
                        result = CameraAnimation.CreateClipForCamera(motionData, $"{camAssetName}_{postfix}");
                        break;
                    case CameraControlMode.Indirect:
                        result = CameraAnimation.CreateIndirectClip(motionData, $"{camAssetName}_{postfix}");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(cameraControlMode), cameraControlMode, null);
                }

                return result;
            }

            var specialAppeal = LoadAppealMotion("apg");
            var anotherAppeal = LoadAppealMotion("apa");
            var gorgeousAppeal = LoadAppealMotion("bpg");

            var group = new AnimationGroup(mainCamera, specialAppeal, anotherAppeal, gorgeousAppeal);

            info.Success(group);

            return group;
        }

        private UniTask<AnimationGroup> ReturnExistingAsync() {
            Debug.Assert(_asyncLoadInfo != null);
            var resName = commonResourceProperties.songResourceName;
            return AsyncLoadInfo.ReturnExistingAsync(_asyncLoadInfo, $"Failed to load camera animation for {resName}.");
        }

        [CanBeNull]
        private AsyncLoadInfo<AnimationGroup> _asyncLoadInfo;

    }
}
