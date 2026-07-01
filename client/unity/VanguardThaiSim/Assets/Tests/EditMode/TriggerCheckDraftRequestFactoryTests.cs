using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerCheckDraftRequestFactoryTests
    {
        [Test]
        public void CopiesExplicitDraftFields()
        {
            ManualTriggerCheckDraftRequest request = TriggerCheckDraftRequestFactory.Create(
                1,
                TriggerCheckSource.Damage,
                2,
                "damage-card-2",
                "HEAL-001",
                TriggerType.Heal);

            Assert.AreEqual(1, request.player_index);
            Assert.AreEqual(TriggerCheckSource.Damage, request.check_source);
            Assert.AreEqual(2, request.check_index);
            Assert.AreEqual("damage-card-2", request.checked_card_instance_id);
            Assert.AreEqual("HEAL-001", request.checked_card_id);
            Assert.AreEqual(TriggerType.Heal, request.trigger_type);
        }

        [Test]
        public void UsesManualDraftDefaults()
        {
            ManualTriggerCheckDraftRequest request = TriggerCheckDraftRequestFactory.Create(
                0,
                TriggerCheckSource.Drive,
                0,
                "drive-card-1",
                "CRIT-001",
                TriggerType.Critical);

            Assert.AreEqual(CombatModifierExpiration.EndOfTurn, request.modifier_expiration);
            Assert.AreEqual(GameStateViewPerspective.Spectator, request.perspective);
            Assert.AreEqual(-1, request.viewer_player_index);
        }

        [Test]
        public void KeepsNullableCardFieldsForCallerValidation()
        {
            ManualTriggerCheckDraftRequest request = TriggerCheckDraftRequestFactory.Create(
                0,
                TriggerCheckSource.Manual,
                0,
                null,
                null,
                TriggerType.Unknown);

            Assert.IsNull(request.checked_card_instance_id);
            Assert.IsNull(request.checked_card_id);
        }
    }
}
