using System;
using System.IO;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using LeadActress.Runtime.Dancing;
using LeadActress.Runtime.Sway;
using LeadActress.Utilities;
using UnityEngine;

namespace LeadActress.Runtime.Loaders {
    [AddComponentMenu("MLTD/Loaders/Model Loader")]
    public class ModelLoader : MonoBehaviour {

        public BatchAssetBundleLoader bundleLoader;

        [Tooltip("The stage root object that idol instances are instantiated to.")]
        public GameObject stageObject;

        public ModelFixer modelFixer;

        public ModelPhysicsImporter physicsImporter;

        public string bodyCostumeName {
            get => _bodyCostumeName;
            set => _bodyCostumeName = value;
        }

        public string bodyCostumeVariation {
            get => _bodyCostumeVariation;
            set => _bodyCostumeVariation = value;
        }

        public string headCostumeName {
            get => _headCostumeName;
            set => _headCostumeName = value;
        }

        public string headCostumeVariation {
            get => _headCostumeVariation;
            set => _headCostumeVariation = value;
        }

        public async UniTask<ModelLoadResult> LoadAndInstantiateAsync() {
            if (_asyncLoadInfo != null) {
                return await ReturnExistingAsync();
            }

            AsyncLoadInfo<ModelLoadResult> info = null;

            lock (this) {
                if (_asyncLoadInfo == null) {
                    info = new AsyncLoadInfo<ModelLoadResult>();
                    _asyncLoadInfo = info;
                }
            }

            if (info == null) {
                return await ReturnExistingAsync();
            }

            if (string.IsNullOrWhiteSpace(headCostumeName)) {
                info.Fail();
                throw new FormatException("Head costume name must not be empty.");
            }

            if (string.IsNullOrWhiteSpace(headCostumeVariation)) {
                info.Fail();
                throw new FormatException("Head costume variation must not empty.");
            }

            if (string.IsNullOrWhiteSpace(bodyCostumeName)) {
                info.Fail();
                throw new FormatException("Body costume name must not be empty.");
            }

            if (string.IsNullOrWhiteSpace(bodyCostumeVariation)) {
                info.Fail();
                throw new FormatException("Body costume variation must not empty.");
            }

            var bodyModelAssetName = $"cb_{bodyCostumeName}_{bodyCostumeVariation}";
            var headModelAssetName = $"ch_{headCostumeName}_{headCostumeVariation}";

            if (!HeadAssetNameRegex.IsMatch(headModelAssetName)) {
                info.Fail();
                throw new FormatException($"\"{headModelAssetName}\" is not a valid head model asset name.");
            }

            if (!BodyAssetNameRegex.IsMatch(bodyModelAssetName)) {
                info.Fail();
                throw new FormatException($"\"{bodyModelAssetName}\" is not a valid body model asset name.");
            }

            var bodyModelBundle = await bundleLoader.LoadFromRelativePathAsync($"{bodyModelAssetName}.unity3d");
            var headModelBundle = await bundleLoader.LoadFromRelativePathAsync($"{headModelAssetName}.unity3d");

            GameObject bodyInstance;
            SwayController bodySway;

            {
                string assetGroup;
                if (IdolSerialRegex.IsMatch(bodyCostumeVariation)) {
                    assetGroup = bodyModelAssetName;
                } else {
                    assetGroup = $"cb_{bodyCostumeName}_a";
                }

                var modelPath = $"assets/imas/resources/chara/body/{bodyCostumeName}/{assetGroup}/{bodyModelAssetName}.fbx";
                var model = bodyModelBundle.LoadAsset<GameObject>(modelPath);

                bodyInstance = Instantiate(model, stageObject.transform);

                modelFixer.FixAllShaders(model, bodyInstance);

                var swayPath = $"assets/imas/resources/chara/body/{bodyCostumeName}/{assetGroup}/{bodyModelAssetName}_sway.txt";
                var swayFile = bodyModelBundle.LoadAsset<TextAsset>(swayPath);
                bodySway = SwayAsset.Parse(swayFile.text);
            }

            GameObject headInstance;
            SwayController headSway;

            var bodyAtama = bodyInstance.transform.Find("MODEL_00/BASE/MUNE1/MUNE2/KUBI/ATAMA");

            {
                string assetGroup;
                if (IdolSerialRegex.IsMatch(headCostumeVariation)) {
                    assetGroup = headModelAssetName;
                } else {
                    assetGroup = $"ch_{headCostumeName}_a";
                }

                var modelPath = $"assets/imas/resources/chara/head/{headCostumeName}/{assetGroup}/{headModelAssetName}.fbx";
                var model = headModelBundle.LoadAsset<GameObject>(modelPath);

                headInstance = Instantiate(model, bodyAtama, true);
                headInstance.name = CharaHeadObjectName;

                modelFixer.FixAllShaders(model, headInstance);

                var swayPath = $"assets/imas/resources/chara/head/{headCostumeName}/{assetGroup}/{headModelAssetName}_sway.txt";
                var swayFile = headModelBundle.LoadAsset<TextAsset>(swayPath);
                headSway = SwayAsset.Parse(swayFile.text);
            }

            bodyInstance.transform.position = Vector3.zero;
            bodyInstance.transform.rotation = Quaternion.identity;

            headInstance.transform.position = Vector3.zero;
            headInstance.transform.rotation = Quaternion.identity;

            var result = new ModelLoadResult(headInstance, bodyInstance, headSway, bodySway);

            ModelFixer.FixGameObjectHierarchy(result);

            physicsImporter.ImportPhysics(result);

            info.Success(result);

            return result;
        }

        private UniTask<ModelLoadResult> ReturnExistingAsync() {
            Debug.Assert(_asyncLoadInfo != null);
            var bodyName = $"{bodyCostumeName}_{bodyCostumeVariation}";
            var headName = $"{headCostumeName}_{headCostumeVariation}";

            return AsyncLoadInfo.ReturnExistingAsync(_asyncLoadInfo, $"Failed to load character model (body {bodyName} and/or head {headName}).");
        }

        public const string CharaHeadObjectName = "CH";

        private static readonly Regex IdolSerialRegex = new Regex(@"^[0-9]{3}[a-z]{3}$");

        // e.g.: cb_ss001_015siz, cb_ex016_b
        private static readonly Regex HeadAssetNameRegex = new Regex("^ch_[a-z]{2}[0-9]{3}(?:_(?:[0-9]{3}[a-z]{3}|[a-z]+))?$");

        // e.g.: cb_ss001_015siz, cb_ex016_b
        private static readonly Regex BodyAssetNameRegex = new Regex("^cb_[a-z]{2}[0-9]{3}(?:_(?:[0-9]{3}[a-z]{3}|[a-z]+))?$");

        // e.g.: ss001
        [SerializeField]
        private string _bodyCostumeName = "ss001";

        // e.g.: 015siz
        [SerializeField]
        private string _bodyCostumeVariation = "001har";

        [SerializeField]
        private string _headCostumeName = "ss001";

        [SerializeField]
        private string _headCostumeVariation = "001har";

        [CanBeNull]
        private AsyncLoadInfo<ModelLoadResult> _asyncLoadInfo;

    }
}
