using System;
using System.Collections.Generic;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Bots
{
    public sealed class HeuristicBotV2Options
    {
        public BoardEvaluationWeights BoardWeights = BoardEvaluationWeights.CreateDefault();
        public double DrawBias = 0.2d;
        public double EmptyVanguardRideBias = 2.0d;
        public double VanguardMoveBias = 0.4d;
        public double RearGuardCallBias = 0.35d;
        public double HarmfulMovePenalty = -2.0d;
        public double MainPhaseBias = 0.05d;
        public double BattlePhaseBias = 0.15d;
        public double EndPhaseBias = -0.05d;
        public double GiftMarkerBias = -0.25d;

        public static HeuristicBotV2Options CreateDefault()
        {
            return new HeuristicBotV2Options();
        }
    }

    public sealed class HeuristicBotActionEvaluation
    {
        public LegalGameAction Action { get; private set; }
        public bool Accepted { get; private set; }
        public string RejectionReason { get; private set; }
        public double OwnScore { get; private set; }
        public double OpponentScore { get; private set; }
        public double ActionBias { get; private set; }
        public double TotalScore { get; private set; }
        public string Summary { get; private set; }

        public HeuristicBotActionEvaluation(
            LegalGameAction action,
            bool accepted,
            string rejectionReason,
            double ownScore,
            double opponentScore,
            double actionBias,
            double totalScore,
            string summary)
        {
            Action = CloneAction(action);
            Accepted = accepted;
            RejectionReason = rejectionReason ?? string.Empty;
            OwnScore = ownScore;
            OpponentScore = opponentScore;
            ActionBias = actionBias;
            TotalScore = totalScore;
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

    public static class HeuristicBotV2
    {
        public static BotDecision DecideNext(
            GameState state,
            int playerIndex,
            ICardRepository cardRepository = null,
            HeuristicBotV2Options options = null)
        {
            return DecideNext(
                BotDecisionContextFactory.Create(state, playerIndex),
                cardRepository,
                options);
        }

        public static BotDecision DecideNext(
            BotDecisionContext context,
            ICardRepository cardRepository = null,
            HeuristicBotV2Options options = null)
        {
            IReadOnlyList<HeuristicBotActionEvaluation> evaluations =
                EvaluateLegalActions(context, cardRepository, options);

            HeuristicBotActionEvaluation best = null;
            for (int i = 0; i < evaluations.Count; i++)
            {
                HeuristicBotActionEvaluation candidate = evaluations[i];
                if (!candidate.Accepted)
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
                return new BotDecision(null, "heuristic-v2: no executable legal actions");
            }

            return new BotDecision(
                CloneAction(best.Action),
                "heuristic-v2: " + best.Summary);
        }

        public static IReadOnlyList<HeuristicBotActionEvaluation> EvaluateLegalActions(
            GameState state,
            int playerIndex,
            ICardRepository cardRepository = null,
            HeuristicBotV2Options options = null)
        {
            return EvaluateLegalActions(
                BotDecisionContextFactory.Create(state, playerIndex),
                cardRepository,
                options);
        }

        public static IReadOnlyList<HeuristicBotActionEvaluation> EvaluateLegalActions(
            BotDecisionContext context,
            ICardRepository cardRepository = null,
            HeuristicBotV2Options options = null)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            HeuristicBotV2Options safeOptions = options ?? HeuristicBotV2Options.CreateDefault();
            GameState state = context.MaskedState;
            int playerIndex = context.PlayerIndex;
            IReadOnlyList<LegalGameAction> legalActions = context.LegalActions;
            var evaluations = new List<HeuristicBotActionEvaluation>(legalActions.Count);

            for (int i = 0; i < legalActions.Count; i++)
            {
                LegalGameAction action = legalActions[i];
                GameState branch = GameStateViewFactory.CreateTrueStateClone(state);
                RulesCommandResult result = RulesCore.TryExecute(branch, action);
                if (!result.accepted)
                {
                    evaluations.Add(new HeuristicBotActionEvaluation(
                        action,
                        false,
                        result.rejection_reason,
                        0d,
                        0d,
                        0d,
                        double.NegativeInfinity,
                        DescribeAction(action) + " rejected"));
                    continue;
                }

                GameState evaluationView = CreateEvaluationView(branch, playerIndex, result.game_event);
                BoardResourceEvaluation ownEvaluation = BoardResourceEvaluator.Evaluate(
                    evaluationView,
                    playerIndex,
                    cardRepository,
                    safeOptions.BoardWeights);
                double opponentScore = EvaluateOpponentScore(
                    evaluationView,
                    playerIndex,
                    cardRepository,
                    safeOptions.BoardWeights);
                double actionBias = CalculateActionBias(state, action, playerIndex, safeOptions);
                double totalScore = ownEvaluation.TotalScore - opponentScore + actionBias;
                string summary =
                    DescribeAction(action) +
                    " score=" + totalScore.ToString("0.###") +
                    " own=" + ownEvaluation.TotalScore.ToString("0.###") +
                    " opp=" + opponentScore.ToString("0.###") +
                    " bias=" + actionBias.ToString("0.###");

                evaluations.Add(new HeuristicBotActionEvaluation(
                    action,
                    true,
                    string.Empty,
                    ownEvaluation.TotalScore,
                    opponentScore,
                    actionBias,
                    totalScore,
                    summary));
            }

            return evaluations;
        }

        private static GameState CreateEvaluationView(GameState branch, int playerIndex, GameEvent resultingEvent)
        {
            GameState view = GameStateViewFactory.CreatePlayerView(branch, playerIndex);
            if (resultingEvent == null ||
                resultingEvent.action_type != GameActionType.Draw ||
                string.IsNullOrEmpty(resultingEvent.card_instance_id))
            {
                return view;
            }

            MaskCardInZone(
                view.GetPlayer(playerIndex).hand,
                resultingEvent.card_instance_id,
                playerIndex,
                GameZone.Hand);
            return view;
        }

        private static void MaskCardInZone(
            IList<GameCardInstance> cards,
            string instanceId,
            int ownerIndex,
            GameZone zone)
        {
            if (cards == null)
            {
                return;
            }

            for (int i = 0; i < cards.Count; i++)
            {
                GameCardInstance card = cards[i];
                if (card == null || card.instance_id != instanceId)
                {
                    continue;
                }

                card.instance_id = "hidden-p" + ownerIndex + "-" + zone.ToString().ToLowerInvariant() + "-draw";
                card.card_id = GameStateViewFactory.HiddenCardId;
                card.owner_index = ownerIndex;
                card.face_up = false;
                card.power_delta = 0;
                return;
            }
        }

        private static double EvaluateOpponentScore(
            GameState evaluationView,
            int playerIndex,
            ICardRepository cardRepository,
            BoardEvaluationWeights weights)
        {
            int opponentIndex = FindOpponentIndex(evaluationView, playerIndex);
            if (opponentIndex < 0)
            {
                return 0d;
            }

            return BoardResourceEvaluator.Evaluate(
                evaluationView,
                opponentIndex,
                cardRepository,
                weights).TotalScore;
        }

        private static int FindOpponentIndex(GameState state, int playerIndex)
        {
            state.EnsureLists();
            for (int i = 0; i < state.players.Count; i++)
            {
                if (i != playerIndex)
                {
                    return i;
                }
            }

            return -1;
        }

        private static double CalculateActionBias(
            GameState sourceState,
            LegalGameAction action,
            int playerIndex,
            HeuristicBotV2Options options)
        {
            if (action == null)
            {
                return 0d;
            }

            switch (action.action_type)
            {
                case GameActionType.Draw:
                    return options.DrawBias;
                case GameActionType.MoveCard:
                    return CalculateMoveBias(sourceState, action, playerIndex, options);
                case GameActionType.SetPhase:
                    return CalculatePhaseBias(action.phase, options);
                case GameActionType.AddGiftMarker:
                    return options.GiftMarkerBias;
                case GameActionType.ResourceFlip:
                    return 0d;
                default:
                    return 0d;
            }
        }

        private static double CalculateMoveBias(
            GameState sourceState,
            LegalGameAction action,
            int playerIndex,
            HeuristicBotV2Options options)
        {
            if (action.from_zone == GameZone.Hand && action.to_zone == GameZone.Vanguard)
            {
                bool hasNoVanguard = sourceState.GetPlayer(playerIndex).vanguard.Count == 0;
                return hasNoVanguard ? options.EmptyVanguardRideBias : options.VanguardMoveBias;
            }

            if (action.from_zone == GameZone.Hand && action.to_zone == GameZone.RearGuard)
            {
                return options.RearGuardCallBias;
            }

            if (action.to_zone == GameZone.Drop || action.to_zone == GameZone.Damage)
            {
                return options.HarmfulMovePenalty;
            }

            return 0d;
        }

        private static double CalculatePhaseBias(GamePhase phase, HeuristicBotV2Options options)
        {
            switch (phase)
            {
                case GamePhase.Main:
                    return options.MainPhaseBias;
                case GamePhase.Battle:
                    return options.BattlePhaseBias;
                case GamePhase.End:
                    return options.EndPhaseBias;
                default:
                    return 0d;
            }
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
