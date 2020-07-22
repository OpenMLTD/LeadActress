using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using LeadActress.Utilities;
using UnityEngine;
using UnityEngine.Networking;

namespace LeadActress.Runtime.Loaders {
    [AddComponentMenu("MLTD/Loaders/Batch AssetBundle Loader")]
    public class BatchAssetBundleLoader : MonoBehaviour {

        private void Awake() {
            _bundles = new Dictionary<string, AsyncLoadInfo<AssetBundle>>();
        }

        public async UniTask<AssetBundle> LoadFromRelativePathAsync([NotNull] string relativePath) {
            AsyncLoadInfo<AssetBundle> info = null;
            bool wasAdded;

            lock (this) {
                wasAdded = _bundles.ContainsKey(relativePath);
            }

            if (wasAdded) {
                return await ReturnExistingAsync(relativePath);
            }

            // Double lock
            lock (this) {
                wasAdded = _bundles.ContainsKey(relativePath);

                if (!wasAdded) {
                    info = new AsyncLoadInfo<AssetBundle>();
                    _bundles.Add(relativePath, info);
                }
            }

            if (wasAdded) {
                return await ReturnExistingAsync(relativePath);
            }

            Debug.Assert(info != null);

            AssetBundle bundle;

            try {
                bundle = await LoadFromRelativePathInternalAsync(relativePath);
            } catch (Exception ex) {
                info.Fail(ex);
                throw;
            }

            info.Success(bundle);

            return bundle;
        }

        public void UnloadAll() {
            foreach (var kv in _bundles) {
                if (kv.Value.IsSuccessful()) {
                    var bundle = kv.Value.Result;

                    if (bundle != null) {
                        bundle.Unload(true);
                    }
                }
            }

            _bundles.Clear();
        }

        private async UniTask<AssetBundle> ReturnExistingAsync([NotNull] string relativePath) {
            var info = _bundles[relativePath];

            while (info.IsLoading()) {
                await UniTask.Yield();
            }

            if (info.IsSuccessful()) {
                return info.Result;
            } else {
                if (info.Exception != null) {
                    throw info.Exception;
                } else {
                    throw new FileLoadException($"Failed to load bundle at relative path: {relativePath}", relativePath);
                }
            }
        }

        private static async UniTask<AssetBundle> LoadFromRelativePathInternalAsync([NotNull] string relativePath) {
            var fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
            AssetBundle bundle;

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
                using (var www = new UnityWebRequest(fullPath)) {
                    www.downloadHandler = new DownloadHandlerAssetBundle(fullPath, 0);

                    await www.SendWebRequest();

                    bundle = DownloadHandlerAssetBundle.GetContent(www);
                }
            } else {
                if (!File.Exists(fullPath)) {
                    throw new FileNotFoundException($"Asset bundle not found: {fullPath}", fullPath);
                }

                bundle = AssetBundle.LoadFromFile(fullPath);
            }

            return bundle;
        }

        private void OnDestroy() {
            UnloadAll();
        }

        private Dictionary<string, AsyncLoadInfo<AssetBundle>> _bundles;

    }
}
