using System;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Imas;
using Imas.Live;
using JetBrains.Annotations;
using LeadActress.Runtime.Dancing;
using LeadActress.Utilities;
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

        public async UniTask<AnimationClip> LoadAsync([NotNull] ScenarioScrObj scenarioData) {
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

            var danceBundle = await bundleLoader.LoadFromRelativePathAsync($"{danceAssetName}.imo.unity3d");

            var danceAssetPath = $"assets/imas/resources/exclude/imo/dance/{songResourceName}/{danceAssetName}_dan.imo.asset";
            var danceData = danceBundle.LoadAsset<CharacterImasMotionAsset>(danceAssetPath);

            // OK, create the animation
            var cfg = DanceAnimation.CreateConfig.Default();
            cfg.FormationNumber = formationNumber;
            var clip = DanceAnimation.CreateFrom(danceData, scenarioData, danceAssetName, cfg);

            info.Success(clip);

            return clip;
        }

        private UniTask<AnimationClip> ReturnExistingAsync() {
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
        private AsyncLoadInfo<AnimationClip> _asyncLoadInfo;

    }
}
