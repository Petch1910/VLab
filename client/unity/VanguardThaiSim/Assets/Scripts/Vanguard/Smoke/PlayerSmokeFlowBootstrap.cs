using System;
using System.IO;
using UnityEngine;

namespace VanguardThaiSim.Smoke
{
    public static class PlayerSmokeFlowBootstrap
    {
        public const string SmokeFlag = "-vanguardPlayerSmoke";
        public const string OutputArg = "-vanguardPlayerSmokeOutput";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RunFromPlayer()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (!IsSmokeRequested(args))
            {
                return;
            }

            int exitCode = 1;
            try
            {
                ClientSmokeFlowReport report = ClientSmokeFlowVerifier.Run();
                string outputPath = ResolveOutputPath(
                    args,
                    Path.Combine(Application.persistentDataPath, "player_smoke_flow_report.json"));
                WriteReport(outputPath, report);

                if (report.IsPass)
                {
                    Debug.Log("Player smoke flow passed: " + outputPath + "\n" + report.ToJson(true));
                    exitCode = 0;
                }
                else
                {
                    Debug.LogError("Player smoke flow blocked: " + outputPath + "\n" + report.ToJson(true));
                    exitCode = 2;
                }
            }
            catch (Exception exception)
            {
                Debug.LogError("Player smoke flow failed: " + exception);
                exitCode = 1;
            }
            finally
            {
                Application.Quit(exitCode);
            }
        }

        public static bool IsSmokeRequested(string[] args)
        {
            if (args == null)
            {
                return false;
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], SmokeFlag, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static string ResolveOutputPath(string[] args, string fallbackPath)
        {
            string explicitPath = GetArgValue(args, OutputArg);
            if (!string.IsNullOrWhiteSpace(explicitPath))
            {
                return Path.GetFullPath(explicitPath);
            }

            if (string.IsNullOrWhiteSpace(fallbackPath))
            {
                return Path.GetFullPath("player_smoke_flow_report.json");
            }

            return Path.GetFullPath(fallbackPath);
        }

        private static string GetArgValue(string[] args, string key)
        {
            if (args == null)
            {
                return string.Empty;
            }

            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }

            return string.Empty;
        }

        private static void WriteReport(string outputPath, ClientSmokeFlowReport report)
        {
            string directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(outputPath, report.ToJson(true));
        }
    }
}
