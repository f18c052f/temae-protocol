using System.IO;
using TemaeTrainer.Sequence.Data;
using UnityEditor;
using UnityEngine;

namespace TemaeTrainer.EditorTools
{
    public static class TemaeDataSync
    {
        internal const string DestFolder = "Assets/StreamingAssets/Data";

        [MenuItem("Tools/Temae/Sync Data to StreamingAssets")]
        public static void Sync()
        {
            var files = TemaeDataValidatorMenu.EnumerateJsonFiles();
            if (files.Length == 0)
            {
                Debug.LogWarning($"No JSON files found under {TemaeDataValidatorMenu.DataFolder}.");
                return;
            }

            var anyError = false;
            foreach (var path in files)
            {
                foreach (var issue in TemaeDataValidatorMenu.ValidateFile(path))
                {
                    if (issue.Severity != ValidationSeverity.Error) continue;
                    anyError = true;
                    Debug.LogError($"{path} :: {issue}");
                }
            }

            if (anyError)
            {
                Debug.LogError("Sync aborted: fix validation errors first (Tools > Temae > Validate Data).");
                return;
            }

            Directory.CreateDirectory(DestFolder);
            foreach (var path in files)
                File.Copy(path, Path.Combine(DestFolder, Path.GetFileName(path)), overwrite: true);

            AssetDatabase.Refresh();
            Debug.Log($"Synced {files.Length} file(s) to {DestFolder}.");
        }
    }
}
