using System;
using System.Collections.Generic;
using UnityEngine;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Bots
{
    public static class AdvancedSearchPrototypeRejectionReasons
    {
        public const string ReadinessGateBlocked = "ADVANCED_SEARCH_READINESS_GATE_BLOCKED";
        public const string StateMissing = "ADVANCED_SEARCH_STATE_MISSING";
    }

    [Serializable]
    public sealed class AdvancedSearchCandidate
    {
        public int rank;
        public bool selected;
        public string action_summary;
        public double heuristic_score;
        public bool branch_accepted;
        public int branch_event_count;
        public string rejection_reason;
    }

    [Serializable]
    public sealed class AdvancedSearchPrototypeResult
    {
        public bool accepted;
        public string rejection_reason;
        public string search_id;
        public bool readiness_allowed;
        public int considered_action_count;
        public string selected_action_summary;
        public double selected_score;
        public string summary;
        public List<AdvancedSearchCandidate> candidates = new List<AdvancedSearchCandidate>();

        public void EnsureLists()
        {
            if (candidates == null)
            {
                candidates = new List<AdvancedSearchCandidate>();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static AdvancedSearchPrototypeResult FromJson(string json)
        {
            AdvancedSearchPrototypeResult result =
                JsonUtility.FromJson<AdvancedSearchPrototypeResult>(json);
            if (result == null)
            {
                throw new ArgumentException("Advanced search prototype JSON could not be parsed.", "json");
            }

            result.EnsureLists();
            return result;
        }
    }

    public sealed class AdvancedSearchPrototypeOptions
    {
        public int MaxCandidates = 8;

        public static AdvancedSearchPrototypeOptions CreateDefault()
        {
            return new AdvancedSearchPrototypeOptions();
        }
    }

    public static class AdvancedSearchPrototype
    {
        public static AdvancedSearchPrototypeResult Search(
            GameState state,
            int playerIndex,
            ICardRepository cardRepository,
            IsmctsReadinessReport readinessReport = null,
            HeuristicBotV2Options heuristicOptions = null,
            AdvancedSearchPrototypeOptions options = null)
        {
            if (state == null)
            {
                return Reject(AdvancedSearchPrototypeRejectionReasons.StateMissing);
            }

            IsmctsReadinessReport safeReadiness = readinessReport ?? IsmctsReadinessGate.EvaluateDefault();
            if (!safeReadiness.advanced_search_allowed)
            {
                return Reject(
                    AdvancedSearchPrototypeRejectionReasons.ReadinessGateBlocked,
                    safeReadiness.advanced_search_allowed);
            }

            AdvancedSearchPrototypeOptions safeOptions =
                options ?? AdvancedSearchPrototypeOptions.CreateDefault();
            IReadOnlyList<HeuristicBotActionEvaluation> evaluations =
                HeuristicBotV2.EvaluateLegalActions(state, playerIndex, cardRepository, heuristicOptions);
            var acceptedEvaluations = new List<HeuristicBotActionEvaluation>();
            for (int i = 0; i < evaluations.Count; i++)
            {
                if (evaluations[i].Accepted)
                {
                    acceptedEvaluations.Add(evaluations[i]);
                }
            }

            acceptedEvaluations.Sort(CompareEvaluations);
            int maxCandidates = Math.Max(0, safeOptions.MaxCandidates);
            int candidateCount = Math.Min(maxCandidates, acceptedEvaluations.Count);
            var candidates = new List<AdvancedSearchCandidate>(candidateCount);
            for (int i = 0; i < candidateCount; i++)
            {
                HeuristicBotActionEvaluation evaluation = acceptedEvaluations[i];
                SnapshotSimulationPathResult branch =
                    SnapshotSimulationPath.SimulateSingle(state, evaluation.Action);
                candidates.Add(new AdvancedSearchCandidate
                {
                    rank = i + 1,
                    selected = i == 0,
                    action_summary = SanitizeSummary(evaluation.Summary),
                    heuristic_score = evaluation.TotalScore,
                    branch_accepted = branch.accepted,
                    branch_event_count = branch.branch_event_count,
                    rejection_reason = branch.rejection_reason ?? string.Empty
                });
            }

            AdvancedSearchCandidate selected = candidates.Count == 0 ? null : candidates[0];
            return new AdvancedSearchPrototypeResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                search_id = BuildSearchId(state, playerIndex, evaluations.Count),
                readiness_allowed = true,
                considered_action_count = candidateCount,
                selected_action_summary = selected == null ? "none" : selected.action_summary,
                selected_score = selected == null ? 0d : selected.heuristic_score,
                summary = "one-ply advanced search prototype; no rollout executed",
                candidates = candidates
            };
        }

        private static AdvancedSearchPrototypeResult Reject(string reason, bool readinessAllowed = false)
        {
            return new AdvancedSearchPrototypeResult
            {
                accepted = false,
                rejection_reason = reason ?? string.Empty,
                readiness_allowed = readinessAllowed,
                summary = "Advanced search prototype rejected: " + (reason ?? string.Empty)
            };
        }

        private static int CompareEvaluations(HeuristicBotActionEvaluation left, HeuristicBotActionEvaluation right)
        {
            int scoreCompare = right.TotalScore.CompareTo(left.TotalScore);
            if (scoreCompare != 0)
            {
                return scoreCompare;
            }

            return string.CompareOrdinal(left.Summary, right.Summary);
        }

        private static string BuildSearchId(GameState state, int playerIndex, int candidateCount)
        {
            string gameId = string.IsNullOrEmpty(state.game_id) ? "game" : state.game_id;
            return gameId +
                   "-p" + playerIndex +
                   "-t" + state.turn_number +
                   "-advanced-c" + candidateCount;
        }

        private static string SanitizeSummary(string summary)
        {
            return summary ?? string.Empty;
        }
    }
}
