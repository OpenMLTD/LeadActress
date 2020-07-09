using System;
using Cysharp.Threading.Tasks;
using Imas;
using JetBrains.Annotations;
using LeadActress.Runtime.Dancing;
using LeadActress.Utilities;
using UnityEngine;

namespace LeadActress.Runtime.Loaders {
    [AddComponentMenu("MLTD/Loaders/Camera Animation Loader")]
    public class CameraAnimationLoader : MonoBehaviour {

        public BatchAssetBundleLoader bundleLoader;

        public CommonResourceProperties commonResourceProperties;

        public async UniTask<AnimationClip> LoadAsync() {
            if (_asyncLoadInfo != null) {
                return await ReturnExistingAsync();
            }

            AsyncLoadInfo<AnimationClip> info = null;

            lock (this) {
                if (_asyncLoadInfo == null) {
                    info = new AsyncLoadInfo<AnimationClip>();
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

            var camAssetPath = $"assets/imas/resources/exclude/imo/camera/{songResourceName}/{camAssetName}_cam.imo.asset";
            var camData = camBundle.LoadAsset<CharacterImasMotionAsset>(camAssetPath);

            var clip = CameraAnimation.CreateFrom(camData, camAssetName);

            info.Success(clip);

            return clip;
        }

        private UniTask<AnimationClip> ReturnExistingAsync() {
            Debug.Assert(_asyncLoadInfo != null);
            var resName = commonResourceProperties.songResourceName;
            return AsyncLoadInfo.ReturnExistingAsync(_asyncLoadInfo, $"Failed to load camera animation for {resName}.");
        }

        [CanBeNull]
        private AsyncLoadInfo<AnimationClip> _asyncLoadInfo;

    }
}
