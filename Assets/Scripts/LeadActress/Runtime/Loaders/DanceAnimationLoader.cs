using System;
using System.IO;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Imas;
using Imas.Live;
using JetBrains.Annotations;
using LeadActress.Runtime.Dancing;
using LeadActress.Utilities;
using UnityEditor.Animations;
using UnityEngine;

namespace LeadActress.Runtime.Loaders {
    [AddComponentMenu("MLTD/Loaders/Dance Animation Loader")]
    public class DanceAnimationLoader : MonoBehaviour {

        public BatchAssetBundleLoader bundleLoader;

        public CommonResourceProperties commonResourceProperties;

        public ModelPlacement placement;

        public async UniTask<AnimatorController> LoadAsync() {
            var group = await LoadClipsAsync();
            var controller = CommonAnimationControllerBuilder.BuildAnimationController(group, $"dan_{commonResourceProperties.danceResourceName}_{placement.motionNumber:00}#{placement.formationNumber:00}");
            return controller;
        }

        private async UniTask<AnimationGroup> LoadClipsAsync() {
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

            var songResourceName = commonResourceProperties.danceResourceName;

            if (string.IsNullOrWhiteSpace(songResourceName)) {
                info.Fail();
                throw new FormatException("Song resource name is empty.");
            }

            if (placement.motionNumber < MltdSimulationConstants.MinDanceMotion || placement.motionNumber > MltdSimulationConstants.MaxDanceMotion) {
                info.Fail();
                throw new FormatException($"Invalid motion number: {placement.motionNumber}, should be {MltdSimulationConstants.MinDanceMotion} to {MltdSimulationConstants.MaxDanceMotion}.");
            }

            if (placement.formationNumber < MltdSimulationConstants.MinDanceFormation || placement.motionNumber > MltdSimulationConstants.MaxDanceFormation) {
                info.Fail();
                throw new FormatException($"Invalid formation number: {placement.motionNumber}, should be {MltdSimulationConstants.MinDanceFormation} to {MltdSimulationConstants.MaxDanceFormation}.");
            }

            var danceAssetName = $"dan_{songResourceName}_{placement.motionNumber:00}";

            if (!DanceAssetNameRegex.IsMatch(danceAssetName)) {
                info.Fail();
                throw new FormatException($"\"{danceAssetName}\" is not a valid dance asset name.");
            }

            var mainDanceBundle = await bundleLoader.LoadFromRelativePathAsync($"{danceAssetName}.imo.unity3d");

            AssetBundle appealBundle = null;
            bool? appealBundleFound = null;

            AnimationClip mainDance;

            {
                var assetPath = $"assets/imas/resources/exclude/imo/dance/{songResourceName}/{danceAssetName}_dan.imo.asset";
                var motionData = mainDanceBundle.LoadAsset<CharacterImasMotionAsset>(assetPath);

                mainDance = await DanceAnimation.CreateAsync(motionData, danceAssetName);
            }

            async UniTask<AnimationClip> LoadAppealMotionAsync(string postfix) {
                var appealAssetName = $"dan_{songResourceName}_{placement.formationNumber:00}";

                AnimationClip result;
                var assetPath = $"assets/imas/resources/exclude/imo/dance/{songResourceName}/{appealAssetName}_{postfix}.imo.asset";

                if (mainDanceBundle.Contains(assetPath)) {
                    var motionData = mainDanceBundle.LoadAsset<CharacterImasMotionAsset>(assetPath);
                    result = await DanceAnimation.CreateAsync(motionData, $"{appealAssetName}_{postfix}");
                } else {
                    if (appealBundleFound.HasValue) {
                        if (!appealBundleFound.Value) {
                            return null;
                        }
                    } else {
                        bool found;
                        (appealBundle, found) = await TryLoadExternalAppealBundleAsync();
                        appealBundleFound = found;
                    }

                    if (appealBundle != null && appealBundle.Contains(assetPath)) {
                        var motionData = appealBundle.LoadAsset<CharacterImasMotionAsset>(assetPath);
                        result = await DanceAnimation.CreateAsync(motionData, $"{appealAssetName}_{postfix}");
                    } else {
                        result = null;
                    }
                }

                return result;
            }

            var specialAppeal = await LoadAppealMotionAsync("apg");
            var anotherAppeal = await LoadAppealMotionAsync("apa");
            var gorgeousAppeal = await LoadAppealMotionAsync("bpg");

            var animationGroup = new AnimationGroup(mainDance, specialAppeal, anotherAppeal, gorgeousAppeal);

            info.Success(animationGroup);

            return animationGroup;
        }

        private async UniTask<( AssetBundle, bool)> TryLoadExternalAppealBundleAsync() {
            AssetBundle appealBundle;
            bool successful;

            try {
                appealBundle = await bundleLoader.LoadFromRelativePathAsync($"dan_{commonResourceProperties.appealResourceName}_ap.imo.unity3d");
                successful = true;
            } catch (FileNotFoundException) {
                appealBundle = null;
                successful = false;
            }

            return (appealBundle, successful);
        }

        private UniTask<AnimationGroup> ReturnExistingAsync() {
            Debug.Assert(_asyncLoadInfo != null);
            var resName = commonResourceProperties.danceResourceName;
            return AsyncLoadInfo.ReturnExistingAsync(_asyncLoadInfo, $"Failed to load dance for {resName}.");
        }

        // e.g.: dan_shtstr
        private static readonly Regex DanceAssetNameRegex = new Regex(@"^dan_[a-z0-9]{5}[a-z0-9+]_[0-9]{2}$");

        [CanBeNull]
        private AsyncLoadInfo<AnimationGroup> _asyncLoadInfo;

    }
}
