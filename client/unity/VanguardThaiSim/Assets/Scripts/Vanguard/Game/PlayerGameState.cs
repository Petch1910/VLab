using System;
using System.Collections.Generic;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class PlayerGameState
    {
        public string player_id;
        public List<GameCardInstance> deck = new List<GameCardInstance>();
        public List<GameCardInstance> hand = new List<GameCardInstance>();
        public List<GameCardInstance> ride_deck = new List<GameCardInstance>();
        public List<GameCardInstance> vanguard = new List<GameCardInstance>();
        public List<GameCardInstance> rear_guard = new List<GameCardInstance>();
        public List<GameCardInstance> drop = new List<GameCardInstance>();
        public List<GameCardInstance> damage = new List<GameCardInstance>();
        public List<GameCardInstance> bind = new List<GameCardInstance>();
        public List<GameCardInstance> trigger = new List<GameCardInstance>();
        public List<GameCardInstance> order = new List<GameCardInstance>();
        public List<GameCardInstance> soul = new List<GameCardInstance>();
        public List<GameCardInstance> g_zone = new List<GameCardInstance>();
        public List<GameCardInstance> guardian = new List<GameCardInstance>();
        public List<GiftMarkerState> gift_markers = new List<GiftMarkerState>();

        public List<GameCardInstance> GetZone(GameZone zone)
        {
            EnsureLists();
            switch (zone)
            {
                case GameZone.Deck:
                    return deck;
                case GameZone.Hand:
                    return hand;
                case GameZone.RideDeck:
                    return ride_deck;
                case GameZone.Vanguard:
                    return vanguard;
                case GameZone.RearGuard:
                    return rear_guard;
                case GameZone.Drop:
                    return drop;
                case GameZone.Damage:
                    return damage;
                case GameZone.Bind:
                    return bind;
                case GameZone.Trigger:
                    return trigger;
                case GameZone.Order:
                    return order;
                case GameZone.Soul:
                    return soul;
                case GameZone.GZone:
                    return g_zone;
                case GameZone.Guardian:
                    return guardian;
                default:
                    throw new ArgumentOutOfRangeException(nameof(zone), zone, null);
            }
        }

        public int CountZone(GameZone zone)
        {
            return GetZone(zone).Count;
        }

        public int GetGiftMarkerCount(GiftMarkerType markerType)
        {
            EnsureLists();
            GiftMarkerState marker = FindGiftMarker(markerType);
            return marker == null ? 0 : marker.count;
        }

        public void AddGiftMarker(GiftMarkerType markerType, int delta)
        {
            EnsureLists();
            GiftMarkerState marker = FindGiftMarker(markerType);
            if (marker == null)
            {
                marker = new GiftMarkerState(markerType, 0);
                gift_markers.Add(marker);
            }

            marker.count = Math.Max(0, marker.count + delta);
        }

        public void EnsureLists()
        {
            if (deck == null) deck = new List<GameCardInstance>();
            if (hand == null) hand = new List<GameCardInstance>();
            if (ride_deck == null) ride_deck = new List<GameCardInstance>();
            if (vanguard == null) vanguard = new List<GameCardInstance>();
            if (rear_guard == null) rear_guard = new List<GameCardInstance>();
            if (drop == null) drop = new List<GameCardInstance>();
            if (damage == null) damage = new List<GameCardInstance>();
            if (bind == null) bind = new List<GameCardInstance>();
            if (trigger == null) trigger = new List<GameCardInstance>();
            if (order == null) order = new List<GameCardInstance>();
            if (soul == null) soul = new List<GameCardInstance>();
            if (g_zone == null) g_zone = new List<GameCardInstance>();
            if (guardian == null) guardian = new List<GameCardInstance>();
            if (gift_markers == null) gift_markers = new List<GiftMarkerState>();
        }

        private GiftMarkerState FindGiftMarker(GiftMarkerType markerType)
        {
            foreach (GiftMarkerState marker in gift_markers)
            {
                if (marker.type == markerType)
                {
                    return marker;
                }
            }

            return null;
        }
    }
}
