using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public enum TimingWindow
    {
        None,
        PhaseTransition,
        StandAndDrawStep,
        RideStep,
        MainStep,
        BattleStep,
        AttackDeclaration,
        GuardStep,
        DriveCheck,
        DamageCheck,
        BattleResolution,
        CloseStep,
        EndPhase,
        OnDraw,
        OnMoveCard,
        OnSetPhase,
        OnAddGiftMarker,
        OnResourceFlip,
        PendingAutoResolution,
        CleanupEndOfBattle,
        CleanupEndOfTurn,
        ManualTriggerCheck
    }

    [Serializable]
    public sealed class PhaseTimingMatrixEntry
    {
        public string command_id;
        public GamePhase phase;
        public TimingWindow timing_window;
        public bool legal;
        public string notes;
    }

    [Serializable]
    public sealed class PhaseTimingMatrixDefinition
    {
        public List<PhaseTimingMatrixEntry> entries = new List<PhaseTimingMatrixEntry>();

        public void EnsureLists()
        {
            if (entries == null)
            {
                entries = new List<PhaseTimingMatrixEntry>();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static PhaseTimingMatrixDefinition FromJson(string json)
        {
            PhaseTimingMatrixDefinition matrix = JsonUtility.FromJson<PhaseTimingMatrixDefinition>(json);
            if (matrix == null)
            {
                throw new ArgumentException("Phase timing matrix JSON could not be parsed.", "json");
            }

            matrix.EnsureLists();
            return matrix;
        }
    }

    public static class PhaseTimingMatrixCommandIds
    {
        public const string Draw = "GameAction.Draw";
        public const string MoveCard = "GameAction.MoveCard";
        public const string SetPhase = "GameAction.SetPhase";
        public const string AddGiftMarker = "GameAction.AddGiftMarker";
        public const string ResourceFlip = "GameAction.ResourceFlip";
        public const string PendingAutoResolution = "PendingAuto.Resolution";
        public const string TriggerCheckDrive = "TriggerCheck.Drive";
        public const string TriggerCheckDamage = "TriggerCheck.Damage";
        public const string TriggerCheckManual = "TriggerCheck.Manual";
        public const string CleanupEndOfBattle = "Cleanup.EndOfBattle";
        public const string CleanupEndOfTurn = "Cleanup.EndOfTurn";
    }

    public static class PhaseTimingMatrix
    {
        public static PhaseTimingMatrixDefinition CreateCurrentMatrix()
        {
            var matrix = new PhaseTimingMatrixDefinition();

            Add(matrix, PhaseTimingMatrixCommandIds.Draw, GamePhase.StandAndDraw, TimingWindow.OnDraw, true,
                "Canonical draw timing.");
            Add(matrix, PhaseTimingMatrixCommandIds.Draw, GamePhase.Battle, TimingWindow.OnDraw, false,
                "Draw is not a battle-step command in the canonical matrix.");
            Add(matrix, PhaseTimingMatrixCommandIds.MoveCard, GamePhase.Main, TimingWindow.OnMoveCard, true,
                "Canonical manual call/ride/move timing currently treated as main-phase movement.");
            Add(matrix, PhaseTimingMatrixCommandIds.MoveCard, GamePhase.Ride, TimingWindow.OnMoveCard, true,
                "Ride phase can use the current MoveCard command for selected-card ride to Vanguard until a dedicated Ride command exists.");
            Add(matrix, PhaseTimingMatrixCommandIds.MoveCard, GamePhase.Mulligan, TimingWindow.OnMoveCard, true,
                "Mulligan/setup can use MoveCard only for first-vanguard setup from Ride Deck; LegalActionGenerator constrains the source and target.");
            Add(matrix, PhaseTimingMatrixCommandIds.MoveCard, GamePhase.Battle, TimingWindow.OnMoveCard, false,
                "Battle movement needs a specific battle-step command later.");
            AddPhaseTransitions(matrix);
            Add(matrix, PhaseTimingMatrixCommandIds.AddGiftMarker, GamePhase.Main, TimingWindow.OnAddGiftMarker, true,
                "Gift marker placement is treated as a main-phase/manual action for now.");
            Add(matrix, PhaseTimingMatrixCommandIds.AddGiftMarker, GamePhase.Battle, TimingWindow.OnAddGiftMarker, false,
                "Gift marker placement is not modeled as a battle-step command.");
            Add(matrix, PhaseTimingMatrixCommandIds.ResourceFlip, GamePhase.Main, TimingWindow.OnResourceFlip, true,
                "Counter-Blast and Counter-Charge are event-sourced resource operations; ability timing validation is still scoped to structured effect execution.");
            Add(matrix, PhaseTimingMatrixCommandIds.ResourceFlip, GamePhase.Battle, TimingWindow.OnResourceFlip, true,
                "Battle-phase resource operations can be legal when a resolving ability pays or restores counter resources.");

            Add(matrix, PhaseTimingMatrixCommandIds.PendingAutoResolution, GamePhase.Main, TimingWindow.PendingAutoResolution, true,
                "Pending AUTO decisions can be resolved manually after main-phase timing events.");
            Add(matrix, PhaseTimingMatrixCommandIds.PendingAutoResolution, GamePhase.Battle, TimingWindow.PendingAutoResolution, true,
                "Pending AUTO decisions can be resolved manually after battle timing events.");

            Add(matrix, PhaseTimingMatrixCommandIds.TriggerCheckDrive, GamePhase.Battle, TimingWindow.DriveCheck, true,
                "Drive checks belong to battle phase.");
            Add(matrix, PhaseTimingMatrixCommandIds.TriggerCheckDrive, GamePhase.Main, TimingWindow.DriveCheck, false,
                "Drive checks are not main-phase commands.");
            Add(matrix, PhaseTimingMatrixCommandIds.TriggerCheckDamage, GamePhase.Battle, TimingWindow.DamageCheck, true,
                "Damage checks belong to battle phase.");
            Add(matrix, PhaseTimingMatrixCommandIds.TriggerCheckDamage, GamePhase.Main, TimingWindow.DamageCheck, false,
                "Damage checks are not main-phase commands.");
            Add(matrix, PhaseTimingMatrixCommandIds.TriggerCheckManual, GamePhase.Battle, TimingWindow.ManualTriggerCheck, true,
                "Manual trigger draft/check scaffolds are allowed as a battle-phase debug surface.");

            Add(matrix, PhaseTimingMatrixCommandIds.CleanupEndOfBattle, GamePhase.Battle, TimingWindow.CleanupEndOfBattle, true,
                "End-of-battle cleanup is a battle close-step checkpoint.");
            Add(matrix, PhaseTimingMatrixCommandIds.CleanupEndOfBattle, GamePhase.End, TimingWindow.CleanupEndOfBattle, false,
                "End phase should not use end-of-battle cleanup.");
            Add(matrix, PhaseTimingMatrixCommandIds.CleanupEndOfTurn, GamePhase.End, TimingWindow.CleanupEndOfTurn, true,
                "End-of-turn cleanup belongs to end phase.");
            Add(matrix, PhaseTimingMatrixCommandIds.CleanupEndOfTurn, GamePhase.Battle, TimingWindow.CleanupEndOfTurn, false,
                "Battle phase should not use end-of-turn cleanup.");

            return matrix;
        }

        public static bool IsLegal(
            PhaseTimingMatrixDefinition matrix,
            string commandId,
            GamePhase phase,
            TimingWindow timingWindow)
        {
            PhaseTimingMatrixEntry entry = Find(matrix, commandId, phase, timingWindow);
            return entry != null && entry.legal;
        }

        public static PhaseTimingMatrixEntry Find(
            PhaseTimingMatrixDefinition matrix,
            string commandId,
            GamePhase phase,
            TimingWindow timingWindow)
        {
            if (matrix == null || matrix.entries == null)
            {
                return null;
            }

            for (int i = 0; i < matrix.entries.Count; i++)
            {
                PhaseTimingMatrixEntry entry = matrix.entries[i];
                if (entry == null)
                {
                    continue;
                }

                if (string.Equals(entry.command_id, commandId, StringComparison.Ordinal) &&
                    entry.phase == phase &&
                    entry.timing_window == timingWindow)
                {
                    return entry;
                }
            }

            return null;
        }

        public static TimingWindow ToTimingWindow(GameActionType actionType)
        {
            switch (actionType)
            {
                case GameActionType.Draw:
                    return TimingWindow.OnDraw;
                case GameActionType.MoveCard:
                    return TimingWindow.OnMoveCard;
                case GameActionType.SetPhase:
                    return TimingWindow.OnSetPhase;
                case GameActionType.AddGiftMarker:
                    return TimingWindow.OnAddGiftMarker;
                case GameActionType.ResourceFlip:
                    return TimingWindow.OnResourceFlip;
                default:
                    return TimingWindow.None;
            }
        }

        private static void AddPhaseTransitions(PhaseTimingMatrixDefinition matrix)
        {
            Add(matrix, PhaseTimingMatrixCommandIds.SetPhase, GamePhase.Mulligan, TimingWindow.OnSetPhase, true,
                "Current manual phase transition surface.");
            Add(matrix, PhaseTimingMatrixCommandIds.SetPhase, GamePhase.StandAndDraw, TimingWindow.OnSetPhase, true,
                "Current manual phase transition surface.");
            Add(matrix, PhaseTimingMatrixCommandIds.SetPhase, GamePhase.Ride, TimingWindow.OnSetPhase, true,
                "Current manual phase transition surface.");
            Add(matrix, PhaseTimingMatrixCommandIds.SetPhase, GamePhase.Main, TimingWindow.OnSetPhase, true,
                "Current manual phase transition surface.");
            Add(matrix, PhaseTimingMatrixCommandIds.SetPhase, GamePhase.Battle, TimingWindow.OnSetPhase, true,
                "Current manual phase transition surface.");
            Add(matrix, PhaseTimingMatrixCommandIds.SetPhase, GamePhase.End, TimingWindow.OnSetPhase, true,
                "Current manual phase transition surface.");
        }

        private static void Add(
            PhaseTimingMatrixDefinition matrix,
            string commandId,
            GamePhase phase,
            TimingWindow timingWindow,
            bool legal,
            string notes)
        {
            matrix.EnsureLists();
            matrix.entries.Add(new PhaseTimingMatrixEntry
            {
                command_id = commandId ?? string.Empty,
                phase = phase,
                timing_window = timingWindow,
                legal = legal,
                notes = notes ?? string.Empty
            });
        }
    }
}
