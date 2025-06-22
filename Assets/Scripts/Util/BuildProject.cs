#if UNITY_EDITOR
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Util
{
    public static class BuildProject
    {
        private static string _debug;
        private const string VERSION = "1.1";

        private static readonly List<Process> Procs = new();
        private static void ExecuteSequentialCommands(string[] commands, bool wait = true, bool dependent = true)
        {
            var proc = new Process();
            proc.StartInfo = new ProcessStartInfo("/bin/bash", "-c '" + string.Join(dependent ? " && " : "; ", commands) + "'")
                { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            Procs.Add(proc);
            
            proc.Start();
            if (wait) proc.WaitForExit();
        }
        
        [MenuItem("File/Execute CI\u200A\u200A\u2215\u200A\u200ACD (macOS only) &#B")]
        public static void Build()
        {
            if (Procs.Count > 0)
            {
                Debug.LogError("Build already in progress!");
                return;
            }

            var butlerAPIPath = $"{Application.dataPath}/../.butler";
            ExecuteSequentialCommands(new[]
            {
                "mkdir /tmp/voidwatch",
                "cd /tmp/voidwatch",
                "curl -L -o butler.zip https://broth.itch.ovh/butler/darwin-amd64/LATEST/archive/default",
                "unzip butler.zip",
                "chmod +x butler",
            });
            
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            
            // MACOS
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);
            BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "/tmp/voidwatch/mac/Voidwatch.app",
                target = BuildTarget.StandaloneOSX
            });
            ExecuteSequentialCommands(new[]
            {
                "cd /tmp/voidwatch/mac",
                "codesign --deep -s - -f Voidwatch.app",
                "zip -r voidwatch.zip Voidwatch.app",
                $"../butler push voidwatch.zip DuskBlossomGames/Voidwatch:osx --identity=\"{butlerAPIPath}\" --userversion={VERSION}",
            }, false);
            
            // WINDOWS
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            for (var arch = 0; arch <= 1; arch++)
            {
                var archStr = arch == 0 ? "x86" : "arm";
                
                EditorUserBuildSettings.SetPlatformSettings(BuildPipeline.GetBuildTargetName(BuildTarget.StandaloneWindows64),
                    "Architecture", ((OSArchitecture) arch).ToString());
                BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = $"/tmp/voidwatch/win-{archStr}/Voidwatch/Voidwatch.exe",
                    target = BuildTarget.StandaloneWindows64
                });
                ExecuteSequentialCommands(new[]
                {
                    $"cd /tmp/voidwatch/win-{archStr}",
                    "rm -rf Voidwatch/Voidwatch_BurstDebugInformation_DoNotShip/",
                    "zip -r voidwatch.zip Voidwatch/",
                    $"../butler push voidwatch.zip DuskBlossomGames/Voidwatch:windows-{archStr} --identity=\"{butlerAPIPath}\" --userversion={VERSION}",
                }, false);
            }
            
            // LINUX
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);
            BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "/tmp/voidwatch/linux/Voidwatch/Voidwatch",
                target = BuildTarget.StandaloneLinux64
            });
            ExecuteSequentialCommands(new[]
            {
                "cd /tmp/voidwatch/linux",
                "rm -rf Voidwatch/Voidwatch_BurstDebugInformation_DoNotShip/",
                "zip -r \"voidwatch.zip\" Voidwatch/",
                $"../butler push voidwatch.zip DuskBlossomGames/Voidwatch:linux --identity=\"{butlerAPIPath}\" --userversion={VERSION}"
            }, false);
            
            // reset to OSX
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);
            
            // wait for finish and cleanup
            while (Procs.Any(p=>!p.HasExited)) System.Threading.Thread.Sleep(500); // checking every half second should be plenty
            Procs.Clear();
            
            ExecuteSequentialCommands(new [] { "rm -rf /tmp/voidwatch" });
            EditorUtility.DisplayDialog("Build Complete", $"Uploaded Voidwatch v{VERSION} to channels 'osx', 'windows-x86', 'windows-arm', and 'linux' on Itch.", "OK");
        }
    }
}
#endif