using System;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class PendingAutoAbilityResolutionRequest
    {
        public int selected_index = -1;
        public string pending_id;
        public int player_index;
        public string timing_event;
        public string source_card_instance_id;
        public string source_card_id;
        public bool hides_source_card_identity;
        public string summary;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static PendingAutoAbilityResolutionRequest FromJson(string json)
        {
            PendingAutoAbilityResolutionRequest request =
                JsonUtility.FromJson<PendingAutoAbilityResolutionRequest>(json);
            if (request == null)
            {
                throw new ArgumentException("Pending auto ability resolution request JSON could not be parsed.", "json");
            }

            return request;
        }
    }

    public sealed class PendingAutoAbilityResolutionRequestResult
    {
        public bool accepted;
        public string rejection_reason;
        public PendingAutoAbilityResolutionRequest request;
    }

    public static class PendingAutoAbilityResolutionRequestFactory
    {
        public const string SelectionMissingReason = "PENDING_AUTO_ABILITY_SELECTION_MISSING";

        public static PendingAutoAbilityResolutionRequestResult Create(PendingAutoAbilitySelectionState selection)
        {
            if (selection == null || !selection.accepted || !selection.has_selection || selection.selected_ability == null)
            {
                return Reject(SelectionMissingReason);
            }

            PendingAutoAbility ability = PendingAutoAbility.FromJson(selection.selected_ability.ToJson());
            return new PendingAutoAbilityResolutionRequestResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                request = new PendingAutoAbilityResolutionRequest
                {
                    selected_index = selection.selected_index,
                    pending_id = ability.pending_id ?? string.Empty,
                    player_index = ability.player_index,
                    timing_event = ability.timing_event ?? string.Empty,
                    source_card_instance_id = SanitizeSourceInstanceId(ability),
                    source_card_id = SanitizeSourceCardId(ability),
                    hides_source_card_identity = ability.hides_source_card_identity ||
                                                 ability.source_card_id == GameStateViewFactory.HiddenCardId,
                    summary = ability.summary ?? string.Empty
                }
            };
        }

        private static string SanitizeSourceInstanceId(PendingAutoAbility ability)
        {
            if (ability.hides_source_card_identity || ability.source_card_id == GameStateViewFactory.HiddenCardId)
            {
                return string.Empty;
            }

            return ability.source_card_instance_id ?? string.Empty;
        }

        private static string SanitizeSourceCardId(PendingAutoAbility ability)
        {
            if (ability.hides_source_card_identity || ability.source_card_id == GameStateViewFactory.HiddenCardId)
            {
                return GameStateViewFactory.HiddenCardId;
            }

            return ability.source_card_id ?? string.Empty;
        }

        private static PendingAutoAbilityResolutionRequestResult Reject(string rejectionReason)
        {
            return new PendingAutoAbilityResolutionRequestResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                request = null
            };
        }
    }
}
