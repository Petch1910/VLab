using System;
using System.Collections.Generic;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    public static class NetworkPublicGameEventApplier
    {
        public static NetworkPublicGameEventApplyResult ApplyToSession(
            LocalOwnerPrivateSession session,
            NetworkPublicGameEvent publicEvent)
        {
            if (session == null)
            {
                return NetworkPublicGameEventApplyResult.Rejected("SESSION_MISSING");
            }

            session.EnsureLists();
            if (session.opponent_public_view == null)
            {
                return NetworkPublicGameEventApplyResult.Rejected("PUBLIC_VIEW_MISSING");
            }

            NetworkPublicGameEventApplyResult result = ApplyToPublicView(session.opponent_public_view, publicEvent);
            if (!result.accepted)
            {
                return result;
            }

            session.public_event_log.Add(Clone(publicEvent));
            session.event_cursor = session.public_event_log.Count;
            return result;
        }

        public static NetworkPublicGameEventApplyResult ApplyToPublicView(
            GameState publicView,
            NetworkPublicGameEvent publicEvent)
        {
            if (publicView == null)
            {
                return NetworkPublicGameEventApplyResult.Rejected("PUBLIC_VIEW_MISSING");
            }

            if (publicEvent == null)
            {
                return NetworkPublicGameEventApplyResult.Rejected("PUBLIC_EVENT_MISSING");
            }

            if (publicEvent.protocol_version != MultiplayerProtocol.ProtocolVersion)
            {
                return NetworkPublicGameEventApplyResult.Rejected("PROTOCOL_VERSION_MISMATCH");
            }

            publicView.EnsureLists();
            if (publicEvent.actor_index < 0 || publicEvent.actor_index >= publicView.players.Count)
            {
                return NetworkPublicGameEventApplyResult.Rejected("ACTOR_INDEX_OUT_OF_RANGE");
            }

            switch (publicEvent.action_type)
            {
                case GameActionType.Draw:
                case GameActionType.MoveCard:
                    return ApplyMove(publicView, publicEvent);
                case GameActionType.SetPhase:
                    publicView.phase = publicEvent.new_phase;
                    return NetworkPublicGameEventApplyResult.Accepted(publicEvent);
                case GameActionType.AddGiftMarker:
                    publicView.GetPlayer(publicEvent.actor_index).AddGiftMarker(
                        publicEvent.gift_marker_type,
                        publicEvent.marker_delta);
                    return NetworkPublicGameEventApplyResult.Accepted(publicEvent);
                default:
                    return NetworkPublicGameEventApplyResult.Rejected("PUBLIC_EVENT_ACTION_UNSUPPORTED");
            }
        }

        private static NetworkPublicGameEventApplyResult ApplyMove(GameState publicView, NetworkPublicGameEvent publicEvent)
        {
            PlayerGameState player = publicView.GetPlayer(publicEvent.actor_index);
            List<GameCardInstance> fromZone = player.GetZone(publicEvent.from_zone);
            List<GameCardInstance> toZone = player.GetZone(publicEvent.to_zone);
            GameCardInstance movedCard = null;

            if (publicEvent.from_zone_count_delta < 0)
            {
                movedCard = RemovePublicCard(fromZone, publicEvent);
            }

            if (publicEvent.to_zone_count_delta > 0)
            {
                toZone.Add(CreateIncomingCard(publicEvent, movedCard));
            }

            return NetworkPublicGameEventApplyResult.Accepted(publicEvent);
        }

        private static GameCardInstance RemovePublicCard(List<GameCardInstance> zone, NetworkPublicGameEvent publicEvent)
        {
            if (zone == null || zone.Count == 0)
            {
                return null;
            }

            int index = FindPublicCardIndex(zone, publicEvent.public_card_instance_id);
            if (index < 0 && publicEvent.from_index >= 0 && publicEvent.from_index < zone.Count)
            {
                index = publicEvent.from_index;
            }

            if (index < 0)
            {
                index = 0;
            }

            GameCardInstance card = zone[index];
            zone.RemoveAt(index);
            return card;
        }

        private static int FindPublicCardIndex(List<GameCardInstance> zone, string publicCardInstanceId)
        {
            if (zone == null || string.IsNullOrWhiteSpace(publicCardInstanceId))
            {
                return -1;
            }

            for (int i = 0; i < zone.Count; i++)
            {
                GameCardInstance card = zone[i];
                if (card != null && string.Equals(card.instance_id ?? "", publicCardInstanceId, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        private static GameCardInstance CreateIncomingCard(NetworkPublicGameEvent publicEvent, GameCardInstance movedCard)
        {
            if (!publicEvent.hides_card_identity && !string.IsNullOrWhiteSpace(publicEvent.public_card_id))
            {
                return new GameCardInstance(
                    string.IsNullOrWhiteSpace(publicEvent.public_card_instance_id)
                        ? publicEvent.event_id + "-public-card"
                        : publicEvent.public_card_instance_id,
                    publicEvent.public_card_id,
                    publicEvent.actor_index,
                    true);
            }

            if (movedCard != null &&
                movedCard.card_id != GameStateViewFactory.HiddenCardId &&
                movedCard.face_up)
            {
                return new GameCardInstance(
                    movedCard.instance_id,
                    movedCard.card_id,
                    publicEvent.actor_index,
                    true);
            }

            return new GameCardInstance(
                publicEvent.event_id + "-hidden-card",
                GameStateViewFactory.HiddenCardId,
                publicEvent.actor_index,
                false);
        }

        private static NetworkPublicGameEvent Clone(NetworkPublicGameEvent publicEvent)
        {
            return publicEvent == null
                ? null
                : NetworkPublicGameEvent.FromJson(publicEvent.ToJson(false));
        }
    }
}
