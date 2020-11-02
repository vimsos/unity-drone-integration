using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Drone
{
#if UNITY_EDITOR
    static class CI
    {
        static void Build()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();

            var scenes = new List<string>();
            foreach (var s in EditorBuildSettings.scenes)
            {
                if (s.enabled)
                { scenes.Add(s.path); }
            }
            buildPlayerOptions.scenes = scenes.ToArray();

            buildPlayerOptions.locationPathName = $"{outputPath}/{target}";
            buildPlayerOptions.target = (BuildTarget)Enum.Parse(typeof(BuildTarget), target);
            buildPlayerOptions.options = BuildOptions.None;

            switch (buildPlayerOptions.target)
            {
                case BuildTarget.WebGL:
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.WebGL, ScriptingImplementation.IL2CPP);
                    buildPlayerOptions.locationPathName += $"/{projectName}";
                    break;
                case BuildTarget.StandaloneOSX:
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
                    buildPlayerOptions.locationPathName += $"/{projectName}.app";
                    break;
                case BuildTarget.StandaloneLinux64:
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
                    buildPlayerOptions.locationPathName += $"/{projectName}.x86-64";
                    break;
                case BuildTarget.StandaloneWindows64:
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
                    buildPlayerOptions.locationPathName += $"/{projectName}.exe";
                    break;
            }

            File.WriteAllText($"Assets/Resources/build.txt", buildName);
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (report.summary.result != BuildResult.Succeeded)
            { throw new OperationCanceledException("Unity3D failed to build."); }
        }

        static string env(string key) => Environment.GetEnvironmentVariable(key);
        static string outputPath => env("UNITY_OUTPUT_PATH");
        static string commit => env("DRONE_COMMIT_SHA").Substring(0, 8);
        static string branch => env("DRONE_COMMIT_BRANCH");
        static string target => env("UNITY_BUILD_TARGET");
        static string dateTime => DateTimeOffset.FromUnixTimeSeconds(long.Parse(env("DRONE_BUILD_CREATED"))).ToLocalTime().ToString("yyyy-MM-dd_HH'h'mm");
        static string projectName => env("DRONE_REPO_NAME");
        static string version => Application.version;
        static string buildName => $"{projectName}_{version}_{dateTime}_{target}_{branch}_{commit}";
    }
#endif
}