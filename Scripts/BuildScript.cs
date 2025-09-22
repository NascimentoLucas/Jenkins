#if UNITY_EDITOR && !UNITY_CLOUD
#if UNITY_ANDROID
#define AutoBuild
#endif
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System;

namespace Nascimento.Dev.Build
{
    [Serializable]
    public class BuildConfig
    {
        public string parentFolder;      // Optional: defaults to <project root>/Builds
        public string keyAlias;
        public string keystorePassword;
        public string keyPassword;
    }

    public static class BuildScript
    {
        private const string kIndexKey = "LastIndexFromAutoBuild";
        private const string kConfigFileName = "build_config.json"; // Lives in Assets/

        public static void BuildMethod()
        {
#if !AutoBuild
            Debug.Log($"{nameof(BuildMethod)} Not Started");
            return;
#endif
            Debug.Log($"{nameof(BuildMethod)} Started");

            // 1) Load config
            var config = LoadConfigOrThrow();

            // 2) Determine parent folder (default: <project root>/Builds)
            string parentFolder = string.IsNullOrWhiteSpace(config.parentFolder)
                ? Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Builds")
                : config.parentFolder.Trim();
            Directory.CreateDirectory(parentFolder);

            // 3) Apply keystore settings (always under Application.dataPath)
            ApplyKeystoreFromConfig(config);

            // 4) Compute unique output path
            int index = PlayerPrefs.GetInt(kIndexKey, 0);
            string buildPath;
            do
            {
                index++;
                buildPath = Path.Combine(parentFolder, $"Build{index}.apk");
            } while (File.Exists(buildPath));
            PlayerPrefs.SetInt(kIndexKey, index);

            // 5) Build
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = GetEnabledScenes(),
                locationPathName = buildPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded: {summary.totalSize} bytes -> {buildPath}");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("Build failed");
                foreach (var step in report.steps)
                {
                    foreach (var message in step.messages.Where(m => m.type == LogType.Error))
                        Debug.LogError($"[Step: {step.name}] {message.content}");
                }
                WriteBuildErrorsToLogFile(report, Path.Combine(parentFolder, "buildErrors.log"));
                throw new Exception("Build failed. See console and buildErrors.log.");
            }
        }

        private static BuildConfig LoadConfigOrThrow()
        {
            string path = Path.Combine(Application.dataPath, kConfigFileName);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Missing {kConfigFileName} at {path}");

            string json = File.ReadAllText(path);
            var cfg = JsonUtility.FromJson<BuildConfig>(json);

            if (string.IsNullOrWhiteSpace(cfg.keyAlias))
                throw new InvalidDataException("keyAlias is required");
            if (string.IsNullOrWhiteSpace(cfg.keystorePassword))
                throw new InvalidDataException("keystorePassword is required");

            return cfg;
        }

        private static void ApplyKeystoreFromConfig(BuildConfig cfg)
        {
            // Always force keystore under Application.dataPath
            string keystorePath = Path.Combine(Application.dataPath, "build_data_keystore.keystore");
            keystorePath = Path.GetFullPath(keystorePath);

            string keyAlias = cfg.keyAlias.Trim();
            string keystorePassword = cfg.keystorePassword;
            string keyPassword = string.IsNullOrEmpty(cfg.keyPassword) ? keystorePassword : cfg.keyPassword;

            Debug.Log($"[Keystore] path='{keystorePath}', alias='{keyAlias}'");

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystorePath;
            PlayerSettings.Android.keyaliasName = keyAlias;
            PlayerSettings.Android.keystorePass = keystorePassword;
            PlayerSettings.Android.keyaliasPass = keyPassword;

            // Older Unity versions exposed top-level PlayerSettings.keystorePass/keyaliasPass.
            // Access them via reflection only if they exist to avoid compile-time errors
            // on newer versions (e.g., Unity 6000+) where they are obsolete with error.
            TrySetLegacyTopLevelKeystoreIfAvailable(keystorePassword, keyPassword);
        }

        private static void TrySetLegacyTopLevelKeystoreIfAvailable(string storePass, string aliasPass)
        {
            try
            {
                var psType = typeof(PlayerSettings);
                var flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
                var ksProp = psType.GetProperty("keystorePass", flags);
                var kaProp = psType.GetProperty("keyaliasPass", flags);

                if (ksProp != null && ksProp.CanWrite)
                {
                    ksProp.SetValue(null, storePass, null);
                }
                if (kaProp != null && kaProp.CanWrite)
                {
                    kaProp.SetValue(null, aliasPass, null);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"[Keystore] Skipped legacy PlayerSettings.* fields: {e.Message}");
            }
        }

        private static string[] GetEnabledScenes()
        {
            return EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();
        }

        private static void WriteBuildErrorsToLogFile(BuildReport report, string logFilePath)
        {
            using (var writer = new StreamWriter(logFilePath))
            {
                writer.WriteLine($"*Build Errors ({report.steps.Length} steps):");
                foreach (var step in report.steps)
                {
                    writer.WriteLine($"**Step: {step.name} | Messages: {step.messages.Length}");
                    foreach (var message in step.messages)
                        writer.WriteLine($"   - {message.type}: {message.content}");
                }
            }
            Debug.LogError($"Wrote buildErrors.log to: {logFilePath}");
        }
    }
}
#endif
