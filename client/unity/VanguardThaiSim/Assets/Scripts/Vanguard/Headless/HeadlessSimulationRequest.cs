using System;
using System.Collections.Generic;

namespace VanguardThaiSim.Headless
{
    [Serializable]
    public sealed class HeadlessSimulationRequest
    {
        public int seed = HeadlessSimulationRunner.DefaultSeed;
        public string ruleset = HeadlessSimulationRunner.DefaultRuleset;
        public string deck_code;

        public static HeadlessSimulationRequest Default()
        {
            return new HeadlessSimulationRequest();
        }

        public HeadlessSimulationRequest CloneNormalized()
        {
            return new HeadlessSimulationRequest
            {
                seed = seed,
                ruleset = NormalizeRuleset(ruleset),
                deck_code = string.IsNullOrWhiteSpace(deck_code) ? null : deck_code.Trim()
            };
        }

        public static string NormalizeRuleset(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return HeadlessSimulationRunner.DefaultRuleset;
            }

            string trimmed = value.Trim();
            if (string.Equals(trimmed, "standard", StringComparison.OrdinalIgnoreCase))
            {
                return "D";
            }

            if (string.Equals(trimmed, "d", StringComparison.OrdinalIgnoreCase))
            {
                return "D";
            }

            if (string.Equals(trimmed, "v", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "v-premium", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "vpremium", StringComparison.OrdinalIgnoreCase))
            {
                return "V";
            }

            if (string.Equals(trimmed, "p", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "premium", StringComparison.OrdinalIgnoreCase))
            {
                return "Premium";
            }

            return trimmed;
        }
    }

    public sealed class HeadlessSimulationCliInput
    {
        public HeadlessSimulationRequest request = HeadlessSimulationRequest.Default();
        public string result_path;
        public string replay_path;
        public List<string> errors = new List<string>();

        public bool IsValid
        {
            get { return errors == null || errors.Count == 0; }
        }
    }

    public static class HeadlessSimulationCliArguments
    {
        public const string ResultPathArgument = "-headlessResultPath";
        public const string SeedArgument = "-headlessSeed";
        public const string RulesetArgument = "-headlessRuleset";
        public const string DeckCodeArgument = "-headlessDeckCode";
        public const string ReplayPathArgument = "-headlessReplayPath";

        public static HeadlessSimulationCliInput Parse(string[] args)
        {
            HeadlessSimulationCliInput input = new HeadlessSimulationCliInput();
            if (args == null)
            {
                return input;
            }

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (string.Equals(arg, ResultPathArgument, StringComparison.Ordinal))
                {
                    input.result_path = ReadValue(args, ref i, ResultPathArgument, input.errors);
                }
                else if (string.Equals(arg, ReplayPathArgument, StringComparison.Ordinal))
                {
                    input.replay_path = ReadValue(args, ref i, ReplayPathArgument, input.errors);
                }
                else if (string.Equals(arg, SeedArgument, StringComparison.Ordinal))
                {
                    string value = ReadValue(args, ref i, SeedArgument, input.errors);
                    if (!string.IsNullOrEmpty(value))
                    {
                        int seed;
                        if (int.TryParse(value, out seed))
                        {
                            input.request.seed = seed;
                        }
                        else
                        {
                            input.errors.Add("Invalid integer for " + SeedArgument + ": " + value);
                        }
                    }
                }
                else if (string.Equals(arg, RulesetArgument, StringComparison.Ordinal))
                {
                    input.request.ruleset = ReadValue(args, ref i, RulesetArgument, input.errors);
                }
                else if (string.Equals(arg, DeckCodeArgument, StringComparison.Ordinal))
                {
                    input.request.deck_code = ReadValue(args, ref i, DeckCodeArgument, input.errors);
                }
            }

            input.request = input.request.CloneNormalized();
            return input;
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
            return string.Equals(value, ResultPathArgument, StringComparison.Ordinal) ||
                   string.Equals(value, SeedArgument, StringComparison.Ordinal) ||
                   string.Equals(value, RulesetArgument, StringComparison.Ordinal) ||
                   string.Equals(value, DeckCodeArgument, StringComparison.Ordinal) ||
                   string.Equals(value, ReplayPathArgument, StringComparison.Ordinal);
        }
    }
}
