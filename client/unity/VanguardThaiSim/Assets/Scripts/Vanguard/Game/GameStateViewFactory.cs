using System;
using System.Collections.Generic;

namespace VanguardThaiSim.Game
{
    public static class GameStateViewFactory
    {
        public const string HiddenCardId = "__hidden_card__";

        public static GameState CreateTrueStateClone(GameState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return GameState.FromJson(state.ToJson(false));
        }

        public static GameState CreatePlayerView(GameState state, int viewerPlayerIndex)
        {
            return CreateView(state, GameStateViewPerspective.Player, viewerPlayerIndex);
        }

        public static GameState CreateSpectatorView(GameState state)
        {
            return CreateView(state, GameStateViewPerspective.Spectator, -1);
        }

        public static GameState CreateView(GameState state, GameStateViewPerspective perspective, int viewerPlayerIndex = -1)
        {
            GameState view = CreateTrueStateClone(state);
            if (perspective == GameStateViewPerspective.TrueState)
            {
                return view;
            }

            Dictionary<string, string> hiddenInstanceIds = new Dictionary<string, string>();
            for (int playerIndex = 0; playerIndex < view.players.Count; playerIndex++)
            {
                PlayerGameState player = view.GetPlayer(playerIndex);
                MaskZone(player.deck, playerIndex, GameZone.Deck, true, hiddenInstanceIds);
                MaskZone(player.hand, playerIndex, GameZone.Hand, !CanSeePrivateZone(perspective, viewerPlayerIndex, playerIndex), hiddenInstanceIds);
                MaskZone(player.ride_deck, playerIndex, GameZone.RideDeck, !CanSeePrivateZone(perspective, viewerPlayerIndex, playerIndex), hiddenInstanceIds);
                MaskZone(player.vanguard, playerIndex, GameZone.Vanguard, false, hiddenInstanceIds);
                MaskZone(player.rear_guard, playerIndex, GameZone.RearGuard, false, hiddenInstanceIds);
                MaskZone(player.drop, playerIndex, GameZone.Drop, false, hiddenInstanceIds);
                MaskZone(player.damage, playerIndex, GameZone.Damage, false, hiddenInstanceIds);
                MaskZone(player.bind, playerIndex, GameZone.Bind, false, hiddenInstanceIds);
                MaskZone(player.trigger, playerIndex, GameZone.Trigger, false, hiddenInstanceIds);
                MaskZone(player.order, playerIndex, GameZone.Order, false, hiddenInstanceIds);
                MaskZone(player.soul, playerIndex, GameZone.Soul, false, hiddenInstanceIds);
                MaskZone(player.g_zone, playerIndex, GameZone.GZone, false, hiddenInstanceIds);
                MaskZone(player.guardian, playerIndex, GameZone.Guardian, false, hiddenInstanceIds);
            }

            MaskEventLog(view.event_log, perspective, viewerPlayerIndex, hiddenInstanceIds);
            return view;
        }

        private static bool CanSeePrivateZone(GameStateViewPerspective perspective, int viewerPlayerIndex, int ownerIndex)
        {
            return perspective == GameStateViewPerspective.Player && viewerPlayerIndex == ownerIndex;
        }

        private static void MaskZone(
            IList<GameCardInstance> cards,
            int ownerIndex,
            GameZone zone,
            bool forceHidden,
            IDictionary<string, string> hiddenInstanceIds)
        {
            if (cards == null)
            {
                return;
            }

            for (int i = 0; i < cards.Count; i++)
            {
                GameCardInstance card = cards[i];
                if (card == null)
                {
                    continue;
                }

                if (!forceHidden && card.face_up)
                {
                    continue;
                }

                string maskedInstanceId = MaskedInstanceId(ownerIndex, zone, i);
                if (!string.IsNullOrEmpty(card.instance_id) && !hiddenInstanceIds.ContainsKey(card.instance_id))
                {
                    hiddenInstanceIds.Add(card.instance_id, maskedInstanceId);
                }

                card.instance_id = maskedInstanceId;
                card.card_id = HiddenCardId;
                card.owner_index = ownerIndex;
                card.face_up = false;
                card.power_delta = 0;
            }
        }

        private static void MaskEventLog(
            IList<GameEvent> events,
            GameStateViewPerspective perspective,
            int viewerPlayerIndex,
            IDictionary<string, string> hiddenInstanceIds)
        {
            if (events == null)
            {
                return;
            }

            for (int i = 0; i < events.Count; i++)
            {
                GameEvent gameEvent = events[i];
                if (gameEvent == null || string.IsNullOrEmpty(gameEvent.card_instance_id))
                {
                    continue;
                }

                string mappedId;
                if (hiddenInstanceIds.TryGetValue(gameEvent.card_instance_id, out mappedId))
                {
                    gameEvent.card_instance_id = mappedId;
                    continue;
                }

                if (ShouldMaskEventCard(gameEvent, perspective, viewerPlayerIndex))
                {
                    gameEvent.card_instance_id = "hidden-event-" + i.ToString("D6");
                }
            }
        }

        private static bool ShouldMaskEventCard(GameEvent gameEvent, GameStateViewPerspective perspective, int viewerPlayerIndex)
        {
            if (perspective == GameStateViewPerspective.TrueState)
            {
                return false;
            }

            bool eventTouchesPrivateZone = IsPrivateZone(gameEvent.from_zone) || IsPrivateZone(gameEvent.to_zone);
            if (!eventTouchesPrivateZone)
            {
                return false;
            }

            if (perspective == GameStateViewPerspective.Spectator)
            {
                return true;
            }

            return gameEvent.actor_index != viewerPlayerIndex;
        }

        private static bool IsPrivateZone(GameZone zone)
        {
            return zone == GameZone.Deck || zone == GameZone.Hand || zone == GameZone.RideDeck;
        }

        private static string MaskedInstanceId(int ownerIndex, GameZone zone, int index)
        {
            return "hidden-p" + ownerIndex + "-" + zone.ToString().ToLowerInvariant() + "-" + index.ToString("D4");
        }
    }
}
