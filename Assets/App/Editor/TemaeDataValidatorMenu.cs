using System;
using System.Collections.Generic;
using System.IO;
using TemaeTrainer.Sequence.Data;
using UnityEditor;
using UnityEngine;

namespace TemaeTrainer.EditorTools
{
    public static class TemaeDataValidatorMenu
    {
        internal const string DataFolder = "Assets/App/Data";

        [MenuItem("Tools/Temae/Validate Data")]
        public static void ValidateAll()
        {
            var files = EnumerateJsonFiles();
            if (files.Length == 0)
            {
                Debug.LogWarning($"No JSON files found under {DataFolder}.");
                return;
            }

            var anyError = false;
            foreach (var path in files)
            {
                IReadOnlyList<ValidationIssue> issues;
                try
                {
                    issues = ValidateFile(path);
                }
                catch (Exception ex)
                {
                    anyError = true;
                    Debug.LogError($"{path} :: failed to parse: {ex.Message}");
                    continue;
                }

                foreach (var issue in issues)
                {
                    if (issue.Severity == ValidationSeverity.Error)
                    {
                        anyError = true;
                        Debug.LogError($"{path} :: {issue}");
                    }
                    else
                    {
                        Debug.LogWarning($"{path} :: {issue}");
                    }
                }
            }

            if (!anyError)
                Debug.Log($"Temae data validation: all {files.Length} file(s) passed.");
        }

        internal static IReadOnlyList<ValidationIssue> ValidateFile(string path)
        {
            var json = File.ReadAllText(path);
            var doc = TemaeJsonParser.Parse(json);
            return TemaeDataValidator.Validate(doc);
        }

        internal static string[] EnumerateJsonFiles()
        {
            return !Directory.Exists(DataFolder)
                ? Array.Empty<string>()
                : Directory.GetFiles(DataFolder, "*.json", SearchOption.TopDirectoryOnly);
        }
    }
}
