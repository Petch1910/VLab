using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public static class HiddenStateViewHardeningRejectionReasons
    {
        public const string StateMissing = "HIDDEN_STATE_VIEW_STATE_MISSING";
        public const string PlayersMissing = "HIDDEN_STATE_VIEW_PLAYERS_MISSING";
        public const string ViewFactoryFailed = "HIDDEN_STATE_VIEW_FACTORY_FAILED";
        public const string HiddenStateLeak = "HIDDEN_STATE_VIEW_LEAK";
        public const string SourceStateMutated = "HIDDEN_STATE_VIEW_SOURCE_MUTATED";
    }

    [Serializable]
    public sealed class HiddenStateViewHardeningCheck
    {
        public string check_id;
        public bool passed;
        public string message;
    }

    [Serializable]
    public sealed class HiddenStateViewHardeningVerificationResult
    {
        public bool accepted;
        public string rejection_reason;
        public int checked_rule_count;
        public int passed_rule_count;
        public int failed_rule_count;
        public string summary;
        public List<HiddenStateViewHardeningCheck> checks = new List<HiddenStateViewHardeningCheck>();

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static HiddenStateViewHardeningVerificationResult FromJson(string json)
        {
            HiddenStateViewHardeningVerificationResult result =
                JsonUtility.FromJson<HiddenStateViewHardeningVerificationResult>(json);
            if (result == null)
            {
                throw new ArgumentException(
                    "Hidden state view hardening verification result JSON could not be parsed.",
                    "json");
            }

            if (result.checks == null)
            {
                result.checks = new List<HiddenStateViewHardeningCheck>();
            }

            return result;
        }
    }

    public static class HiddenStateViewHardeningVerifier
    {
        private static readonly GameZone[] PrivateZones =
        {
            GameZone.Deck,
            GameZone.Hand,
            GameZone.RideDeck
        };

        private static readonly GameZone[] PublicZones =
        {
            GameZone.Vanguard,
            GameZone.RearGuard,
            GameZone.Drop,
            GameZone.Damage,
            GameZone.Bind,
            GameZone.Trigger,
            GameZone.Order,
            GameZone.Soul,
            GameZone.GZone,
            GameZone.Guardian
        };

        public static HiddenStateViewHardeningVerificationResult Verify(GameState trueState)
        {
            if (trueState == null)
            {
                return Reject(HiddenStateViewHardeningRejectionReasons.StateMissing, null);
            }

            if (trueState.players == null || trueState.players.Count == 0)
            {
                return Reject(HiddenStateViewHardeningRejectionReasons.PlayersMissing, null);
            }

            GameStateNoMutationSnapshot sourceSnapshot = NoMutationSnapshot.Capture(trueState);
            GameState source = CloneState(trueState);
            List<HiddenStateViewHardeningCheck> checks = new List<HiddenStateViewHardeningCheck>();

            try
            {
                GameState trueView = GameStateViewFactory.CreateView(
                    trueState,
                    GameStateViewPerspective.TrueState);
                CheckTrueStateClone(source, trueView, checks);

                for (int viewerIndex = 0; viewerIndex < source.players.Count; viewerIndex++)
                {
                    GameState playerView = GameStateViewFactory.CreatePlayerView(trueState, viewerIndex);
                    CheckPlayerView(source, playerView, viewerIndex, checks);
                }

                GameState spectatorView = GameStateViewFactory.CreateSpectatorView(trueState);
                CheckSpectatorView(source, spectatorView, checks);
            }
            catch (Exception exception)
            {
                AddCheck(
                    checks,
                    "view-factory",
                    false,
                    HiddenStateViewHardeningRejectionReasons.ViewFactoryFailed + ": " + exception.Message);
            }

            if (!sourceSnapshot.Matches(trueState))
            {
                AddCheck(
                    checks,
                    "source-state-no-mutation",
                    false,
                    "GameStateViewFactory changed the source true state while creating views.");
            }

            int failed = CountFailed(checks);
            if (failed > 0)
            {
                string reason = HasFailedCheck(checks, "source-state-no-mutation")
                    ? HiddenStateViewHardeningRejectionReasons.SourceStateMutated
                    : HiddenStateViewHardeningRejectionReasons.HiddenStateLeak;
                return Reject(reason, checks);
            }

            return Accept(checks);
        }

        private static void CheckTrueStateClone(
            GameState source,
            GameState trueView,
            List<HiddenStateViewHardeningCheck> checks)
        {
            AddCheck(
                checks,
                "true-state-clone-preserves-hidden-data",
                JsonUtility.ToJson(source, false) == JsonUtility.ToJson(trueView, false),
                "True-state perspective must preserve the full source state in a clone.");
        }

        private static void CheckPlayerView(
            GameState source,
            GameState view,
            int viewerIndex,
            List<HiddenStateViewHardeningCheck> checks)
        {
            AddCheck(
                checks,
                "player-" + viewerIndex + "-player-count",
                view != null && view.players != null && view.players.Count == source.players.Count,
                "Player view must preserve player count.");

            if (view == null || view.players == null)
            {
                return;
            }

            for (int ownerIndex = 0; ownerIndex < source.players.Count; ownerIndex++)
            {
                CheckHiddenZone(source, view, ownerIndex, GameZone.Deck, "player-" + viewerIndex, checks);
                CheckPrivateZoneVisibility(
                    source,
                    view,
                    ownerIndex,
                    GameZone.Hand,
                    ownerIndex == viewerIndex,
                    "player-" + viewerIndex,
                    checks);
                CheckPrivateZoneVisibility(
                    source,
                    view,
                    ownerIndex,
                    GameZone.RideDeck,
                    ownerIndex == viewerIndex,
                    "player-" + viewerIndex,
                    checks);
                CheckPublicZones(source, view, ownerIndex, "player-" + viewerIndex, checks);
            }

            CheckPrivateEvents(
                source,
                view,
                GameStateViewPerspective.Player,
                viewerIndex,
                "player-" + viewerIndex,
                checks);
        }

        private static void CheckSpectatorView(
            GameState source,
            GameState view,
            List<HiddenStateViewHardeningCheck> checks)
        {
            AddCheck(
                checks,
                "spectator-player-count",
                view != null && view.players != null && view.players.Count == source.players.Count,
                "Spectator view must preserve player count.");

            if (view == null || view.players == null)
            {
                return;
            }

            for (int ownerIndex = 0; ownerIndex < source.players.Count; ownerIndex++)
            {
                for (int i = 0; i < PrivateZones.Length; i++)
                {
                    CheckHiddenZone(source, view, ownerIndex, PrivateZones[i], "spectator", checks);
                }

                CheckPublicZones(source, view, ownerIndex, "spectator", checks);
            }

            CheckPrivateEvents(
                source,
                view,
                GameStateViewPerspective.Spectator,
                -1,
                "spectator",
                checks);
        }

        private static void CheckPrivateZoneVisibility(
            GameState source,
            GameState view,
            int ownerIndex,
            GameZone zone,
            bool ownerCanSee,
            string checkPrefix,
            List<HiddenStateViewHardeningCheck> checks)
        {
            if (!ownerCanSee)
            {
                CheckHiddenZone(source, view, ownerIndex, zone, checkPrefix, checks);
                return;
            }

            IList<GameCardInstance> sourceZone = source.GetPlayer(ownerIndex).GetZone(zone);
            IList<GameCardInstance> viewZone = view.GetPlayer(ownerIndex).GetZone(zone);
            AddCheck(
                checks,
                checkPrefix + "-p" + ownerIndex + "-" + ZoneId(zone) + "-count",
                viewZone.Count == sourceZone.Count,
                checkPrefix + " owner private zone count must match.");

            int count = Math.Min(sourceZone.Count, viewZone.Count);
            for (int i = 0; i < count; i++)
            {
                GameCardInstance sourceCard = sourceZone[i];
                GameCardInstance viewCard = viewZone[i];
                if (sourceCard == null || viewCard == null)
                {
                    AddCheck(
                        checks,
                        checkPrefix + "-p" + ownerIndex + "-" + ZoneId(zone) + "-" + i + "-null",
                        sourceCard == viewCard,
                        checkPrefix + " null card slots must match.");
                    continue;
                }

                bool shouldRemainVisible = sourceCard.face_up;
                if (shouldRemainVisible)
                {
                    AddCheck(
                        checks,
                        checkPrefix + "-p" + ownerIndex + "-" + ZoneId(zone) + "-" + i + "-owner-visible",
                        viewCard.card_id == sourceCard.card_id &&
                        viewCard.instance_id == sourceCard.instance_id &&
                        viewCard.face_up == sourceCard.face_up,
                        checkPrefix + " owner-visible private card must not be masked.");
                }
                else
                {
                    AddCheck(
                        checks,
                        checkPrefix + "-p" + ownerIndex + "-" + ZoneId(zone) + "-" + i + "-owner-face-down-hidden",
                        IsHiddenCard(viewCard, sourceCard, ownerIndex, zone, i),
                        checkPrefix + " owner face-down private card must remain hidden.");
                }
            }
        }

        private static void CheckHiddenZone(
            GameState source,
            GameState view,
            int ownerIndex,
            GameZone zone,
            string checkPrefix,
            List<HiddenStateViewHardeningCheck> checks)
        {
            IList<GameCardInstance> sourceZone = source.GetPlayer(ownerIndex).GetZone(zone);
            IList<GameCardInstance> viewZone = view.GetPlayer(ownerIndex).GetZone(zone);
            AddCheck(
                checks,
                checkPrefix + "-p" + ownerIndex + "-" + ZoneId(zone) + "-hidden-count",
                viewZone.Count == sourceZone.Count,
                checkPrefix + " hidden zone count must match.");

            int count = Math.Min(sourceZone.Count, viewZone.Count);
            for (int i = 0; i < count; i++)
            {
                GameCardInstance sourceCard = sourceZone[i];
                GameCardInstance viewCard = viewZone[i];
                AddCheck(
                    checks,
                    checkPrefix + "-p" + ownerIndex + "-" + ZoneId(zone) + "-" + i + "-hidden",
                    IsHiddenCard(viewCard, sourceCard, ownerIndex, zone, i),
                    checkPrefix + " must hide " + zone + " card at index " + i + ".");
            }
        }

        private static void CheckPublicZones(
            GameState source,
            GameState view,
            int ownerIndex,
            string checkPrefix,
            List<HiddenStateViewHardeningCheck> checks)
        {
            for (int zoneIndex = 0; zoneIndex < PublicZones.Length; zoneIndex++)
            {
                GameZone zone = PublicZones[zoneIndex];
                IList<GameCardInstance> sourceZone = source.GetPlayer(ownerIndex).GetZone(zone);
                IList<GameCardInstance> viewZone = view.GetPlayer(ownerIndex).GetZone(zone);
                AddCheck(
                    checks,
                    checkPrefix + "-p" + ownerIndex + "-" + ZoneId(zone) + "-public-count",
                    viewZone.Count == sourceZone.Count,
                    checkPrefix + " public zone count must match.");

                int count = Math.Min(sourceZone.Count, viewZone.Count);
                for (int i = 0; i < count; i++)
                {
                    GameCardInstance sourceCard = sourceZone[i];
                    GameCardInstance viewCard = viewZone[i];
                    if (sourceCard == null || viewCard == null)
                    {
                        AddCheck(
                            checks,
                            checkPrefix + "-p" + ownerIndex + "-" + ZoneId(zone) + "-" + i + "-public-null",
                            sourceCard == viewCard,
                            checkPrefix + " public null card slots must match.");
                        continue;
                    }

                    if (sourceCard.face_up)
                    {
                        AddCheck(
                            checks,
                            checkPrefix + "-p" + ownerIndex + "-" + ZoneId(zone) + "-" + i + "-public-visible",
                            viewCard.card_id == sourceCard.card_id &&
                            viewCard.instance_id == sourceCard.instance_id &&
                            viewCard.face_up == sourceCard.face_up,
                            checkPrefix + " face-up public card must remain visible.");
                    }
                    else
                    {
                        AddCheck(
                            checks,
                            checkPrefix + "-p" + ownerIndex + "-" + ZoneId(zone) + "-" + i + "-public-hidden",
                            IsHiddenCard(viewCard, sourceCard, ownerIndex, zone, i),
                            checkPrefix + " face-down public card must be hidden.");
                    }
                }
            }
        }

        private static void CheckPrivateEvents(
            GameState source,
            GameState view,
            GameStateViewPerspective perspective,
            int viewerIndex,
            string checkPrefix,
            List<HiddenStateViewHardeningCheck> checks)
        {
            int sourceCount = source.event_log == null ? 0 : source.event_log.Count;
            int viewCount = view.event_log == null ? 0 : view.event_log.Count;
            AddCheck(
                checks,
                checkPrefix + "-event-count",
                sourceCount == viewCount,
                checkPrefix + " event log count must match.");

            int count = Math.Min(sourceCount, viewCount);
            for (int i = 0; i < count; i++)
            {
                GameEvent sourceEvent = source.event_log[i];
                GameEvent viewEvent = view.event_log[i];
                if (sourceEvent == null || viewEvent == null || string.IsNullOrEmpty(sourceEvent.card_instance_id))
                {
                    continue;
                }

                bool shouldMask = ShouldMaskPrivateEvent(sourceEvent, perspective, viewerIndex);
                if (shouldMask)
                {
                    AddCheck(
                        checks,
                        checkPrefix + "-event-" + i + "-private-hidden",
                        !string.Equals(sourceEvent.card_instance_id, viewEvent.card_instance_id, StringComparison.Ordinal) &&
                        StartsWithHiddenPrefix(viewEvent.card_instance_id),
                        checkPrefix + " private event card id must be masked.");
                }
                else
                {
                    AddCheck(
                        checks,
                        checkPrefix + "-event-" + i + "-visible-allowed",
                        string.Equals(sourceEvent.card_instance_id, viewEvent.card_instance_id, StringComparison.Ordinal),
                        checkPrefix + " event card id may remain visible only when allowed.");
                }
            }
        }

        private static bool ShouldMaskPrivateEvent(
            GameEvent gameEvent,
            GameStateViewPerspective perspective,
            int viewerIndex)
        {
            bool touchesPrivateZone =
                IsPrivateZone(gameEvent.from_zone) ||
                IsPrivateZone(gameEvent.to_zone);
            if (!touchesPrivateZone)
            {
                return false;
            }

            if (perspective == GameStateViewPerspective.Spectator)
            {
                return true;
            }

            return perspective == GameStateViewPerspective.Player &&
                   gameEvent.actor_index != viewerIndex;
        }

        private static bool IsPrivateZone(GameZone zone)
        {
            return zone == GameZone.Deck ||
                   zone == GameZone.Hand ||
                   zone == GameZone.RideDeck;
        }

        private static bool IsHiddenCard(
            GameCardInstance viewCard,
            GameCardInstance sourceCard,
            int ownerIndex,
            GameZone zone,
            int index)
        {
            if (viewCard == null || sourceCard == null)
            {
                return viewCard == sourceCard;
            }

            return viewCard.card_id == GameStateViewFactory.HiddenCardId &&
                   viewCard.owner_index == ownerIndex &&
                   !viewCard.face_up &&
                   viewCard.power_delta == 0 &&
                   !string.Equals(viewCard.instance_id, sourceCard.instance_id, StringComparison.Ordinal) &&
                   string.Equals(viewCard.instance_id, MaskedInstanceId(ownerIndex, zone, index), StringComparison.Ordinal);
        }

        private static string MaskedInstanceId(int ownerIndex, GameZone zone, int index)
        {
            return "hidden-p" + ownerIndex + "-" + ZoneId(zone) + "-" + index.ToString("D4");
        }

        private static bool StartsWithHiddenPrefix(string value)
        {
            return !string.IsNullOrEmpty(value) &&
                   (value.StartsWith("hidden-p", StringComparison.Ordinal) ||
                    value.StartsWith("hidden-event-", StringComparison.Ordinal));
        }

        private static string ZoneId(GameZone zone)
        {
            return zone.ToString().ToLowerInvariant();
        }

        private static GameState CloneState(GameState state)
        {
            return GameState.FromJson(JsonUtility.ToJson(state, false));
        }

        private static void AddCheck(
            List<HiddenStateViewHardeningCheck> checks,
            string checkId,
            bool passed,
            string message)
        {
            checks.Add(new HiddenStateViewHardeningCheck
            {
                check_id = checkId ?? string.Empty,
                passed = passed,
                message = message ?? string.Empty
            });
        }

        private static HiddenStateViewHardeningVerificationResult Accept(
            List<HiddenStateViewHardeningCheck> checks)
        {
            int passed = CountPassed(checks);
            return new HiddenStateViewHardeningVerificationResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                checked_rule_count = checks == null ? 0 : checks.Count,
                passed_rule_count = passed,
                failed_rule_count = 0,
                checks = checks ?? new List<HiddenStateViewHardeningCheck>(),
                summary = "Hidden-state views passed " + passed + " masking rule check(s)."
            };
        }

        private static HiddenStateViewHardeningVerificationResult Reject(
            string rejectionReason,
            List<HiddenStateViewHardeningCheck> checks)
        {
            int passed = CountPassed(checks);
            int failed = CountFailed(checks);
            return new HiddenStateViewHardeningVerificationResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                checked_rule_count = checks == null ? 0 : checks.Count,
                passed_rule_count = passed,
                failed_rule_count = failed,
                checks = checks ?? new List<HiddenStateViewHardeningCheck>(),
                summary = "Hidden-state view hardening rejected: " + (rejectionReason ?? string.Empty)
            };
        }

        private static int CountPassed(List<HiddenStateViewHardeningCheck> checks)
        {
            if (checks == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < checks.Count; i++)
            {
                if (checks[i] != null && checks[i].passed)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountFailed(List<HiddenStateViewHardeningCheck> checks)
        {
            if (checks == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < checks.Count; i++)
            {
                if (checks[i] != null && !checks[i].passed)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool HasFailedCheck(List<HiddenStateViewHardeningCheck> checks, string checkId)
        {
            if (checks == null)
            {
                return false;
            }

            for (int i = 0; i < checks.Count; i++)
            {
                if (checks[i] != null &&
                    !checks[i].passed &&
                    string.Equals(checks[i].check_id, checkId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
