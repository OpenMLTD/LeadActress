using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace LeadActress.Runtime.Loaders {
    [AddComponentMenu("MLTD/Loaders/Scene Loader")]
    public class SceneLoader : MonoBehaviour {

        [Tooltip("The " + nameof(BatchAssetBundleLoader) + " used to load asset bundles.")]
        public BatchAssetBundleLoader bundleLoader;

        [Tooltip("The skybox material used for open scenes.")]
        public Material skyboxMaterial;

        [Tooltip("The material for screens.")]
        public Material vjMaterial;

        [Tooltip("Stage serial number, from 1 to 999.")]
        [Range(MltdSimulationConstants.MinStageSerial, MltdSimulationConstants.MaxStageSerial)]
        public int stageSerial = 1;

        [Tooltip("Whether prop scene(s) should be loaded.")]
        public bool loadPropScenes = true;

        [Tooltip("Load all available prop scene(s) attached with the main scene.")]
        public bool loadAllPropScenes = true;

        [Tooltip("If " + nameof(loadAllPropScenes) + " is not checked, only load these prop scene(s).")]
        public int[] onlyLoadPropScenes = Array.Empty<int>();

        private async void Start() {
            if (stageSerial < MltdSimulationConstants.MinStageSerial || stageSerial > MltdSimulationConstants.MaxStageSerial) {
                Debug.LogError($"Invalid stage serial {stageSerial}, should be {MltdSimulationConstants.MinStageSerial} to {MltdSimulationConstants.MaxStageSerial}.");
            }

            if (loadPropScenes) {
                var tasks = new List<UniTask>();

                tasks.Add(LoadMainScene());

                if (loadAllPropScenes) {
                    var propCount = GetPropSceneCount();

                    for (var i = 1; i <= propCount; i += 1) {
                        tasks.Add(LoadPropScene(i));
                    }
                } else {
                    foreach (var i in onlyLoadPropScenes) {
                        tasks.Add(LoadPropScene(i));
                    }
                }

                await UniTask.WhenAll(tasks.ToArray());
            } else {
                await LoadMainScene();
            }
        }

        private int GetPropSceneCount() {
            var count = 0;

            while (HasPropScene(count + 1)) {
                count += 1;
            }

            return count;
        }

        private bool HasPropScene(int number) {
            var ss = stageSerial.ToString("000");
            var ps = number.ToString("00");
            var bundleFileName = $"stage{ss}_ts{ps}.unity3d";
            var bundleFilePath = Path.Combine(Application.streamingAssetsPath, bundleFileName);

            return File.Exists(bundleFilePath);
        }

        private async UniTask LoadMainScene() {
            var ss = stageSerial.ToString("000");
            var stageName = $"stage{ss}";
            var sceneBundle = await bundleLoader.LoadFromRelativePathAsync($"{stageName}.unity3d");

            Debug.Assert(sceneBundle.isStreamedSceneAssetBundle);

            var scenePaths = sceneBundle.GetAllScenePaths();

            Debug.Assert(scenePaths.Length == 1);

            var sceneName = Path.GetFileNameWithoutExtension(scenePaths[0]);
            Debug.Log($"Scene: {sceneName}");

            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            var scene = SceneManager.GetSceneByName(sceneName);
            var rootObjects = scene.GetRootGameObjects();

            foreach (var obj in rootObjects) {
                FixMaterials(obj);
            }
        }

        private async UniTask LoadPropScene(int number) {
            var ss = stageSerial.ToString("000");
            var ps = number.ToString("00");
            var bundleFileName = $"stage{ss}_ts{ps}.unity3d";

            AssetBundle sceneBundle;
            Debug.Log($"Loading prop scene {ps}");

            try {
                sceneBundle = await bundleLoader.LoadFromRelativePathAsync(bundleFileName);
            } catch (AggregateException ex) {
                Debug.LogException(ex);
                return;
            }

            Debug.Assert(sceneBundle.isStreamedSceneAssetBundle);

            var scenePaths = sceneBundle.GetAllScenePaths();

            Debug.Assert(scenePaths.Length == 1);

            var sceneName = Path.GetFileNameWithoutExtension(scenePaths[0]);

            Debug.Log($"Loaded prop scene {sceneName}/{ps}");

            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            var scene = SceneManager.GetSceneByName(sceneName);
            var rootObjects = scene.GetRootGameObjects();

            foreach (var obj in rootObjects) {
                FixMaterials(obj);
            }
        }

        private void FixMaterials([NotNull] GameObject gameObject) {
            if (
                // gameObject.name.Contains("lt_glow_far") || // Disable texture-based gradient lights from the front of the stage
                // gameObject.name.Contains("lt_glow_beam") || // Disable texture-based gradient lights on stage
                // gameObject.name.Contains("lt_glow_center") || // Emitters on the lights on stage
                gameObject.name.Contains("lt_glow") ||
                gameObject.name.Contains("ltmap") || // Disable light maps
                gameObject.name.Contains("pPlane") // Disable audience generation plates
            ) {
                gameObject.SetActive(false);
                return;
            }

            if (gameObject.TryGetComponent<MeshRenderer>(out var renderer)) {
                var materials = renderer.materials;
                var materialReplaced = false;

                for (var i = 0; i < renderer.materials.Length; i++) {
                    var material = renderer.materials[i];

                    if (material.name.Contains("_sky")) {
                        var newMaterial = skyboxMaterial;
                        materials[i] = newMaterial;
                        Destroy(material);
                        materialReplaced = true;
                    } else if (material.name.Contains("_vj")) {
                        var newMaterial = vjMaterial;
                        materials[i] = newMaterial;
                        Destroy(material);
                        materialReplaced = true;
                    } else {
                        material.shader = Shader.Find("Custom/Standard");
                    }
                }

                if (materialReplaced) {
                    renderer.materials = materials;
                }

                // Enable two-sided shadow casting, except for walls (for better effect in theaters)
                if (!gameObject.name.Contains("wall_")) {
                    renderer.shadowCastingMode = ShadowCastingMode.TwoSided;
                }
            }

            if (gameObject.TryGetComponent<ParticleSystemRenderer>(out var ps)) {
                //ps.material.shader = Shader.Find("Unlit/Texture");
                ps.enabled = false;
            }

            var t = gameObject.transform;
            var count = t.childCount;

            for (var i = 0; i < count; i += 1) {
                var child = t.GetChild(i);
                FixMaterials(child.gameObject);
            }
        }

    }
}
