using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

namespace LeadActress.Runtime.Dancing {
    [AddComponentMenu("MLTD/MLTD Model Fixer")]
    public class ModelFixer : MonoBehaviour {

        public Material skinMaterial;

        public Material skinMaterialNoShade;

        public Material clothesMaterial;

        public Material cutoutMaterial;

        public void FixAllMaterials([NotNull] GameObject prefab, [NotNull] GameObject instance, [NotNull] ModelFixerOptions options) {
            var oldRenderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
            var newRenderers = instance.GetComponentsInChildren<SkinnedMeshRenderer>();
            var rendererCount = oldRenderers.Length;

            for (var i = 0; i < rendererCount; i += 1) {
                FixMaterials(oldRenderers[i], newRenderers[i], options);
            }
        }

        public static void FixGameObjectHierarchy([NotNull] ModelLoadResult loadResult) {
            AddBodyScaleNode(loadResult.Body);
            // BindHead(loadResult.Body, loadResult.Head);
        }

        public static void FixMeshRenderers([NotNull] ModelLoadResult loadResult) {
            var q = new Queue<GameObject>();

            q.Enqueue(loadResult.Body);

            while (q.Count > 0) {
                var obj = q.Dequeue();
                var t = obj.transform;

                var childCount = t.childCount;

                if (childCount > 0) {
                    for (var i = 0; i < childCount; i += 1) {
                        var childTransform = t.GetChild(i);
                        q.Enqueue(childTransform.gameObject);
                    }
                }

                if (obj.TryGetComponent<MeshRenderer>(out var renderer)) {
                    renderer.shadowCastingMode = ShadowCastingMode.TwoSided;
                }
            }
        }

        private static void AddBodyScaleNode([NotNull] GameObject body) {
            var t = body.transform;

            var model00 = t.Find("MODEL_00");
            var @base = t.Find("MODEL_00/BASE");

            var bodyScale = new GameObject("BODY_SCALE");
            var scaleT = bodyScale.transform;
            scaleT.position = model00.position;
            scaleT.rotation = Quaternion.identity;

            scaleT.SetParent(model00, true);
            @base.SetParent(scaleT, true);
        }

        private static void BindHead([NotNull] GameObject body, [NotNull] GameObject head) {
            var bodyT = body.transform;
            var headT = head.transform;
            var i = 0;

            while (headT.childCount > 1) {
                var child = headT.GetChild(i);
                var go = child.gameObject;

                if (go.name == "KUBI") {
                    i += 1;
                    continue;
                }

                child.SetParent(bodyT, true);
            }

            var kubiBody = bodyT.Find("MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/KUBI");
            var kubiHead = headT.transform.Find("KUBI");
            i = 0;

            while (kubiHead.childCount > 1) {
                var child = kubiHead.GetChild(i);
                var go = child.gameObject;

                if (go.name == "ATAMA") {
                    i += 1;
                    continue;
                }

                child.SetParent(kubiBody, true);
            }

            var atamaBody = kubiBody.Find("ATAMA");
            var atamaHead = kubiHead.Find("ATAMA");

            while (atamaHead.childCount > 0) {
                var child = atamaHead.GetChild(0);
                child.SetParent(atamaBody, true);
            }
        }

        private void FixMaterials([NotNull] SkinnedMeshRenderer prefabRenderer, [NotNull] SkinnedMeshRenderer instanceRenderer, [NotNull] ModelFixerOptions options) {
            var oldMaterials = prefabRenderer.sharedMaterials;
            var newMaterials = instanceRenderer.materials;
            var materialCount = oldMaterials.Length;

            for (var i = 0; i < materialCount; i += 1) {
                var oldMaterial = oldMaterials[i];
                var newMaterial = newMaterials[i];

                if (oldMaterial.name.Contains("skin")) {
                    newMaterial = Instantiate(skinMaterial);
                    ApplyNewMaterial(oldMaterial, newMaterial);
                    newMaterials[i] = newMaterial;
                } else if (oldMaterial.name.Contains("face")) {
                    newMaterial = Instantiate(skinMaterialNoShade);
                    ApplyNewMaterial(oldMaterial, newMaterial);
                    newMaterials[i] = newMaterial;
                } else if (oldMaterial.name.EndsWith("cut")) {
                    // newMaterial.shader = Shader.Find("Unlit/Transparent Cutout");
                    newMaterial = Instantiate(cutoutMaterial);
                    ApplyNewMaterial(oldMaterial, newMaterial);
                    newMaterials[i] = newMaterial;
                } else if (oldMaterial.name.Contains("eyel") || oldMaterial.name.Contains("eyer")) {
                    if (options.EyesHighlights) {
                        newMaterial.shader = Shader.Find("Unlit/Dual");
                    } else {
                        newMaterial.shader = Shader.Find("Unlit/Texture");
                    }
                } else if (oldMaterial.name.Contains("hair")) {
                    if (options.HairHighlights) {
                        newMaterial.shader = Shader.Find("Unlit/Dual");
                    } else {
                        newMaterial.shader = Shader.Find("Unlit/Texture");
                    }
                } else {
                    // newMaterial.shader = Shader.Find("Unlit/Texture");
                    newMaterial = Instantiate(clothesMaterial);
                    ApplyNewMaterial(oldMaterial, newMaterial);
                    newMaterials[i] = newMaterial;
                }
            }

            instanceRenderer.materials = newMaterials;
        }

        private void ApplyNewMaterial([NotNull] Material oldMaterial, [NotNull] Material newMaterial) {
            newMaterial.name = oldMaterial.name;

            var mainTex = oldMaterial.GetTexture("_MainTex");

            newMaterial.shader = Shader.Find("VRM/MToon");

            if (mainTex != null) {
                newMaterial.SetTexture(MToon.Utils.PropMainTex, mainTex);
                newMaterial.SetTexture(MToon.Utils.PropShadeTexture, mainTex);
            }
        }

    }
}
