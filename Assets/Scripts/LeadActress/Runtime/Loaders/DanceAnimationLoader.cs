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

        public int motionNumber {
            get => _motionNumber;
            set => _motionNumber = value;
        }

        public int formationNumber {
            get => _formationNumber;
            set => _formationNumber = value;
        }

        public async UniTask<AnimatorController> LoadAsync() {
            var group = await LoadClipsAsync();
            var controller = CommonAnimationControllerBuilder.BuildAnimationController(group, $"dan_{commonResourceProperties.songResourceName}_{motionNumber:00}#{formationNumber:00}");
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

            var songResourceName = commonResourceProperties.songResourceName;

            if (string.IsNullOrWhiteSpace(songResourceName)) {
                info.Fail();
                throw new FormatException("Song resource name is empty.");
            }

            if (motionNumber < MltdSimulationConstants.MinDanceMotion || motionNumber > MltdSimulationConstants.MaxDanceMotion) {
                info.Fail();
                throw new FormatException($"Invalid motion number: {motionNumber}, should be {MltdSimulationConstants.MinDanceMotion} to {MltdSimulationConstants.MaxDanceMotion}.");
            }

            if (formationNumber < MltdSimulationConstants.MinDanceFormation || motionNumber > MltdSimulationConstants.MaxDanceFormation) {
                info.Fail();
                throw new FormatException($"Invalid formation number: {motionNumber}, should be {MltdSimulationConstants.MinDanceFormation} to {MltdSimulationConstants.MaxDanceFormation}.");
            }

            var danceAssetName = $"dan_{songResourceName}_{motionNumber:00}";

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

                mainDance = DanceAnimation.CreateFrom(motionData, danceAssetName);
            }

            async UniTask<AnimationClip> LoadAppealMotionAsync(string postfix) {
                AnimationClip result;
                var assetPath = $"assets/imas/resources/exclude/imo/dance/{songResourceName}/{danceAssetName}_{postfix}.imo.asset";

                if (mainDanceBundle.Contains(assetPath)) {
                    var motionData = mainDanceBundle.LoadAsset<CharacterImasMotionAsset>(assetPath);
                    result = DanceAnimation.CreateFrom(motionData, $"{danceAssetName}_{postfix}");
                } else {
                    if (appealBundleFound.HasValue) {
                        if (!appealBundleFound.Value) {
                            return null;
                        }
                    } else {
                        bool found;
                        (appealBundle, found) = await TryLoadAppealBundleAsync();
                        appealBundleFound = found;
                    }

                    if (appealBundle != null && appealBundle.Contains(assetPath)) {
                        var motionData = appealBundle.LoadAsset<CharacterImasMotionAsset>(assetPath);
                        result = DanceAnimation.CreateFrom(motionData, $"{danceAssetName}_{postfix}");
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

        private async UniTask<( AssetBundle, bool)> TryLoadAppealBundleAsync() {
            AssetBundle appealBundle;
            bool successful;

            try {
                appealBundle = await bundleLoader.LoadFromRelativePathAsync($"dan_{commonResourceProperties.songResourceName}_ap.imo.unity3d");
                successful = true;
            } catch (FileNotFoundException) {
                appealBundle = null;
                successful = false;
            }

            return (appealBundle, successful);
        }

        private UniTask<AnimationGroup> ReturnExistingAsync() {
            Debug.Assert(_asyncLoadInfo != null);
            var resName = commonResourceProperties.songResourceName;
            return AsyncLoadInfo.ReturnExistingAsync(_asyncLoadInfo, $"Failed to load dance for {resName}.");
        }

        // e.g.: dan_shtstr
        private static readonly Regex DanceAssetNameRegex = new Regex(@"^dan_[a-z0-9]{6}_[0-9]{2}$");

        [Tooltip("Which dance animation does this idol use.")]
        [SerializeField]
        [Range(MltdSimulationConstants.MinDanceMotion, MltdSimulationConstants.MaxDanceMotion)]
        private int _motionNumber = MltdSimulationConstants.MinDanceMotion;

        [Tooltip("Which position does this idol stand.")]
        [SerializeField]
        [Range(MltdSimulationConstants.MinDanceFormation, MltdSimulationConstants.MaxDanceFormation)]
        private int _formationNumber = MltdSimulationConstants.MinDanceFormation;

        [CanBeNull]
        private AsyncLoadInfo<AnimationGroup> _asyncLoadInfo;

    }
}
