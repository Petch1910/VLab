using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Headless
{
    [Serializable]
    public sealed class HeadlessBatchSimulationRequest
    {
        public const int DefaultRunCount = 3;
        public const int MaxRunCount = 50;

        public int start_seed = HeadlessSimulationRunner.DefaultSeed;
        public int run_count = DefaultRunCount;
        public string ruleset = HeadlessSimulationRunner.DefaultRuleset;
        public string deck_code;

        public HeadlessBatchSimulationRequest CloneNormalized()
        {
            return new HeadlessBatchSimulationRequest
            {
                start_seed = start_seed,
                run_count = run_count,
                ruleset = HeadlessSimulationRequest.NormalizeRuleset(ruleset),
                deck_code = string.IsNullOrWhiteSpace(deck_code) ? null : deck_code.Trim()
            };
        }
    }

    [Serializable]
    public sealed class HeadlessBatchSimulationResult
    {
        public bool accepted;
        public string failure_reason;
        public int start_seed;
        public int run_count;
        public int accepted_count;
        public int blocked_count;
        public string ruleset;
        public string deck_source;
        public List<HeadlessSimulationResult> results = new List<HeadlessSimulationResult>();

        public string ToJson(bool prettyPrint = false)
        {
            if (results == null)
            {
                results = new List<HeadlessSimulationResult>();
            }

            return JsonUtility.ToJson(this, prettyPrint);
        }
    }

    public sealed class HeadlessBatchSimulationCliInput
    {
        public HeadlessBatchSimulationRequest request = new HeadlessBatchSimulationRequest();
        public string result_path;
        public List<string> errors = new List<string>();

        public bool IsValid
        {
            get { return errors == null || errors.Count == 0; }
        }
    }

    public static class HeadlessBatchSimulationCliArguments
    {
        public const string BatchCountArgument = "-headlessBatchCount";
        public const string StartSeedArgument = "-headlessStartSeed";
        public const string RulesetArgument = HeadlessSimulationCliArguments.RulesetArgument;
        public const string DeckCodeArgument = HeadlessSimulationCliArguments.DeckCodeArgument;
        public const string ResultPathArgument = "-headlessBatchResultPath";

        public static HeadlessBatchSimulationCliInput Parse(string[] args)
        {
            HeadlessBatchSimulationCliInput input = new HeadlessBatchSimulationCliInput();
            if (args == null)
            {
                return input;
            }

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (string.Equals(arg, BatchCountArgument, StringComparison.Ordinal))
                {
                    ReadInt(args, ref i, BatchCountArgument, input.errors, value => input.request.run_count = value);
                }
                else if (string.Equals(arg, StartSeedArgument, StringComparison.Ordinal))
                {
                    ReadInt(args, ref i, StartSeedArgument, input.errors, value => input.request.start_seed = value);
                }
                else if (string.Equals(arg, RulesetArgument, StringComparison.Ordinal))
                {
                    input.request.ruleset = ReadValue(args, ref i, RulesetArgument, input.errors);
                }
                else if (string.Equals(arg, DeckCodeArgument, StringComparison.Ordinal))
                {
                    input.request.deck_code = ReadValue(args, ref i, DeckCodeArgument, input.errors);
                }
                else if (string.Equals(arg, ResultPathArgument, StringComparison.Ordinal))
                {
                    input.result_path = ReadValue(args, ref i, ResultPathArgument, input.errors);
                }
            }

            input.request = input.request.CloneNormalized();
            return input;
        }

        private static void ReadInt(
            string[] args,
            ref int index,
            string argument,
            List<string> errors,
            Action<int> assign)
        {
            string value = ReadValue(args, ref index, argument, errors);
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            int parsed;
            if (int.TryParse(value, out parsed))
            {
                assign(parsed);
            }
            else
            {
                errors.Add("Invalid integer for " + argument + ": " + value);
            }
        }

        private static string ReadValue(string[] args, ref int index, string argument, List<string> errors)
        {
            if (index + 1 >= args.Length || IsKnownArgument(args[index + 1]))
            {
                errors.Add("Missing value for " + argument + ".");
                return null;
            }

            index++;
            return args[index];
        }

        private static bool IsKnownArgument(string value)
        {
            return string.Equals(value, BatchCountArgument, StringComparison.Ordinal) ||
                   string.Equals(value, StartSeedArgument, StringComparison.Ordinal) ||
                   string.Equals(value, RulesetArgument, StringComparison.Ordinal) ||
                   string.Equals(value, DeckCodeArgument, StringComparison.Ordinal) ||
                   string.Equals(value, ResultPathArgument, StringComparison.Ordinal);
        }
    }

    public static class HeadlessBatchSimulationRunner
    {
        public static HeadlessBatchSimulationResult RunDefault()
        {
            return Run(new HeadlessBatchSimulationRequest());
        }

        public static HeadlessBatchSimulationResult Run(HeadlessBatchSimulationRequest request)
        {
            HeadlessBatchSimulationRequest safeRequest =
                (request ?? new HeadlessBatchSimulationRequest()).CloneNormalized();

            HeadlessBatchSimulationResult result = new HeadlessBatchSimulationResult
            {
                start_seed = safeRequest.start_seed,
                run_count = safeRequest.run_count,
                ruleset = safeRequest.ruleset,
                deck_source = string.IsNullOrWhiteSpace(safeRequest.deck_code) ? "default" : "deck_code"
            };

            if (safeRequest.run_count < 1 || safeRequest.run_count > HeadlessBatchSimulationRequest.MaxRunCount)
            {
                result.accepted = false;
                result.failure_reason = "Batch run_count must be between 1 and " +
                                        HeadlessBatchSimulationRequest.MaxRunCount + ".";
                return result;
            }

            for (int i = 0; i < safeRequest.run_count; i++)
            {
                HeadlessSimulationResult run = HeadlessSimulationRunner.Run(new HeadlessSimulationRequest
                {
                    seed = safeRequest.start_seed + i,
                    ruleset = safeRequest.ruleset,
                    deck_code = safeRequest.deck_code
                });

                result.results.Add(run);
                if (run.accepted)
                {
                    result.accepted_count++;
                }
                else
                {
                    result.blocked_count++;
                }
            }

            result.accepted = result.blocked_count == 0;
            if (!result.accepted)
            {
                result.failure_reason = result.blocked_count + " headless run(s) blocked.";
            }

            return result;
        }
    }
}
