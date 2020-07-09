using System;
using System.IO;
using Cysharp.Threading.Tasks;
using LeadActress.Runtime.Dancing;
using UnityEngine;
using UnityEngine.Networking;

namespace LeadActress.Runtime.Loaders {
    [AddComponentMenu("MLTD/Loaders/External Audio Loader")]
    public class ExternalAudioLoader : MonoBehaviour {

        public CommonResourceProperties commonResourceProperties;

        public async UniTask<AudioClip> LoadAsync() {
            var relativePath = $"song3_{commonResourceProperties.songResourceName}.mp3";
            var fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
            var uri = new Uri(fullPath);

            AudioClip clip;

            using (var www = new UnityWebRequest(uri)) {
                www.downloadHandler = new DownloadHandlerAudioClip(uri, AudioType.MPEG);

                await www.SendWebRequest();

                clip = DownloadHandlerAudioClip.GetContent(www);
            }

            clip.name = relativePath;

            return clip;
        }

    }
}
