using System.Collections.Generic;
using System.Text;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public static class PlayTableSelectedCardPreviewFormatter
    {
        public const string NoSelectionText = "Select a card to inspect it.";
        public const string MissingCardText = "Selected card is unavailable.";
        public const string HiddenDetailsText = "Details are hidden from this viewer.";
        public const string NoCardSpecificActionText = "Legal now: no card-specific action.";

        public static string Format(
            GameCardInstance card,
            GameZone zone,
            CardDetail detail,
            string actionHint)
        {
            if (card == null)
            {
                return NoSelectionText;
            }

            if (IsHidden(card))
            {
                return "Hidden card\nZone: " + zone + "\n" + HiddenDetailsText;
            }

            string cardId = FirstNonEmpty(card.card_id, MissingCardText);
            string name = detail == null ? "Unknown in loaded pack" : FirstNonEmpty(detail.NameTh, detail.CardId, cardId);
            StringBuilder builder = new StringBuilder();
            builder.Append("Card ID: ").Append(cardId).Append('\n');
            builder.Append("Name: ").Append(name).Append('\n');
            builder.Append("Zone: ").Append(zone).Append('\n');
            if (detail != null)
            {
                builder.Append("Type: ").Append(JoinKnown(detail.Type1, detail.Type2)).Append('\n');
                builder.Append("Grade: ").Append(FormatNullable(detail.Grade)).Append(" | ");
                builder.Append("Power: ").Append(FormatNullable(detail.Power)).Append(" | ");
                builder.Append("Shield: ").Append(FormatNullable(detail.Shield)).Append('\n');
                builder.Append("Trigger: ").Append(FirstNonEmpty(detail.Trigger, "-")).Append('\n');
                builder.Append("Clan/Nation: ")
                    .Append(JoinKnown(detail.Clan, detail.Nation, detail.Nation2))
                    .Append('\n');
            }
            else
            {
                builder.Append("Type: -\n");
                builder.Append("Grade: - | Power: - | Shield: -\n");
            }

            builder.Append(FirstNonEmpty(actionHint, NoCardSpecificActionText)).Append("\n\n");
            if (detail != null)
            {
                builder.Append(FirstNonEmpty(detail.TextTh, "No Thai skill text in runtime pack."));
            }
            else
            {
                builder.Append("Runtime card detail is not loaded.");
            }

            return builder.ToString();
        }

        public static string FormatActionHint(
            IEnumerable<LegalGameAction> legalActions,
            string selectedCardInstanceId)
        {
            if (string.IsNullOrWhiteSpace(selectedCardInstanceId) || legalActions == null)
            {
                return NoCardSpecificActionText;
            }

            List<string> labels = new List<string>();
            foreach (LegalGameAction action in legalActions)
            {
                if (action == null || action.card_instance_id != selectedCardInstanceId)
                {
                    continue;
                }

                string label = FormatActionLabel(action);
                if (string.IsNullOrWhiteSpace(label) || labels.Contains(label))
                {
                    continue;
                }

                labels.Add(label);
                if (labels.Count >= 4)
                {
                    break;
                }
            }

            if (labels.Count == 0)
            {
                return NoCardSpecificActionText;
            }

            return "Legal now: " + string.Join(", ", labels.ToArray()) + ".";
        }

        private static string FormatActionLabel(LegalGameAction action)
        {
            switch (action.action_type)
            {
                case GameActionType.MoveCard:
                    if (action.from_zone == GameZone.Hand && action.to_zone == GameZone.Vanguard)
                    {
                        return "Ride to Vanguard";
                    }

                    if (action.from_zone == GameZone.Hand && action.to_zone == GameZone.RearGuard)
                    {
                        return "Call to Rear-guard";
                    }

                    if (action.from_zone == GameZone.Hand && action.to_zone == GameZone.Drop)
                    {
                        return "Discard";
                    }

                    return "Move to " + action.to_zone;
                case GameActionType.DeclareAttack:
                    return "Attack";
                case GameActionType.Guard:
                    return "Guard";
                case GameActionType.ResourceFlip:
                    return action.resource_operation_type.ToString();
                case GameActionType.TriggerCheck:
                    return "Trigger check";
                case GameActionType.MulliganCards:
                    return "Mulligan";
                default:
                    return string.Empty;
            }
        }

        private static bool IsHidden(GameCardInstance card)
        {
            return card == null ||
                   !card.face_up ||
                   card.card_id == GameStateViewFactory.HiddenCardId;
        }

        private static string FormatNullable(int? value)
        {
            return value.HasValue ? value.Value.ToString() : "-";
        }

        private static string JoinKnown(params string[] values)
        {
            if (values == null)
            {
                return "-";
            }

            List<string> known = new List<string>();
            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value) && !known.Contains(value))
                {
                    known.Add(value);
                }
            }

            return known.Count == 0 ? "-" : string.Join(" / ", known.ToArray());
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return string.Empty;
        }
    }
}
