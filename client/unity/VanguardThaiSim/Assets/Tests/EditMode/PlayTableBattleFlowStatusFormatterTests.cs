using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PlayTableBattleFlowStatusFormatterTests
    {
        [Test]
        public void NullStateFormatsNoGameLoaded()
        {
            Assert.AreEqual(
                PlayTableBattleFlowStatusFormatter.NoStateMessage,
                PlayTableBattleFlowStatusFormatter.Format(null));
        }

        [Test]
        public void NonBattlePhasePromptsBattlePhase()
        {
            var state = new GameState { phase = GamePhase.Main };

            Assert.AreEqual(
                PlayTableBattleFlowStatusFormatter.NotBattlePhaseMessage,
                PlayTableBattleFlowStatusFormatter.Format(state));
        }

        [Test]
        public void EmptyBattleLogPromptsAttackSetup()
        {
            var state = new GameState { phase = GamePhase.Battle };

            Assert.AreEqual(
                PlayTableBattleFlowStatusFormatter.ReadyMessage,
                PlayTableBattleFlowStatusFormatter.Format(state));
        }

        [Test]
        public void LatestAttackPromptsGuardStepWithoutIds()
        {
            var state = new GameState { phase = GamePhase.Battle };
            state.event_log.Add(new GameEvent
            {
                action_type = GameActionType.DeclareAttack,
                card_instance_id = "attacker-secret",
                target_card_instance_id = "target-secret"
            });

            string formatted = PlayTableBattleFlowStatusFormatter.Format(state);

            Assert.AreEqual(PlayTableBattleFlowStatusFormatter.AttackDeclaredMessage, formatted);
            Assert.IsFalse(formatted.Contains("attacker-secret"));
            Assert.IsFalse(formatted.Contains("target-secret"));
        }

        [Test]
        public void LatestGuardPromptsContinueChecks()
        {
            var state = new GameState { phase = GamePhase.Battle };
            state.event_log.Add(new GameEvent { action_type = GameActionType.Guard });

            Assert.AreEqual(
                PlayTableBattleFlowStatusFormatter.GuardPlacedMessage,
                PlayTableBattleFlowStatusFormatter.Format(state));
        }

        [Test]
        public void LatestTriggerCheckPromptsManualModifierResolution()
        {
            var state = new GameState { phase = GamePhase.Battle };
            state.event_log.Add(new GameEvent { action_type = GameActionType.TriggerCheck });

            Assert.AreEqual(
                PlayTableBattleFlowStatusFormatter.TriggerCheckedMessage,
                PlayTableBattleFlowStatusFormatter.Format(state));
        }
    }
}
