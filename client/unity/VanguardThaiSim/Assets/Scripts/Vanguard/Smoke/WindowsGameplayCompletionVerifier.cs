using System;
using System.Collections.Generic;
using UnityEngine;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Smoke
{
    [Serializable]
    public sealed class WindowsGameplayCompletionReport
    {
        public bool accepted;
        public string summary;
        public int event_count;
        public int player_vanguard_count;
        public int player_soul_count;
        public int player_rear_guard_count;
        public int player_trigger_count;
        public int opponent_vanguard_count;
        public int opponent_guardian_count;
        public int opponent_trigger_count;
        public GamePhase final_phase;
        public ReplayDeterminismVerificationResult replay_determinism;
        public List<string> steps = new List<string>();
        public List<string> blockers = new List<string>();

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static WindowsGameplayCompletionReport FromJson(string json)
        {
            WindowsGameplayCompletionReport report =
                JsonUtility.FromJson<WindowsGameplayCompletionReport>(json);
            if (report == null)
            {
                throw new ArgumentException(
                    "Windows gameplay completion report JSON could not be parsed.",
                    nameof(json));
            }

            report.EnsureLists();
            return report;
        }

        public void AddStep(string step)
        {
            EnsureLists();
            if (!string.IsNullOrWhiteSpace(step))
            {
                steps.Add(step.Trim());
            }
        }

        public void AddBlocker(string blocker)
        {
            EnsureLists();
            accepted = false;
            blockers.Add(string.IsNullOrWhiteSpace(blocker) ? "Unknown blocker." : blocker.Trim());
        }

        public void EnsureLists()
        {
            if (steps == null)
            {
                steps = new List<string>();
            }

            if (blockers == null)
            {
                blockers = new List<string>();
            }
        }
    }

    public static class WindowsGameplayCompletionVerifier
    {
        private const int SmokeMainCount = 50;
        private const int SmokeRideCount = 4;

        public static WindowsGameplayCompletionReport Run()
        {
            return Run(CreateSmokeDeck("m28"));
        }

        public static WindowsGameplayCompletionReport Run(VanguardDeck deck)
        {
            WindowsGameplayCompletionReport report = new WindowsGameplayCompletionReport();
            try
            {
                VanguardDeck safeDeck = deck ?? CreateSmokeDeck("m28");
                GameState initial = GameStateFactory.CreateTwoPlayerGame(
                    safeDeck,
                    safeDeck,
                    2801,
                    "m28-player",
                    "m28-opponent");
                GameState live = CloneState(initial);

                Execute(live, 0, FirstMove(live, 0, GameZone.RideDeck, GameZone.Vanguard), report, "P1 set first Vanguard.");
                Execute(live, 1, FirstMove(live, 1, GameZone.RideDeck, GameZone.Vanguard), report, "P2 set first Vanguard.");
                Execute(live, 0, FirstMulliganKeep(live, 0), report, "P1 kept opening hand.");
                Execute(live, 1, FirstMulliganKeep(live, 1), report, "P2 kept opening hand.");
                Execute(live, 0, FirstPhase(live, 0, GamePhase.StandAndDraw), report, "P1 entered Stand & Draw.");
                Execute(live, 0, FirstAction(live, 0, GameActionType.Draw), report, "P1 drew for turn.");
                Execute(live, 0, FirstPhase(live, 0, GamePhase.Ride), report, "P1 entered Ride.");
                Execute(live, 0, FirstMove(live, 0, GameZone.Hand, GameZone.Vanguard), report, "P1 rode from hand.");
                Execute(live, 0, FirstPhase(live, 0, GamePhase.Main), report, "P1 entered Main.");
                Execute(live, 0, FirstMove(live, 0, GameZone.Hand, GameZone.RearGuard), report, "P1 called a rear-guard.");
                Execute(live, 0, FirstPhase(live, 0, GamePhase.Battle), report, "P1 entered Battle.");
                Execute(live, 0, FirstAction(live, 0, GameActionType.DeclareAttack), report, "P1 declared an attack.");
                Execute(live, 1, FirstAction(live, 1, GameActionType.Guard), report, "P2 guarded from hand.");
                Execute(live, 0, FirstTrigger(live, 0, TriggerCheckSource.Drive), report, "P1 performed a drive check.");
                Execute(live, 1, FirstTrigger(live, 1, TriggerCheckSource.Damage), report, "P2 performed a damage check.");
                Execute(live, 0, FirstPhase(live, 0, GamePhase.End), report, "P1 entered End.");

                ValidateFinalState(live, report);
                report.replay_determinism =
                    ReplayDeterminismVerifier.Verify(initial, live, live.event_log);
                if (report.replay_determinism == null || !report.replay_determinism.accepted)
                {
                    string reason = report.replay_determinism == null
                        ? "Replay determinism result missing."
                        : report.replay_determinism.rejection_reason;
                    report.AddBlocker("Replay determinism failed: " + reason);
                }
            }
            catch (Exception exception)
            {
                report.AddBlocker(exception.Message);
            }

            report.accepted = report.blockers.Count == 0;
            report.summary = report.accepted
                ? "Windows gameplay completion smoke passed with " + report.event_count + " committed event(s)."
                : "Windows gameplay completion smoke blocked: " + report.blockers[0];
            return report;
        }

        private static void Execute(
            GameState state,
            int playerIndex,
            LegalGameAction action,
            WindowsGameplayCompletionReport report,
            string step)
        {
            RulesCore.ExecuteOrThrow(state, action);
            report.AddStep(step + " [" + action.action_type + " P" + (playerIndex + 1) + "]");
        }

        private static void ValidateFinalState(GameState live, WindowsGameplayCompletionReport report)
        {
            PlayerGameState player = live.GetPlayer(0);
            PlayerGameState opponent = live.GetPlayer(1);

            report.event_count = live.event_log.Count;
            report.player_vanguard_count = player.CountZone(GameZone.Vanguard);
            report.player_soul_count = player.CountZone(GameZone.Soul);
            report.player_rear_guard_count = player.CountZone(GameZone.RearGuard);
            report.player_trigger_count = player.CountZone(GameZone.Trigger);
            report.opponent_vanguard_count = opponent.CountZone(GameZone.Vanguard);
            report.opponent_guardian_count = opponent.CountZone(GameZone.Guardian);
            report.opponent_trigger_count = opponent.CountZone(GameZone.Trigger);
            report.final_phase = live.phase;

            if (report.event_count < 16)
            {
                report.AddBlocker("Expected at least 16 committed events, got " + report.event_count + ".");
            }

            if (report.player_vanguard_count != 1 || report.opponent_vanguard_count != 1)
            {
                report.AddBlocker("Both players must have exactly one Vanguard after setup.");
            }

            if (report.player_soul_count < 1)
            {
                report.AddBlocker("Ride-to-soul path did not leave the previous Vanguard in Soul.");
            }

            if (report.player_rear_guard_count < 1)
            {
                report.AddBlocker("Main phase call did not create a rear-guard.");
            }

            if (report.opponent_guardian_count < 1)
            {
                report.AddBlocker("Guard step did not move an opponent hand card to Guardian.");
            }

            if (report.player_trigger_count < 1 || report.opponent_trigger_count < 1)
            {
                report.AddBlocker("Drive/Damage trigger checks did not move checked cards to Trigger zone.");
            }

            if (report.final_phase != GamePhase.End)
            {
                report.AddBlocker("Expected final phase End, got " + report.final_phase + ".");
            }
        }

        private static LegalGameAction FirstAction(GameState state, int playerIndex, GameActionType actionType)
        {
            IReadOnlyList<LegalGameAction> actions = RulesCore.GetLegalActions(state, playerIndex);
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].action_type == actionType)
                {
                    return actions[i];
                }
            }

            throw new InvalidOperationException("Missing legal action " + actionType + " for player " + playerIndex + ".");
        }

        private static LegalGameAction FirstMove(
            GameState state,
            int playerIndex,
            GameZone fromZone,
            GameZone toZone)
        {
            IReadOnlyList<LegalGameAction> actions = RulesCore.GetLegalActions(state, playerIndex);
            for (int i = 0; i < actions.Count; i++)
            {
                LegalGameAction action = actions[i];
                if (action.action_type == GameActionType.MoveCard &&
                    action.from_zone == fromZone &&
                    action.to_zone == toZone)
                {
                    return action;
                }
            }

            throw new InvalidOperationException(
                "Missing legal move " + fromZone + " to " + toZone + " for player " + playerIndex + ".");
        }

        private static LegalGameAction FirstMulliganKeep(GameState state, int playerIndex)
        {
            IReadOnlyList<LegalGameAction> actions = RulesCore.GetLegalActions(state, playerIndex);
            for (int i = 0; i < actions.Count; i++)
            {
                LegalGameAction action = actions[i];
                if (action.action_type == GameActionType.MulliganCards &&
                    action.card_instance_ids != null &&
                    action.card_instance_ids.Count == 0)
                {
                    return action;
                }
            }

            throw new InvalidOperationException("Missing keep-opening-hand action for player " + playerIndex + ".");
        }

        private static LegalGameAction FirstPhase(GameState state, int playerIndex, GamePhase phase)
        {
            IReadOnlyList<LegalGameAction> actions = RulesCore.GetLegalActions(state, playerIndex);
            for (int i = 0; i < actions.Count; i++)
            {
                LegalGameAction action = actions[i];
                if (action.action_type == GameActionType.SetPhase && action.phase == phase)
                {
                    return action;
                }
            }

            throw new InvalidOperationException("Missing phase action " + phase + " for player " + playerIndex + ".");
        }

        private static LegalGameAction FirstTrigger(
            GameState state,
            int playerIndex,
            TriggerCheckSource source)
        {
            IReadOnlyList<LegalGameAction> actions = RulesCore.GetLegalActions(state, playerIndex);
            for (int i = 0; i < actions.Count; i++)
            {
                LegalGameAction action = actions[i];
                if (action.action_type == GameActionType.TriggerCheck &&
                    action.trigger_check_source == source)
                {
                    return action;
                }
            }

            throw new InvalidOperationException("Missing trigger check " + source + " for player " + playerIndex + ".");
        }

        private static GameState CloneState(GameState state)
        {
            return GameState.FromJson(state.ToJson(false));
        }

        private static VanguardDeck CreateSmokeDeck(string prefix)
        {
            VanguardDeck deck = VanguardDeck.Create(
                prefix + " Windows Gameplay Completion Deck",
                "D",
                "vanguard_th",
                "m28-01");

            for (int i = 0; i < SmokeMainCount; i++)
            {
                deck.AddCard(DeckZone.Main, prefix + "-MAIN-" + i.ToString("00"), 1);
            }

            for (int i = 0; i < SmokeRideCount; i++)
            {
                deck.AddCard(DeckZone.Ride, prefix + "-RIDE-" + i.ToString("00"), 1);
            }

            return deck;
        }
    }
}
