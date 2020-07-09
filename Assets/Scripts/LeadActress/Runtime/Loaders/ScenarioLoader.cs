using System;
using Cysharp.Threading.Tasks;
using Imas.Live;
using JetBrains.Annotations;
using LeadActress.Runtime.Dancing;
using LeadActress.Utilities;
using UnityEngine;

namespace LeadActress.Runtime.Loaders {
    [AddComponentMenu("MLTD/Loaders/Scenario Loader")]
    public class ScenarioLoader : MonoBehaviour {

        public BatchAssetBundleLoader bundleLoader;

        public CommonResourceProperties commonResourceProperties;

        public ScenarioVariation variation {
            get => _variation;
            set => _variation = value;
        }

        public async UniTask<ScenarioScrObj> LoadAsync() {
            if (_asyncLoadInfo != null) {
                return await ReturnExistingAsync();
            }

            AsyncLoadInfo<ScenarioScrObj> info = null;

            lock (this) {
                if (_asyncLoadInfo == null) {
                    info = new AsyncLoadInfo<ScenarioScrObj>();
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

            var scenarioBundleName = $"scrobj_{songResourceName}";
            var scenarioBundle = await bundleLoader.LoadFromRelativePathAsync($"{scenarioBundleName}.unity3d");

            var scenarioVar = GetVariationInfixString(variation);
            var scenarioAssetPath = $"assets/imas/resources/scrobj/{songResourceName}/{songResourceName}_scenario{scenarioVar}_sobj.asset";
            var scenarioData = scenarioBundle.LoadAsset<ScenarioScrObj>(scenarioAssetPath);

            info.Success(scenarioData);

            return scenarioData;
        }

        private UniTask<ScenarioScrObj> ReturnExistingAsync() {
            Debug.Assert(_asyncLoadInfo != null);
            var resName = commonResourceProperties.songResourceName;
            return AsyncLoadInfo.ReturnExistingAsync(_asyncLoadInfo, $"Failed to load scenario for {resName}.");
        }

        [NotNull]
        private static string GetVariationInfixString(ScenarioVariation variation) {
            switch (variation) {
                case ScenarioVariation.Main:
                    return string.Empty;
                case ScenarioVariation.Landscape:
                    return "_yoko";
                case ScenarioVariation.Portrait:
                    return "_tate";
                default:
                    throw new ArgumentOutOfRangeException(nameof(variation), variation, null);
            }
        }

        [SerializeField]
        private ScenarioVariation _variation = ScenarioVariation.Main;

        private AsyncLoadInfo<ScenarioScrObj> _asyncLoadInfo;

    }
}
