using System;
using System.Collections.Generic;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Bots
{
    public sealed class PlaybookIntegrationOptions
    {
        public double PriorityCallBias = 3.0d;
        public double AggroBattlePhaseBias = 0.4d;

        public static PlaybookIntegrationOptions CreateDefault()
        {
            return new PlaybookIntegrationOptions();
        }
    }

    public sealed class PlaybookActionEvaluation
    {
        public LegalGameAction Action { get; private set; }
        public double BaseScore { get; private set; }
        public double PlaybookBias { get; private set; }
        public double TotalScore { get; private set; }
        public string PlaybookId { get; private set; }
        public string Summary { get; private set; }

        public PlaybookActionEvaluation(
            LegalGameAction action,
            double baseScore,
            double playbookBias,
            double totalScore,
            string playbookId,
            string summary)
        {
            Action = CloneAction(action);
            BaseScore = baseScore;
            PlaybookBias = playbookBias;
            TotalScore = totalScore;
            PlaybookId = playbookId ?? string.Empty;
            Summary = summary ?? string.Empty;
        }

        private static LegalGameAction CloneAction(LegalGameAction action)
        {
            if (action == null)
            {
                return null;
            }

            return new LegalGameAction
            {
                label = action.label,
                action_type = action.action_type,
                actor_index = action.actor_index,
                card_instance_id = action.card_instance_id,
                target_card_instance_id = action.target_card_instance_id,
                from_zone = action.from_zone,
                to_zone = action.to_zone,
                phase = action.phase,
                gift_marker_type = action.gift_marker_type,
                marker_delta = action.marker_delta,
                resource_operation_type = action.resource_operation_type,
                resource_delta = action.resource_delta,
                trigger_check_source = action.trigger_check_source,
                card_instance_ids = action.card_instance_ids == null
                    ? null
                    : new List<string>(action.card_instance_ids)
            };
        }
    }

    public static class PlaybookIntegratedBot
    {
        public static BotDecision DecideNext(
            GameState state,
            int playerIndex,
            ICardRepository cardRepository,
            BotPlaybookLibrary playbookLibrary,
            HeuristicBotV2Options heuristicOptions = null,
            PlaybookIntegrationOptions playbookOptions = null)
        {
            return DecideNext(
                BotDecisionContextFactory.Create(state, playerIndex),
                cardRepository,
                playbookLibrary,
                heuristicOptions,
                playbookOptions);
        }

        public static BotDecision DecideNext(
            BotDecisionContext context,
            ICardRepository cardRepository,
            BotPlaybookLibrary playbookLibrary,
            HeuristicBotV2Options heuristicOptions = null,
            PlaybookIntegrationOptions playbookOptions = null)
        {
            IReadOnlyList<PlaybookActionEvaluation> evaluations = EvaluateActions(
                context,
                cardRepository,
                playbookLibrary,
                heuristicOptions,
                playbookOptions);

            PlaybookActionEvaluation best = null;
            for (int i = 0; i < evaluations.Count; i++)
            {
                PlaybookActionEvaluation candidate = evaluations[i];
                if (candidate.Action == null)
                {
                    continue;
                }

                if (best == null || candidate.TotalScore > best.TotalScore)
                {
                    best = candidate;
                }
            }

            if (best == null)
            {
                return new BotDecision(null, "playbook-bot: no executable legal actions");
            }

            return new BotDecision(
                CloneAction(best.Action),
                "playbook-bot: " + best.Summary);
        }

        public static IReadOnlyList<PlaybookActionEvaluation> EvaluateActions(
            GameState state,
            int playerIndex,
            ICardRepository cardRepository,
            BotPlaybookLibrary playbookLibrary,
            HeuristicBotV2Options heuristicOptions = null,
            PlaybookIntegrationOptions playbookOptions = null)
        {
            return EvaluateActions(
                BotDecisionContextFactory.Create(state, playerIndex),
                cardRepository,
                playbookLibrary,
                heuristicOptions,
                playbookOptions);
        }

        public static IReadOnlyList<PlaybookActionEvaluation> EvaluateActions(
            BotDecisionContext context,
            ICardRepository cardRepository,
            BotPlaybookLibrary playbookLibrary,
            HeuristicBotV2Options heuristicOptions = null,
            PlaybookIntegrationOptions playbookOptions = null)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            PlaybookIntegrationOptions safePlaybookOptions =
                playbookOptions ?? PlaybookIntegrationOptions.CreateDefault();
            GameState state = context.MaskedState;
            int playerIndex = context.PlayerIndex;
            BotPlaybook playbook = MatchPlaybook(state, playerIndex, playbookLibrary);
            IReadOnlyList<HeuristicBotActionEvaluation> baseEvaluations =
                HeuristicBotV2.EvaluateLegalActions(context, cardRepository, heuristicOptions);
            var evaluations = new List<PlaybookActionEvaluation>(baseEvaluations.Count);

            for (int i = 0; i < baseEvaluations.Count; i++)
            {
                HeuristicBotActionEvaluation baseEvaluation = baseEvaluations[i];
                if (!baseEvaluation.Accepted)
                {
                    continue;
                }

                double bias = CalculatePlaybookBias(
                    state,
                    playerIndex,
                    baseEvaluation.Action,
                    playbook,
                    safePlaybookOptions);
                double total = baseEvaluation.TotalScore + bias;
                string summary =
                    DescribeAction(baseEvaluation.Action) +
                    " playbook=" + SafePlaybookId(playbook) +
                    " base=" + baseEvaluation.TotalScore.ToString("0.###") +
                    " bias=" + bias.ToString("0.###") +
                    " total=" + total.ToString("0.###");

                evaluations.Add(new PlaybookActionEvaluation(
                    baseEvaluation.Action,
                    baseEvaluation.TotalScore,
                    bias,
                    total,
                    SafePlaybookId(playbook),
                    summary));
            }

            return evaluations;
        }

        private static BotPlaybook MatchPlaybook(
            GameState state,
            int playerIndex,
            BotPlaybookLibrary playbookLibrary)
        {
            if (playbookLibrary == null)
            {
                return BotPlaybook.CreateDefault();
            }

            GameState playerView = GameStateViewFactory.CreatePlayerView(state, playerIndex);
            return playbookLibrary.MatchFromState(playerView, playerIndex);
        }

        private static double CalculatePlaybookBias(
            GameState state,
            int playerIndex,
            LegalGameAction action,
            BotPlaybook playbook,
            PlaybookIntegrationOptions options)
        {
            if (action == null || playbook == null)
            {
                return 0d;
            }

            playbook.EnsureLists();
            if (action.action_type == GameActionType.MoveCard &&
                action.from_zone == GameZone.Hand &&
                action.to_zone == GameZone.RearGuard &&
                IsPriorityCall(state, playerIndex, action.card_instance_id, playbook))
            {
                return options.PriorityCallBias;
            }

            if (action.action_type == GameActionType.SetPhase &&
                action.phase == GamePhase.Battle &&
                playbook.preferred_profile == BotProfileType.Aggro)
            {
                return options.AggroBattlePhaseBias;
            }

            return 0d;
        }

        private static bool IsPriorityCall(
            GameState state,
            int playerIndex,
            string cardInstanceId,
            BotPlaybook playbook)
        {
            if (string.IsNullOrEmpty(cardInstanceId))
            {
                return false;
            }

            string cardId = FindVisibleOwnHandCardId(state, playerIndex, cardInstanceId);
            if (string.IsNullOrEmpty(cardId))
            {
                return false;
            }

            for (int i = 0; i < playbook.priority_call_card_ids.Count; i++)
            {
                if (playbook.priority_call_card_ids[i] == cardId)
                {
                    return true;
                }
            }

            return false;
        }

        private static string FindVisibleOwnHandCardId(
            GameState state,
            int playerIndex,
            string cardInstanceId)
        {
            PlayerGameState player = state.GetPlayer(playerIndex);
            for (int i = 0; i < player.hand.Count; i++)
            {
                GameCardInstance card = player.hand[i];
                if (card != null &&
                    card.instance_id == cardInstanceId &&
                    card.face_up &&
                    !string.IsNullOrEmpty(card.card_id) &&
                    card.card_id != GameStateViewFactory.HiddenCardId)
                {
                    return card.card_id;
                }
            }

            return string.Empty;
        }

        private static string SafePlaybookId(BotPlaybook playbook)
        {
            if (playbook == null || string.IsNullOrEmpty(playbook.playbook_id))
            {
                return "default_balanced";
            }

            return playbook.playbook_id;
        }

        private static string DescribeAction(LegalGameAction action)
        {
            if (action == null)
            {
                return "None";
            }

            switch (action.action_type)
            {
                case GameActionType.Draw:
                    return "Draw";
                case GameActionType.MoveCard:
                    return "MoveCard " + action.from_zone + "->" + action.to_zone;
                case GameActionType.SetPhase:
                    return "SetPhase " + action.phase;
                case GameActionType.AddGiftMarker:
                    return "AddGiftMarker " + action.gift_marker_type;
                case GameActionType.ResourceFlip:
                    return "ResourceFlip " + action.resource_operation_type;
                default:
                    return action.action_type.ToString();
            }
        }

        private static LegalGameAction CloneAction(LegalGameAction action)
        {
            if (action == null)
            {
                return null;
            }

            return new LegalGameAction
            {
                label = action.label,
                action_type = action.action_type,
                actor_index = action.actor_index,
                card_instance_id = action.card_instance_id,
                from_zone = action.from_zone,
                to_zone = action.to_zone,
                phase = action.phase,
                gift_marker_type = action.gift_marker_type,
                marker_delta = action.marker_delta,
                resource_operation_type = action.resource_operation_type,
                resource_delta = action.resource_delta
            };
        }
    }
}
