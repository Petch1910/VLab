using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using VanguardThaiSim.Smoke;

namespace VanguardThaiSim.EditorTools
{
    public static class ClientSmokeFlowRunner
    {
        public static void RunFromCommandLine()
        {
            int exitCode = 1;
            try
            {
                ClientSmokeFlowReport report = ClientSmokeFlowVerifier.Run();
                VerifyBuildSettings(report);

                if (report.IsPass)
                {
                    Debug.Log("Client smoke flow passed:\n" + report.ToJson(true));
                    exitCode = 0;
                }
                else
                {
                    Debug.LogError("Client smoke flow blocked:\n" + report.ToJson(true));
                    exitCode = 2;
                }
            }
            catch (Exception exception)
            {
                Debug.LogError("Client smoke flow failed: " + exception);
                exitCode = 1;
            }
            finally
            {
                EditorApplication.Exit(exitCode);
            }
        }

        private static void VerifyBuildSettings(ClientSmokeFlowReport report)
        {
            VerifyEnabledScene(report);
            VerifyBuildTarget(report, BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, "Windows Standalone");
            VerifyDefine(report, BuildTargetGroup.Standalone, "Standalone", "VANGUARD_PHOTON_REALTIME");
            report.AddStep("Build settings smoke: Windows target and Photon define are available.");
        }

        private static void VerifyEnabledScene(ClientSmokeFlowReport report)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (!scenes[i].enabled)
                {
                    continue;
                }

                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                string absolutePath = Path.Combine(projectRoot, scenes[i].path);
                if (!File.Exists(absolutePath))
                {
                    report.AddBlocker("Enabled scene is missing: " + scenes[i].path);
                    return;
                }

                report.AddStep("Build settings smoke: enabled scene found at " + scenes[i].path + ".");
                return;
            }

            report.AddBlocker("No enabled scene found in EditorBuildSettings.");
        }

        private static void VerifyBuildTarget(
            ClientSmokeFlowReport report,
            BuildTargetGroup group,
            BuildTarget target,
            string label)
        {
            if (!BuildPipeline.IsBuildTargetSupported(group, target))
            {
                report.AddBlocker(label + " build target support is not installed or not available.");
            }
        }

        private static void VerifyDefine(
            ClientSmokeFlowReport report,
            BuildTargetGroup group,
            string label,
            string define)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            if (string.IsNullOrEmpty(defines) ||
                Array.IndexOf(defines.Split(';'), define) < 0)
            {
                report.AddBlocker(label + " scripting define is missing: " + define);
            }
        }
    }
}
