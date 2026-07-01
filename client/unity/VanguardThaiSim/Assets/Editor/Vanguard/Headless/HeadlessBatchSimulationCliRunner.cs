using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using VanguardThaiSim.Headless;

namespace VanguardThaiSim.EditorTools
{
    public static class HeadlessBatchSimulationCliRunner
    {
        public static void RunFromCommandLine()
        {
            int exitCode = 1;
            try
            {
                HeadlessBatchSimulationCliInput input =
                    HeadlessBatchSimulationCliArguments.Parse(Environment.GetCommandLineArgs());
                HeadlessBatchSimulationResult result = input.IsValid
                    ? HeadlessBatchSimulationRunner.Run(input.request)
                    : FailedFromInput(input);

                string resultPath = ResolveResultPath(input);
                string directory = Path.GetDirectoryName(resultPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(resultPath, result.ToJson(true));
                if (result.accepted)
                {
                    Debug.Log("Headless batch simulation passed. Result: " + resultPath + "\n" + result.ToJson(true));
                    exitCode = 0;
                }
                else
                {
                    Debug.LogError("Headless batch simulation blocked. Result: " + resultPath + "\n" + result.ToJson(true));
                    exitCode = 2;
                }
            }
            catch (Exception exception)
            {
                Debug.LogError("Headless batch simulation failed: " + exception);
                exitCode = 1;
            }
            finally
            {
                EditorApplication.Exit(exitCode);
            }
        }

        private static HeadlessBatchSimulationResult FailedFromInput(HeadlessBatchSimulationCliInput input)
        {
            HeadlessBatchSimulationRequest request =
                input == null ? new HeadlessBatchSimulationRequest() : input.request.CloneNormalized();
            return new HeadlessBatchSimulationResult
            {
                accepted = false,
                failure_reason = input == null ? "Invalid batch input." : string.Join(" | ", input.errors.ToArray()),
                start_seed = request.start_seed,
                run_count = request.run_count,
                ruleset = request.ruleset,
                deck_source = string.IsNullOrWhiteSpace(request.deck_code) ? "default" : "deck_code"
            };
        }

        private static string ResolveResultPath(HeadlessBatchSimulationCliInput input)
        {
            if (input != null && !string.IsNullOrWhiteSpace(input.result_path))
            {
                return Path.GetFullPath(input.result_path);
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, "work", "headless_batch_m17_04_result.json");
        }
    }
}
