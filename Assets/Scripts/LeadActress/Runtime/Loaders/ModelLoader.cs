using System;
using System.Collections.Generic;
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

        public string headCostumeName {
            get => _headCostumeName;
            set => _headCostumeName = value;
        }

        public string headCostumeVariation {
            get => _headCostumeVariation;
            set => _headCostumeVariation = value;
        }

        public string headCostumeOverride {
            get => _headCostumeOverride;
            set => _headCostumeOverride = value;
        }

        public string bodyCostumeName {
            get => _bodyCostumeName;
            set => _bodyCostumeName = value;
        }

        public string bodyCostumeVariation {
            get => _bodyCostumeVariation;
            set => _bodyCostumeVariation = value;
        }

        public string bodyCostumeOverride {
            get => _bodyCostumeOverride;
            set => _bodyCostumeOverride = value;
        }

        public string bodyTextureOverride {
            get => _bodyTextureOverride;
            set => _bodyTextureOverride = value;
        }

        public bool importEyesHighlights {
            get => _importEyesHighlights;
            set => _importEyesHighlights = value;
        }

        public bool importHairHighlights {
            get => _importHairHighlights;
            set => _importHairHighlights = value;
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

            var fixerOptions = ModelFixerOptions.CreateDefault();
            fixerOptions.EyesHighlights = importEyesHighlights;
            fixerOptions.HairHighlights = importHairHighlights;

            var bodyDetailName = string.IsNullOrWhiteSpace(bodyCostumeOverride) ? bodyCostumeName : bodyCostumeOverride;
            var bodyModelBundleName = $"cb_{bodyDetailName}_{bodyCostumeVariation}";

            if (!BodyAssetNameRegex.IsMatch(bodyModelBundleName)) {
                info.Fail();
                throw new FormatException($"\"{bodyModelBundleName}\" is not a valid body model asset name.");
            }

            var bodyBundle = await bundleLoader.LoadFromRelativePathAsync($"{bodyModelBundleName}.unity3d");
            AssetBundle exBodyBundle = null;

            GameObject bodyInstance;
            SwayController bodySway;

            do {
                string assetGroup;
                if (IdolSerialRegex.IsMatch(bodyCostumeVariation)) {
                    assetGroup = bodyModelBundleName;
                } else {
                    assetGroup = $"cb_{bodyDetailName}_a";
                }

                var modelPath = $"assets/imas/resources/chara/body/{bodyCostumeName}/{assetGroup}/{bodyModelBundleName}.fbx";

                if (bodyBundle.Contains(modelPath)) {
                    var model = bodyBundle.LoadAsset<GameObject>(modelPath);

                    bodyInstance = Instantiate(model, stageObject.transform);

                    modelFixer.FixAllMaterials(model, bodyInstance, fixerOptions);

                    var swayPath = $"assets/imas/resources/chara/body/{bodyCostumeName}/{assetGroup}/{bodyModelBundleName}_sway.txt";
                    var swayFile = bodyBundle.LoadAsset<TextAsset>(swayPath);

                    bodySway = SwayAsset.Parse(swayFile.text);

                    break;
                }

                exBodyBundle = bodyBundle;

                // TODO: remove duplicate code
                bodyDetailName = bodyCostumeName;
                bodyModelBundleName = $"cb_{bodyDetailName}_{bodyCostumeVariation}";

                bodyBundle = await bundleLoader.LoadFromRelativePathAsync($"{bodyModelBundleName}.unity3d");

                if (IdolSerialRegex.IsMatch(bodyCostumeVariation)) {
                    assetGroup = bodyModelBundleName;
                } else {
                    assetGroup = $"cb_{bodyDetailName}_a";
                }

                modelPath = $"assets/imas/resources/chara/body/{bodyCostumeName}/{assetGroup}/{bodyModelBundleName}.fbx";

                {
                    var model = bodyBundle.LoadAsset<GameObject>(modelPath);

                    bodyInstance = Instantiate(model, stageObject.transform);

                    modelFixer.FixAllMaterials(model, bodyInstance, fixerOptions);

                    var swayPath = $"assets/imas/resources/chara/body/{bodyCostumeName}/{assetGroup}/{bodyModelBundleName}_sway.txt";
                    var swayFile = bodyBundle.LoadAsset<TextAsset>(swayPath);

                    bodySway = SwayAsset.Parse(swayFile.text);
                }
            } while (false);

            var bodyAtama = bodyInstance.transform.Find("MODEL_00/BASE/MUNE1/MUNE2/KUBI/ATAMA");

            GameObject headInstance;
            SwayController headSway;
            AssetBundle headBundle, exHeadBundle = null;

            if (string.IsNullOrWhiteSpace(headCostumeOverride)) {
                (headInstance, headSway, headBundle) = await TryLoadHeadSimple(bodyAtama, info, fixerOptions);
            } else {
                (headInstance, headSway, headBundle, exHeadBundle) = await TryLoadHeadEx(bodyAtama, info, fixerOptions);
            }

            bodyInstance.transform.position = Vector3.zero;
            bodyInstance.transform.rotation = Quaternion.identity;

            headInstance.transform.position = Vector3.zero;
            headInstance.transform.rotation = Quaternion.identity;

            var result = new ModelLoadResult(headInstance, bodyInstance, headSway, bodySway);

            // Ex bundles (for MR4/MR5 etc.):
            //   head: the base contains both mesh and texture; the extra may contain overriding texture (or nothing at all, like sr128_017kth)
            //   body: the base contains both mesh and texture; the extra contains texture, may contain mesh
            ProcessHeadCostumeOverride(exHeadBundle, headInstance, headCostumeName, headCostumeVariation, headCostumeOverride);
            ProcessBodyCostumeOverride(exBodyBundle, bodyInstance, bodyCostumeName, bodyCostumeVariation, bodyCostumeOverride);
            ProcessBodyTextureOverride(bodyBundle, bodyInstance, bodyCostumeName, bodyCostumeVariation, bodyTextureOverride);

            ModelFixer.FixGameObjectHierarchy(result);
            ModelFixer.FixMeshRenderers(result);

            physicsImporter.ImportPhysics(result);

            info.Success(result);

            return result;
        }

        private async UniTask<(GameObject HeadInstance, SwayController Sway, AssetBundle HeadBundle)> TryLoadHeadSimple([NotNull] Transform bodyAtama, [NotNull] AsyncLoadInfo<ModelLoadResult> info, [NotNull] ModelFixerOptions fixerOptions) {
            var headModelBundleName = $"ch_{headCostumeName}_{headCostumeVariation}";

            if (!HeadAssetNameRegex.IsMatch(headModelBundleName)) {
                info.Fail();
                throw new FormatException($"\"{headModelBundleName}\" is not a valid head model asset name.");
            }

            var headModelBundle = await bundleLoader.LoadFromRelativePathAsync($"{headModelBundleName}.unity3d");

            string assetGroup;
            if (IdolSerialRegex.IsMatch(headCostumeVariation)) {
                assetGroup = headModelBundleName;
            } else {
                assetGroup = $"ch_{headCostumeName}_a";
            }

            var modelPath = $"assets/imas/resources/chara/head/{headCostumeName}/{assetGroup}/{headModelBundleName}.fbx";

            var model = headModelBundle.LoadAsset<GameObject>(modelPath);

            var headInstance = Instantiate(model, bodyAtama, true);
            headInstance.name = CharaHeadObjectName;

            modelFixer.FixAllMaterials(model, headInstance, fixerOptions);

            var swayPath = $"assets/imas/resources/chara/head/{headCostumeName}/{assetGroup}/{headModelBundleName}_sway.txt";
            var swayFile = headModelBundle.LoadAsset<TextAsset>(swayPath);
            var headSway = SwayAsset.Parse(swayFile.text);

            return (headInstance, headSway, headModelBundle);
        }

        // TODO: simplify and remove duplicate code
        private async UniTask<(GameObject HeadInstance, SwayController Sway, AssetBundle HeadBundle, AssetBundle ExHeadBundle)> TryLoadHeadEx([NotNull] Transform bodyAtama, [NotNull] AsyncLoadInfo<ModelLoadResult> info, [NotNull] ModelFixerOptions fixerOptions) {
            var headModelBundleName = $"ch_{headCostumeName}_{headCostumeVariation}";
            var exHeadModelBundleName = $"ch_{headCostumeOverride}_{headCostumeVariation}";

            if (!HeadAssetNameRegex.IsMatch(headModelBundleName)) {
                info.Fail();
                throw new FormatException($"\"{headModelBundleName}\" is not a valid head model asset name.");
            }

            if (!HeadAssetNameRegex.IsMatch(exHeadModelBundleName)) {
                info.Fail();
                throw new FormatException($"\"{exHeadModelBundleName}\" is not a valid head model asset name.");
            }

            var headModelBundle = await bundleLoader.LoadFromRelativePathAsync($"{headModelBundleName}.unity3d");
            var exHeadModelBundle = await bundleLoader.LoadFromRelativePathAsync($"{exHeadModelBundleName}.unity3d");

            string assetGroup;
            if (IdolSerialRegex.IsMatch(headCostumeVariation)) {
                assetGroup = headModelBundleName;
            } else {
                assetGroup = $"ch_{headCostumeName}_a";
            }

            var modelPath = $"assets/imas/resources/chara/head/{headCostumeName}/{assetGroup}/{headModelBundleName}.fbx";

            var model = headModelBundle.LoadAsset<GameObject>(modelPath);

            var headInstance = Instantiate(model, bodyAtama, true);
            headInstance.name = CharaHeadObjectName;

            modelFixer.FixAllMaterials(model, headInstance, fixerOptions);

            var swayPath = $"assets/imas/resources/chara/head/{headCostumeName}/{assetGroup}/{headModelBundleName}_sway.txt";
            var swayFile = headModelBundle.LoadAsset<TextAsset>(swayPath);
            var headSway = SwayAsset.Parse(swayFile.text);

            return (headInstance, headSway, headModelBundle, exHeadModelBundle);
        }

        private static void ProcessHeadCostumeOverride([CanBeNull] AssetBundle exHeadBundle, [NotNull] GameObject headInstance, [NotNull] string headCostumeName, [NotNull] string headCostumeVariation, [NotNull] string headCostumeOverride) {
            if (string.IsNullOrWhiteSpace(headCostumeOverride)) {
                return;
            }

            Debug.Assert(exHeadBundle != null);

            // TODO: remove duplicate code
            var q = new Queue<GameObject>();
            q.Enqueue(headInstance);

            var origTexPrefix = $"ch_{headCostumeName}_{headCostumeVariation}";
            var newTexPrefix = $"ch_{headCostumeOverride}_{headCostumeVariation}";

            while (q.Count > 0) {
                var go = q.Dequeue();

                {
                    var t = go.transform;
                    var childCount = t.childCount;

                    for (var i = 0; i < childCount; i += 1) {
                        var childTransform = t.GetChild(i);
                        q.Enqueue(childTransform.gameObject);
                    }
                }

                if (!go.TryGetComponent<SkinnedMeshRenderer>(out var renderer)) {
                    continue;
                }

                var materials = renderer.materials;

                foreach (var material in materials) {
                    if (!material.HasProperty("_MainTex")) {
                        continue;
                    }

                    var oldTexture = material.GetTexture("_MainTex");

                    if (!oldTexture.name.StartsWith(origTexPrefix)) {
                        continue;
                    }

                    var trailing = oldTexture.name.Substring(origTexPrefix.Length);
                    var newTexName = $"{newTexPrefix}{trailing}";
                    var newTexPath = $"assets/imas/resources/chara/head/{headCostumeName}/{origTexPrefix}/{newTexName}.png";

                    if (exHeadBundle.Contains(newTexPath)) {
                        var newTexture = exHeadBundle.LoadAsset<Texture2D>(newTexPath);
                        material.SetTexture("_MainTex", newTexture);
                    } else {
                        Debug.LogWarning($"Cannot find texture override: {newTexPath}");
                    }
                }
            }
        }

        // TODO: remove duplicate code
        private static void ProcessBodyCostumeOverride([CanBeNull] AssetBundle exBodyBundle, [NotNull] GameObject bodyInstance, [NotNull] string bodyCostumeName, [NotNull] string bodyCostumeVariation, [NotNull] string bodyCostumeOverride) {
            if (exBodyBundle == null) {
                return;
            }

            if (string.IsNullOrWhiteSpace(bodyCostumeOverride)) {
                return;
            }

            var q = new Queue<GameObject>();
            q.Enqueue(bodyInstance);

            var origTexPrefix = $"cb_{bodyCostumeName}_{bodyCostumeVariation}";
            var newTexPrefix = $"cb_{bodyCostumeOverride}_{bodyCostumeVariation}";

            while (q.Count > 0) {
                var go = q.Dequeue();

                {
                    var t = go.transform;
                    var childCount = t.childCount;

                    for (var i = 0; i < childCount; i += 1) {
                        var childTransform = t.GetChild(i);
                        q.Enqueue(childTransform.gameObject);
                    }
                }

                if (!go.TryGetComponent<SkinnedMeshRenderer>(out var renderer)) {
                    continue;
                }

                var materials = renderer.materials;

                foreach (var material in materials) {
                    if (!material.HasProperty("_MainTex")) {
                        continue;
                    }

                    var oldTexture = material.GetTexture("_MainTex");

                    if (!oldTexture.name.StartsWith(origTexPrefix)) {
                        continue;
                    }

                    var trailing = oldTexture.name.Substring(origTexPrefix.Length);
                    var newTexName = $"{newTexPrefix}{trailing}";
                    // Note the naming difference between here and the head!
                    var newTexPath = $"assets/imas/resources/chara/body/{bodyCostumeName}/{newTexPrefix}/{newTexName}.png";

                    if (exBodyBundle.Contains(newTexPath)) {
                        var newTexture = exBodyBundle.LoadAsset<Texture2D>(newTexPath);
                        material.SetTexture("_MainTex", newTexture);
                    } else {
                        Debug.LogWarning($"Cannot find texture override: {newTexPath}");
                    }
                }
            }
        }

        private static void ProcessBodyTextureOverride([NotNull] AssetBundle bodyBundle, [NotNull] GameObject bodyInstance, [NotNull] string bodyCostumeName, [NotNull] string bodyCostumeVariation, [NotNull] string bodyTextureOverride) {
            if (string.IsNullOrWhiteSpace(bodyTextureOverride)) {
                return;
            }

            var q = new Queue<GameObject>();
            q.Enqueue(bodyInstance);

            var origTexPrefix = $"cb_{bodyCostumeName}_{bodyCostumeVariation}";
            var newTexPrefix = $"cb_{bodyCostumeName}_{bodyTextureOverride}";

            while (q.Count > 0) {
                var go = q.Dequeue();

                {
                    var t = go.transform;
                    var childCount = t.childCount;

                    for (var i = 0; i < childCount; i += 1) {
                        var childTransform = t.GetChild(i);
                        q.Enqueue(childTransform.gameObject);
                    }
                }

                if (!go.TryGetComponent<SkinnedMeshRenderer>(out var renderer)) {
                    continue;
                }

                var materials = renderer.materials;

                foreach (var material in materials) {
                    if (!material.HasProperty("_MainTex")) {
                        continue;
                    }

                    var oldTexture = material.GetTexture("_MainTex");

                    if (!oldTexture.name.StartsWith(origTexPrefix)) {
                        continue;
                    }

                    var trailing = oldTexture.name.Substring(origTexPrefix.Length);
                    var newTexName = $"{newTexPrefix}{trailing}";
                    var newTexPath = $"assets/imas/resources/chara/body/{bodyCostumeName}/{origTexPrefix}/{newTexName}.png";

                    if (bodyBundle.Contains(newTexPath)) {
                        var newTexture = bodyBundle.LoadAsset<Texture2D>(newTexPath);
                        material.SetTexture("_MainTex", newTexture);
                    } else {
                        Debug.LogWarning($"Cannot find texture override: {newTexPath}");
                    }
                }
            }
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

        [SerializeField]
        private string _headCostumeName = "ss001";

        [SerializeField]
        private string _headCostumeVariation = "001har";

        // e.g. "ss101" when costumeName="ss001"
        [LabelOverride("Head Cos. Name Override")]
        [SerializeField]
        private string _headCostumeOverride = string.Empty;

        // e.g.: ss001
        [SerializeField]
        private string _bodyCostumeName = "ss001";

        // e.g.: 015siz
        [SerializeField]
        private string _bodyCostumeVariation = "001har";

        // e.g. "sr128" when costumeName="sr028"
        [LabelOverride("Body Cos. Name Override")]
        [SerializeField]
        private string _bodyCostumeOverride = string.Empty;

        // e.g. "049mom", when bodyCostumeName="sr028", bodyCostumeVariation="a"
        // Some costumes, e.g. sr028, has multiple textures in one file for one mesh
        [SerializeField]
        private string _bodyTextureOverride = string.Empty;

        [SerializeField]
        private bool _importEyesHighlights = true;

        [SerializeField]
        private bool _importHairHighlights = true;

        [CanBeNull]
        private AsyncLoadInfo<ModelLoadResult> _asyncLoadInfo;

    }
}
