using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace TemaeTrainer.Sequence.Data
{
    // Loads via UnityWebRequest rather than File.ReadAllText because StreamingAssets
    // is inside the APK on Android/Quest and is not directly file-accessible there.
    public class TemaeJsonLoader
    {
        public async Awaitable<TemaeDocument> LoadFromStreamingAssetsAsync(string fileName)
        {
            var path = Path.Combine(Application.streamingAssetsPath, "Data", fileName);
            using var request = UnityWebRequest.Get(path);
            await Awaitable.FromAsyncOperation(request.SendWebRequest());

            if (request.result != UnityWebRequest.Result.Success)
                throw new InvalidOperationException($"Failed to load '{path}': {request.error}");

            return TemaeJsonParser.Parse(request.downloadHandler.text);
        }
    }
}
