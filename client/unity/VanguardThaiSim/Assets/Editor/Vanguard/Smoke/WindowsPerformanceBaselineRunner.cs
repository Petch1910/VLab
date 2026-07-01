using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using VanguardThaiSim.Smoke;

namespace VanguardThaiSim.EditorTools
{
    public static class WindowsPerformanceBaselineRunner
    {
        private const string OutputArg = "-performanceBaselineOutput";

        public static void RunFromCommandLine()
        {
            int exitCode = 1;
            try
            {
                WindowsPerformanceBaselineReport report = WindowsPerformanceBaseline.Run();
                string outputPath = ResolveOutputPath(Environment.GetCommandLineArgs());
                string directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(outputPath, report.ToJson(true));
                if (report.accepted)
                {
                    Debug.Log("Windows performance baseline passed: " + outputPath + "\n" + report.ToJson(true));
                    exitCode = 0;
                }
                else
                {
                    Debug.LogError("Windows performance baseline blocked: " + outputPath + "\n" + report.ToJson(true));
                    exitCode = 2;
                }
            }
            catch (Exception exception)
            {
                Debug.LogError("Windows performance baseline failed: " + exception);
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
            return Path.Combine(projectRoot, "work", "windows_performance_baseline_m27_03.json");
        }
    }
}
