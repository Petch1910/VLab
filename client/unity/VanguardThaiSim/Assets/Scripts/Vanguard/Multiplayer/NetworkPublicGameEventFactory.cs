using System;
using System.Collections.Generic;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    public static class NetworkPublicGameEventFactory
    {
        public static NetworkPublicGameEvent Create(
            GameState stateBeforeEvent,
            GameEvent gameEvent,
            RoomPlayerInfo player,
            string roomId,
            string packDefinitionHash)
        {
            NetworkPublicGameEvent publicEvent = Create(stateBeforeEvent, gameEvent);
            string revealProof;
            string rejectionReason;
            if (DeckCommitmentService.TryCreateRevealProof(player, roomId, packDefinitionHash, publicEvent, out revealProof, out rejectionReason))
            {
                publicEvent.reveal_proof = revealProof;
            }

            return publicEvent;
        }

        public static NetworkPublicGameEvent Create(GameState stateBeforeEvent, GameEvent gameEvent)
        {
            if (gameEvent == null)
            {
                throw new ArgumentNullException(nameof(gameEvent));
            }

            NetworkPublicGameEvent publicEvent = new NetworkPublicGameEvent
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                event_id = "public-" + (gameEvent.event_id ?? Guid.NewGuid().ToString("N")),
                source_event_id = gameEvent.event_id,
                visibility = PublicEventVisibility.Public,
                action_type = gameEvent.action_type,
                actor_index = gameEvent.actor_index,
                from_zone = gameEvent.from_zone,
                to_zone = gameEvent.to_zone,
                from_index = gameEvent.from_index,
                to_index = gameEvent.to_index,
                previous_phase = gameEvent.previous_phase,
                new_phase = gameEvent.new_phase,
                gift_marker_type = gameEvent.gift_marker_type,
                marker_delta = gameEvent.marker_delta
            };

            if (gameEvent.action_type == GameActionType.Draw || gameEvent.action_type == GameActionType.MoveCard)
            {
                publicEvent.from_zone_count_delta = -1;
                publicEvent.to_zone_count_delta = 1;
                GameCardInstance card = FindCard(stateBeforeEvent, gameEvent.actor_index, gameEvent.from_zone, gameEvent.card_instance_id);
                if (ShouldRevealCardIdentity(gameEvent.from_zone, gameEvent.to_zone))
                {
                    publicEvent.hides_card_identity = false;
                    publicEvent.public_card_id = card == null ? "" : card.card_id;
                    publicEvent.public_card_instance_id = publicEvent.event_id + "-card";
                }
                else
                {
                    publicEvent.hides_card_identity = true;
                    publicEvent.public_card_id = "";
                    publicEvent.public_card_instance_id = "";
                }
            }

            return publicEvent;
        }

        private static bool ShouldRevealCardIdentity(GameZone fromZone, GameZone toZone)
        {
            return IsPublicZone(fromZone) || IsPublicZone(toZone);
        }

        private static bool IsPublicZone(GameZone zone)
        {
            return zone != GameZone.Deck && zone != GameZone.Hand && zone != GameZone.RideDeck;
        }

        private static GameCardInstance FindCard(GameState state, int playerIndex, GameZone zone, string cardInstanceId)
        {
            if (state == null || string.IsNullOrEmpty(cardInstanceId))
            {
                return null;
            }

            PlayerGameState player = state.GetPlayer(playerIndex);
            List<GameCardInstance> cards = player.GetZone(zone);
            for (int i = 0; i < cards.Count; i++)
            {
                GameCardInstance card = cards[i];
                if (card != null && string.Equals(card.instance_id ?? "", cardInstanceId, StringComparison.Ordinal))
                {
                    return card;
                }
            }

            return null;
        }
    }
}
