using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.UI
{
    public static class TriggerCheckDraftRequestFactory
    {
        public static ManualTriggerCheckDraftRequest Create(
            int playerIndex,
            TriggerCheckSource checkSource,
            int checkIndex,
            string selectedCardInstanceId,
            string selectedCardId,
            TriggerType triggerType)
        {
            return new ManualTriggerCheckDraftRequest
            {
                player_index = playerIndex,
                check_source = checkSource,
                check_index = checkIndex,
                checked_card_instance_id = selectedCardInstanceId,
                checked_card_id = selectedCardId,
                trigger_type = triggerType,
                modifier_expiration = CombatModifierExpiration.EndOfTurn,
                perspective = GameStateViewPerspective.Spectator,
                viewer_player_index = -1
            };
        }
    }
}
