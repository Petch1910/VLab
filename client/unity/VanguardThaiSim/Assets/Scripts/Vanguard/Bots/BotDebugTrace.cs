using System;
using System.Collections.Generic;
using UnityEngine;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Bots
{
    [Serializable]
    public sealed class BotDebugTraceLine
    {
        public int rank;
        public bool selected;
        public string action_summary;
        public double base_score;
        public double playbook_bias;
        public double total_score;
    }

    [Serializable]
    public sealed class BotDebugTrace
    {
        public string trace_id;
        public int player_index;
        public string playbook_id;
        public string selected_action_summary;
        public double selected_score;
        public int candidate_count;
        public bool sanitized;
        public string explanation;
        public List<BotDebugTraceLine> lines = new List<BotDebugTraceLine>();

        public void EnsureLists()
        {
            if (lines == null)
            {
                lines = new List<BotDebugTraceLine>();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static BotDebugTrace FromJson(string json)
        {
            BotDebugTrace trace = JsonUtility.FromJson<BotDebugTrace>(json);
            if (trace == null)
            {
                throw new ArgumentException("Bot debug trace JSON could not be parsed.", "json");
            }

            trace.EnsureLists();
            return trace;
        }
    }

    public static class BotDebugTracer
    {
        public static BotDebugTrace CreatePlaybookTrace(
            GameState state,
            int playerIndex,
            ICardRepository cardRepository,
            BotPlaybookLibrary playbookLibrary,
            HeuristicBotV2Options heuristicOptions = null,
            PlaybookIntegrationOptions playbookOptions = null,
            int maxLines = 8)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            IReadOnlyList<PlaybookActionEvaluation> evaluations =
                PlaybookIntegratedBot.EvaluateActions(
                    state,
                    playerIndex,
                    cardRepository,
                    playbookLibrary,
                    heuristicOptions,
                    playbookOptions);
            var ordered = new List<PlaybookActionEvaluation>(evaluations);
            ordered.Sort(CompareEvaluations);

            int safeMaxLines = Math.Max(0, maxLines);
            PlaybookActionEvaluation best = ordered.Count == 0 ? null : ordered[0];
            var trace = new BotDebugTrace
            {
                trace_id = BuildTraceId(state, playerIndex, evaluations.Count),
                player_index = playerIndex,
                playbook_id = best == null ? "none" : best.PlaybookId,
                selected_action_summary = best == null ? "none" : SanitizeSummary(best.Summary),
                selected_score = best == null ? 0d : best.TotalScore,
                candidate_count = evaluations.Count,
                sanitized = true,
                explanation = "playbook bot trace; no actions executed"
            };

            int lineCount = Math.Min(safeMaxLines, ordered.Count);
            for (int i = 0; i < lineCount; i++)
            {
                PlaybookActionEvaluation evaluation = ordered[i];
                trace.lines.Add(new BotDebugTraceLine
                {
                    rank = i + 1,
                    selected = i == 0,
                    action_summary = SanitizeSummary(evaluation.Summary),
                    base_score = evaluation.BaseScore,
                    playbook_bias = evaluation.PlaybookBias,
                    total_score = evaluation.TotalScore
                });
            }

            return trace;
        }

        private static int CompareEvaluations(PlaybookActionEvaluation left, PlaybookActionEvaluation right)
        {
            int scoreCompare = right.TotalScore.CompareTo(left.TotalScore);
            if (scoreCompare != 0)
            {
                return scoreCompare;
            }

            return string.CompareOrdinal(left.Summary, right.Summary);
        }

        private static string BuildTraceId(GameState state, int playerIndex, int candidateCount)
        {
            string gameId = string.IsNullOrEmpty(state.game_id) ? "game" : state.game_id;
            return gameId +
                   "-p" + playerIndex +
                   "-t" + state.turn_number +
                   "-trace-c" + candidateCount;
        }

        private static string SanitizeSummary(string summary)
        {
            return summary ?? string.Empty;
        }
    }
}
