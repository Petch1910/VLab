using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using VanguardThaiSim.Headless;

namespace VanguardThaiSim.EditorTools
{
    public static class HeadlessSimulationCliRunner
    {
        public static void RunFromCommandLine()
        {
            int exitCode = 1;
            try
            {
                HeadlessSimulationCliInput input = HeadlessSimulationCliArguments.Parse(Environment.GetCommandLineArgs());
                HeadlessSimulationOutput output;
                if (input.IsValid)
                {
                    output = HeadlessSimulationRunner.RunWithReplay(input.request);
                }
                else
                {
                    HeadlessSimulationResult failed =
                        HeadlessSimulationResult.Failed(string.Join(" | ", input.errors.ToArray()), input.request.seed);
                    failed.ruleset = input.request.ruleset;
                    failed.deck_source = string.IsNullOrWhiteSpace(input.request.deck_code) ? "default" : "deck_code";
                    output = new HeadlessSimulationOutput
                    {
                        result = failed,
                        replay = HeadlessReplayArtifact.FromResult(failed, null)
                    };
                }

                HeadlessSimulationResult result = output.result;
                string resultPath = ResolveResultPath(input);
                string replayPath = ResolveReplayPath(input, resultPath);
                result.result_path = resultPath;
                result.replay_path = replayPath;
                string directory = Path.GetDirectoryName(resultPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(resultPath, result.ToJson(true));

                string replayDirectory = Path.GetDirectoryName(replayPath);
                if (!string.IsNullOrEmpty(replayDirectory))
                {
                    Directory.CreateDirectory(replayDirectory);
                }

                File.WriteAllText(replayPath, output.replay.ToJson(true));

                if (result.accepted)
                {
                    Debug.Log(
                        "Headless simulation passed. Result: " + resultPath +
                        " Replay: " + replayPath + "\n" + result.ToJson(true));
                    exitCode = 0;
                }
                else
                {
                    Debug.LogError(
                        "Headless simulation blocked. Result: " + resultPath +
                        " Replay: " + replayPath + "\n" + result.ToJson(true));
                    exitCode = 2;
                }
            }
            catch (Exception exception)
            {
                Debug.LogError("Headless simulation failed: " + exception);
                exitCode = 1;
            }
            finally
            {
                EditorApplication.Exit(exitCode);
            }
        }

        private static string ResolveResultPath(HeadlessSimulationCliInput input)
        {
            if (input != null && !string.IsNullOrWhiteSpace(input.result_path))
            {
                return Path.GetFullPath(input.result_path);
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, "work", "headless_m17_01_result.json");
        }

        private static string ResolveReplayPath(HeadlessSimulationCliInput input, string resultPath)
        {
            if (input != null && !string.IsNullOrWhiteSpace(input.replay_path))
            {
                return Path.GetFullPath(input.replay_path);
            }

            string directory = Path.GetDirectoryName(resultPath);
            if (string.IsNullOrEmpty(directory))
            {
                directory = Directory.GetCurrentDirectory();
            }

            return Path.Combine(directory, "headless_replay.json");
        }
    }
}
