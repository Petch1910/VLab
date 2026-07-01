using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using VanguardThaiSim.Smoke;

namespace VanguardThaiSim.EditorTools
{
    public static class WindowsPerformanceGateRunner
    {
        private const string OutputArg = "-performanceGateOutput";

        public static void RunFromCommandLine()
        {
            int exitCode = 1;
            try
            {
                WindowsPerformanceGateReport report = WindowsPerformanceGate.Run();
                string outputPath = ResolveOutputPath(Environment.GetCommandLineArgs());
                string directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(outputPath, report.ToJson(true));
                if (report.accepted)
                {
                    Debug.Log("Windows performance gate passed: " + outputPath + "\n" + report.ToJson(true));
                    exitCode = 0;
                }
                else
                {
                    Debug.LogError("Windows performance gate blocked: " + outputPath + "\n" + report.ToJson(true));
                    exitCode = 2;
                }
            }
            catch (Exception exception)
            {
                Debug.LogError("Windows performance gate failed: " + exception);
                exitCode = 1;
            }
            finally
            {
                EditorApplication.Exit(exitCode);
            }
        }

        public static string ResolveOutputPath(string[] args)
        {
            if (args != null)
            {
                for (int i = 0; i < args.Length - 1; i++)
                {
                    if (string.Equals(args[i], OutputArg, StringComparison.Ordinal))
                    {
                        return Path.GetFullPath(args[i + 1]);
                    }
                }
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, "work", "windows_performance_gate_m27_04.json");
        }
    }
}
