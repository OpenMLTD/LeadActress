using System;
using System.IO;
using Cysharp.Threading.Tasks;
using LeadActress.Runtime.Dancing;
using NLayer;
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

#if UNITY_EDITOR || UNITY_STANDALONE
            using (var www = new UnityWebRequest(uri)) {
                www.downloadHandler = new DownloadHandlerBuffer();

                await www.SendWebRequest();

                var data = www.downloadHandler.data;

                using (var memoryStream = new MemoryStream(data, false)) {
                    using (var mpeg = new MpegFile(memoryStream)) {
                        var samples = new float[mpeg.Length];
                        mpeg.ReadSamples(samples, 0, samples.Length);

                        clip = AudioClip.Create(relativePath, samples.Length, mpeg.Channels, mpeg.SampleRate, false);
                        clip.SetData(samples, 0);
                    }
                }
            }
#else
            using (var www = new UnityWebRequest(uri)) {
                www.downloadHandler = new DownloadHandlerAudioClip(uri, AudioType.MPEG);

                await www.SendWebRequest();

                clip = DownloadHandlerAudioClip.GetContent(www);
            }

            clip.name = relativePath;
#endif

            return clip;
        }

    }
}
