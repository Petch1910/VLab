using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public static class StructuredTargetTemplateRejectionReasons
    {
        public const string StateMissing = "STRUCTURED_TARGET_STATE_MISSING";
        public const string TargetMissing = "STRUCTURED_TARGET_MISSING";
        public const string NegativeCount = "STRUCTURED_TARGET_NEGATIVE_COUNT";
        public const string UnsupportedTargetType = "STRUCTURED_TARGET_UNSUPPORTED_TYPE";
        public const string UnsupportedZone = "STRUCTURED_TARGET_UNSUPPORTED_ZONE";
        public const string HiddenZone = "STRUCTURED_TARGET_HIDDEN_ZONE";
        public const string TargetCountUnavailable = "STRUCTURED_TARGET_COUNT_UNAVAILABLE";
    }

    [Serializable]
    public sealed class StructuredTargetCandidate
    {
        public int player_index;
        public string zone;
        public string instance_id;
        public string card_id;
        public bool face_up;
    }

    [Serializable]
    public sealed class StructuredTargetTemplateResult
    {
        public bool accepted;
        public string rejection_reason;
        public bool requires_manual_resolution;
        public int requested_count;
        public List<StructuredTargetCandidate> candidates = new List<StructuredTargetCandidate>();
        public string summary;

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static StructuredTargetTemplateResult FromJson(string json)
        {
            StructuredTargetTemplateResult result =
                JsonUtility.FromJson<StructuredTargetTemplateResult>(json);
            if (result == null)
            {
                throw new ArgumentException("Structured target template result JSON could not be parsed.", "json");
            }

            result.EnsureLists();
            return result;
        }

        public void EnsureLists()
        {
            if (candidates == null)
            {
                candidates = new List<StructuredTargetCandidate>();
            }
        }
    }

    public static class StructuredTargetTemplate
    {
        public static StructuredTargetTemplateResult Resolve(
            GameState state,
            int playerIndex,
            StructuredAbilityTarget target)
        {
            if (state == null)
            {
                return Reject(StructuredTargetTemplateRejectionReasons.StateMissing, false, 0);
            }

            if (target == null)
            {
                return Reject(StructuredTargetTemplateRejectionReasons.TargetMissing, false, 0);
            }

            target.EnsureLists();
            if (target.count < 0)
            {
                return Reject(StructuredTargetTemplateRejectionReasons.NegativeCount, false, target.count);
            }

            string targetType = target.type ?? string.Empty;
            if (targetType == "none")
            {
                return Accept(new List<StructuredTargetCandidate>(), target.count, "No target required.");
            }

            if (targetType == "circle")
            {
                return Reject(StructuredTargetTemplateRejectionReasons.UnsupportedTargetType + ": circle", true, target.count);
            }

            if (targetType != "self" && targetType != "unit" && targetType != "card")
            {
                return Reject(StructuredTargetTemplateRejectionReasons.UnsupportedTargetType + ": " + targetType, true, target.count);
            }

            if (!TryParseZone(target.zone, out GameZone zone))
            {
                return Reject(StructuredTargetTemplateRejectionReasons.UnsupportedZone + ": " + (target.zone ?? string.Empty), true, target.count);
            }

            if (IsHiddenOrUnsupportedZone(target.owner, target.zone))
            {
                return Reject(StructuredTargetTemplateRejectionReasons.HiddenZone + ": " + (target.zone ?? string.Empty), true, target.count);
            }

            List<int> owners = ResolveOwners(state, playerIndex, target.owner);
            var candidates = new List<StructuredTargetCandidate>();
            for (int ownerIndex = 0; ownerIndex < owners.Count; ownerIndex++)
            {
                AddCandidates(state, owners[ownerIndex], zone, candidates);
            }

            int requestedCount = Math.Max(0, target.count);
            if (requestedCount > 0 && candidates.Count > requestedCount)
            {
                candidates.RemoveRange(requestedCount, candidates.Count - requestedCount);
            }

            if (!target.optional && target.count > 0 && candidates.Count < target.count)
            {
                return Reject(
                    StructuredTargetTemplateRejectionReasons.TargetCountUnavailable,
                    false,
                    target.count,
                    candidates);
            }

            return Accept(
                candidates,
                target.count,
                "Structured target template resolved " + candidates.Count + " candidate(s).");
        }

        private static void AddCandidates(
            GameState state,
            int ownerIndex,
            GameZone zone,
            List<StructuredTargetCandidate> candidates)
        {
            PlayerGameState player = state.GetPlayer(ownerIndex);
            List<GameCardInstance> cards = player.GetZone(zone);
            for (int i = 0; i < cards.Count; i++)
            {
                GameCardInstance card = cards[i];
                if (card == null || string.IsNullOrEmpty(card.instance_id))
                {
                    continue;
                }

                if (!card.face_up)
                {
                    continue;
                }

                candidates.Add(new StructuredTargetCandidate
                {
                    player_index = ownerIndex,
                    zone = zone.ToString(),
                    instance_id = card.instance_id,
                    card_id = card.card_id ?? string.Empty,
                    face_up = card.face_up
                });
            }
        }

        private static List<int> ResolveOwners(GameState state, int playerIndex, string owner)
        {
            var owners = new List<int>();
            string safeOwner = owner ?? string.Empty;
            if (safeOwner == "opponent")
            {
                for (int i = 0; i < state.players.Count; i++)
                {
                    if (i != playerIndex)
                    {
                        owners.Add(i);
                    }
                }
            }
            else if (safeOwner == "any")
            {
                for (int i = 0; i < state.players.Count; i++)
                {
                    owners.Add(i);
                }
            }
            else
            {
                owners.Add(playerIndex);
            }

            return owners;
        }

        private static bool IsHiddenOrUnsupportedZone(string owner, string zone)
        {
            string safeZone = zone ?? string.Empty;
            if (safeZone == "Soul" || safeZone == "GZone" || safeZone == "Deck")
            {
                return true;
            }

            if ((owner ?? string.Empty) != "self" && (safeZone == "Hand" || safeZone == "RideDeck"))
            {
                return true;
            }

            return false;
        }

        private static bool TryParseZone(string value, out GameZone zone)
        {
            return Enum.TryParse(value ?? string.Empty, false, out zone);
        }

        private static StructuredTargetTemplateResult Accept(
            List<StructuredTargetCandidate> candidates,
            int requestedCount,
            string summary)
        {
            return new StructuredTargetTemplateResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                requires_manual_resolution = false,
                requested_count = requestedCount,
                candidates = CloneCandidates(candidates),
                summary = summary ?? string.Empty
            };
        }

        private static StructuredTargetTemplateResult Reject(
            string rejectionReason,
            bool requiresManualResolution,
            int requestedCount,
            List<StructuredTargetCandidate> candidates = null)
        {
            return new StructuredTargetTemplateResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                requires_manual_resolution = requiresManualResolution,
                requested_count = requestedCount,
                candidates = CloneCandidates(candidates),
                summary = "Structured target template rejected: " + (rejectionReason ?? string.Empty)
            };
        }

        private static List<StructuredTargetCandidate> CloneCandidates(List<StructuredTargetCandidate> candidates)
        {
            if (candidates == null)
            {
                return new List<StructuredTargetCandidate>();
            }

            var result = new List<StructuredTargetCandidate>();
            for (int i = 0; i < candidates.Count; i++)
            {
                StructuredTargetCandidate candidate = candidates[i];
                if (candidate == null)
                {
                    continue;
                }

                result.Add(JsonUtility.FromJson<StructuredTargetCandidate>(JsonUtility.ToJson(candidate, false)));
            }

            return result;
        }
    }
}
