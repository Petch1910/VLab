using System;
using System.Collections.Generic;
using UnityEngine;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Bots
{
    [Serializable]
    public sealed class ComboDiscoveryLine
    {
        public string line_id;
        public int rank;
        public string candidate_id;
        public double total_score;
        public double guard_pressure;
        public double trigger_pressure;
        public double trigger_risk;
        public string replay_reference;
        public string explanation;
    }

    [Serializable]
    public sealed class ComboDiscoveryReport
    {
        public string report_id;
        public int player_index;
        public int opponent_player_index;
        public string playbook_id;
        public BotProfileType preferred_profile;
        public double board_score;
        public int opponent_expected_shield;
        public int opponent_maximum_shield;
        public double opponent_guard_confidence;
        public bool trigger_probability_valid;
        public string trigger_probability_error;
        public double probability_at_least_one_trigger;
        public List<string> battle_candidate_ids = new List<string>();
        public List<string> battle_candidate_explanations = new List<string>();
        public List<ComboDiscoveryLine> combo_lines = new List<ComboDiscoveryLine>();
        public int source_event_count;
        public string explanation;

        public void EnsureLists()
        {
            if (battle_candidate_ids == null) battle_candidate_ids = new List<string>();
            if (battle_candidate_explanations == null) battle_candidate_explanations = new List<string>();
            if (combo_lines == null) combo_lines = new List<ComboDiscoveryLine>();
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static ComboDiscoveryReport FromJson(string json)
        {
            ComboDiscoveryReport report = JsonUtility.FromJson<ComboDiscoveryReport>(json);
            if (report == null)
            {
                throw new ArgumentException("Combo discovery report JSON could not be parsed.", "json");
            }

            report.EnsureLists();
            return report;
        }
    }

    public static class ComboDiscoveryRunner
    {
        public static ComboDiscoveryReport Analyze(
            GameState state,
            int playerIndex,
            int opponentPlayerIndex,
            ICardRepository cardRepository,
            BotPlaybookLibrary playbookLibrary,
            int totalCards,
            int triggerCards,
            int checkCount,
            BattleSequenceSearchOptions battleOptions = null)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            BotPlaybookLibrary safeLibrary = playbookLibrary ?? new BotPlaybookLibrary();
            BotPlaybook playbook = safeLibrary.MatchFromState(state, playerIndex);
            BoardResourceEvaluation board = BoardResourceEvaluator.Evaluate(state, playerIndex, cardRepository);
            OpponentGuardEstimate guard = OpponentGuardEstimator.Estimate(state, opponentPlayerIndex, cardRepository);
            bool triggerOk = TriggerProbabilityEngine.TryCalculate(
                totalCards,
                triggerCards,
                checkCount,
                out TriggerProbabilityResult triggerProbability);
            IReadOnlyList<BattleSequenceCandidate> candidates = BattleSequenceSearch.Search(
                state,
                playerIndex,
                cardRepository,
                board,
                triggerOk ? triggerProbability : null,
                battleOptions);
            var v2Options = new BattleSequenceSearchV2Options
            {
                BattleOptions = battleOptions ?? BattleSequenceSearchOptions.CreateDefault()
            };
            IReadOnlyList<BattleSequenceV2Candidate> comboCandidates = BattleSequenceSearchV2.Search(
                state,
                playerIndex,
                cardRepository,
                guard,
                triggerOk ? triggerProbability : null,
                v2Options);

            var report = new ComboDiscoveryReport
            {
                report_id = BuildReportId(state, playerIndex, opponentPlayerIndex, candidates.Count),
                player_index = playerIndex,
                opponent_player_index = opponentPlayerIndex,
                playbook_id = playbook.playbook_id,
                preferred_profile = playbook.preferred_profile,
                board_score = board.TotalScore,
                opponent_expected_shield = guard.ExpectedShieldEstimate,
                opponent_maximum_shield = guard.MaximumShieldEstimate,
                opponent_guard_confidence = guard.Confidence,
                trigger_probability_valid = triggerOk,
                trigger_probability_error = triggerOk ? string.Empty : triggerProbability.ErrorCode,
                probability_at_least_one_trigger = triggerOk ? triggerProbability.ProbabilityAtLeastOneTrigger : 0d,
                source_event_count = state.event_log == null ? 0 : state.event_log.Count,
                explanation = "offline advisory report; no actions executed"
            };

            for (int i = 0; i < candidates.Count; i++)
            {
                report.battle_candidate_ids.Add(candidates[i].CandidateId);
                report.battle_candidate_explanations.Add(candidates[i].Explanation);
            }

            for (int i = 0; i < comboCandidates.Count; i++)
            {
                BattleSequenceV2Candidate candidate = comboCandidates[i];
                report.combo_lines.Add(new ComboDiscoveryLine
                {
                    line_id = report.report_id + "-line-" + i.ToString("D2"),
                    rank = i + 1,
                    candidate_id = candidate.CandidateId,
                    total_score = candidate.TotalScore,
                    guard_pressure = candidate.GuardPressureContribution,
                    trigger_pressure = candidate.TriggerPressureContribution,
                    trigger_risk = candidate.TriggerRisk,
                    replay_reference = BuildReplayReference(state, candidate.CandidateId),
                    explanation = candidate.Explanation
                });
            }

            return report;
        }

        private static string BuildReportId(
            GameState state,
            int playerIndex,
            int opponentPlayerIndex,
            int candidateCount)
        {
            string gameId = string.IsNullOrEmpty(state.game_id) ? "game" : state.game_id;
            return gameId +
                   "-p" + playerIndex +
                   "-o" + opponentPlayerIndex +
                   "-t" + state.turn_number +
                   "-c" + candidateCount;
        }

        private static string BuildReplayReference(GameState state, string candidateId)
        {
            string gameId = string.IsNullOrEmpty(state.game_id) ? "game" : state.game_id;
            int eventCount = state.event_log == null ? 0 : state.event_log.Count;
            return gameId + ":events:" + eventCount + ":candidate:" + (candidateId ?? string.Empty);
        }
    }
}
