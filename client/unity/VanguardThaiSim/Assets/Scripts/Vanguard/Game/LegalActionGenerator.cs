using System.Collections.Generic;

namespace VanguardThaiSim.Game
{
    public static class LegalActionGenerator
    {
        public static IReadOnlyList<LegalGameAction> Generate(GameState state, int playerIndex)
        {
            PlayerGameState player = state.GetPlayer(playerIndex);
            List<LegalGameAction> actions = new List<LegalGameAction>();

            if (player.deck.Count > 0)
            {
                actions.Add(new LegalGameAction
                {
                    label = "Draw",
                    action_type = GameActionType.Draw,
                    actor_index = playerIndex,
                    from_zone = GameZone.Deck,
                    to_zone = GameZone.Hand
                });
            }

            AddMoveActions(actions, playerIndex, player.hand, GameZone.Hand, GameZone.Vanguard, "Ride/call to vanguard");
            AddMoveActions(actions, playerIndex, player.hand, GameZone.Hand, GameZone.RearGuard, "Call to rear-guard");
            AddMoveActions(actions, playerIndex, player.hand, GameZone.Hand, GameZone.Drop, "Discard");
            AddMoveActions(actions, playerIndex, player.hand, GameZone.Hand, GameZone.Damage, "Move hand to damage");
            AddMoveActions(actions, playerIndex, player.vanguard, GameZone.Vanguard, GameZone.Drop, "Move vanguard to drop");
            AddMoveActions(actions, playerIndex, player.vanguard, GameZone.Vanguard, GameZone.Damage, "Move vanguard to damage");
            AddMoveActions(actions, playerIndex, player.vanguard, GameZone.Vanguard, GameZone.RearGuard, "Move vanguard to rear-guard");
            AddMoveActions(actions, playerIndex, player.rear_guard, GameZone.RearGuard, GameZone.Drop, "Retire rear-guard");
            AddMoveActions(actions, playerIndex, player.rear_guard, GameZone.RearGuard, GameZone.Damage, "Move rear-guard to damage");
            AddMoveActions(actions, playerIndex, player.rear_guard, GameZone.RearGuard, GameZone.Vanguard, "Move rear-guard to vanguard");
            AddMoveActions(actions, playerIndex, player.damage, GameZone.Damage, GameZone.Drop, "Heal damage to drop");
            AddMoveActions(actions, playerIndex, player.drop, GameZone.Drop, GameZone.Vanguard, "Move drop to vanguard");
            AddMoveActions(actions, playerIndex, player.drop, GameZone.Drop, GameZone.RearGuard, "Move drop to rear-guard");
            AddMoveActions(actions, playerIndex, player.drop, GameZone.Drop, GameZone.Damage, "Move drop to damage");
            AddMoveActions(actions, playerIndex, player.bind, GameZone.Bind, GameZone.Vanguard, "Move bind to vanguard");
            AddMoveActions(actions, playerIndex, player.bind, GameZone.Bind, GameZone.RearGuard, "Move bind to rear-guard");
            AddMoveActions(actions, playerIndex, player.bind, GameZone.Bind, GameZone.Drop, "Move bind to drop");
            AddMoveActions(actions, playerIndex, player.bind, GameZone.Bind, GameZone.Damage, "Move bind to damage");
            if (state.phase == GamePhase.Mulligan && player.vanguard.Count == 0)
            {
                AddMoveActions(actions, playerIndex, player.ride_deck, GameZone.RideDeck, GameZone.Vanguard, "Set first vanguard");
            }

            AddTopDeckMoveAction(actions, playerIndex, player.deck, GameZone.Deck, GameZone.Soul, "Soul-Charge top card");
            AddMoveActions(actions, playerIndex, player.soul, GameZone.Soul, GameZone.Drop, "Soul-Blast");
            AddResourceFlipActions(actions, playerIndex, player.damage);

            AddPhaseAction(actions, state, playerIndex, GamePhase.StandAndDraw);
            AddPhaseAction(actions, state, playerIndex, GamePhase.Ride);
            AddPhaseAction(actions, state, playerIndex, GamePhase.Main);
            AddPhaseAction(actions, state, playerIndex, GamePhase.Battle);
            AddPhaseAction(actions, state, playerIndex, GamePhase.End);

            AddGiftMarkerAction(actions, playerIndex, GiftMarkerType.Force);
            AddGiftMarkerAction(actions, playerIndex, GiftMarkerType.Accel);
            AddGiftMarkerAction(actions, playerIndex, GiftMarkerType.Protect);

            if (state.phase == GamePhase.Mulligan)
            {
                actions.Add(new LegalGameAction
                {
                    label = "Keep opening hand",
                    action_type = GameActionType.MulliganCards,
                    actor_index = playerIndex,
                    card_instance_ids = new List<string>()
                });

                foreach (GameCardInstance card in player.hand)
                {
                    actions.Add(new LegalGameAction
                    {
                        label = "Mulligan " + card.card_id,
                        action_type = GameActionType.MulliganCards,
                        actor_index = playerIndex,
                        card_instance_ids = new List<string> { card.instance_id }
                    });
                }
            }

            if (state.phase == GamePhase.Battle)
            {
                int opponentIndex = 1 - playerIndex;
                if (opponentIndex >= 0 && opponentIndex < state.players.Count)
                {
                    PlayerGameState opponent = state.GetPlayer(opponentIndex);
                    foreach (GameCardInstance attacker in player.vanguard)
                    {
                        foreach (GameCardInstance target in opponent.vanguard)
                        {
                            actions.Add(new LegalGameAction
                            {
                                label = "Attack vanguard with " + attacker.card_id,
                                action_type = GameActionType.DeclareAttack,
                                actor_index = playerIndex,
                                card_instance_id = attacker.instance_id,
                                target_card_instance_id = target.instance_id
                            });
                        }
                        foreach (GameCardInstance target in opponent.rear_guard)
                        {
                            actions.Add(new LegalGameAction
                            {
                                label = "Attack rear-guard with " + attacker.card_id,
                                action_type = GameActionType.DeclareAttack,
                                actor_index = playerIndex,
                                card_instance_id = attacker.instance_id,
                                target_card_instance_id = target.instance_id
                            });
                        }
                    }
                    foreach (GameCardInstance attacker in player.rear_guard)
                    {
                        foreach (GameCardInstance target in opponent.vanguard)
                        {
                            actions.Add(new LegalGameAction
                            {
                                label = "Attack vanguard with rear-guard " + attacker.card_id,
                                action_type = GameActionType.DeclareAttack,
                                actor_index = playerIndex,
                                card_instance_id = attacker.instance_id,
                                target_card_instance_id = target.instance_id
                            });
                        }
                        foreach (GameCardInstance target in opponent.rear_guard)
                        {
                            actions.Add(new LegalGameAction
                            {
                                label = "Attack rear-guard with rear-guard " + attacker.card_id,
                                action_type = GameActionType.DeclareAttack,
                                actor_index = playerIndex,
                                card_instance_id = attacker.instance_id,
                                target_card_instance_id = target.instance_id
                            });
                        }
                    }
                }

                foreach (GameCardInstance card in player.hand)
                {
                    actions.Add(new LegalGameAction
                    {
                        label = "Guard with " + card.card_id,
                        action_type = GameActionType.Guard,
                        actor_index = playerIndex,
                        card_instance_id = card.instance_id
                    });
                }

                if (player.deck.Count > 0)
                {
                    AddTriggerCheckAction(actions, playerIndex, TriggerCheckSource.Drive, "Drive check top card");
                    AddTriggerCheckAction(actions, playerIndex, TriggerCheckSource.Damage, "Damage check top card");
                }
            }

            return actions;
        }

        private static void AddMoveActions(
            List<LegalGameAction> actions,
            int playerIndex,
            IReadOnlyList<GameCardInstance> cards,
            GameZone fromZone,
            GameZone toZone,
            string label)
        {
            foreach (GameCardInstance card in cards)
            {
                actions.Add(new LegalGameAction
                {
                    label = label + " " + card.card_id,
                    action_type = GameActionType.MoveCard,
                    actor_index = playerIndex,
                    card_instance_id = card.instance_id,
                    from_zone = fromZone,
                    to_zone = toZone
                });
            }
        }

        private static void AddTopDeckMoveAction(
            List<LegalGameAction> actions,
            int playerIndex,
            IReadOnlyList<GameCardInstance> cards,
            GameZone fromZone,
            GameZone toZone,
            string label)
        {
            if (cards.Count == 0)
            {
                return;
            }

            actions.Add(new LegalGameAction
            {
                label = label,
                action_type = GameActionType.MoveCard,
                actor_index = playerIndex,
                card_instance_id = string.Empty,
                from_zone = fromZone,
                to_zone = toZone
            });
        }

        private static void AddResourceFlipActions(
            List<LegalGameAction> actions,
            int playerIndex,
            IReadOnlyList<GameCardInstance> damage)
        {
            foreach (GameCardInstance card in damage)
            {
                if (card.face_up)
                {
                    actions.Add(new LegalGameAction
                    {
                        label = "Counter-Blast " + card.card_id,
                        action_type = GameActionType.ResourceFlip,
                        actor_index = playerIndex,
                        card_instance_id = card.instance_id,
                        from_zone = GameZone.Damage,
                        to_zone = GameZone.Damage,
                        resource_operation_type = GameResourceOperationType.CounterBlast,
                        resource_delta = -1
                    });
                }
                else
                {
                    actions.Add(new LegalGameAction
                    {
                        label = "Counter-Charge " + card.card_id,
                        action_type = GameActionType.ResourceFlip,
                        actor_index = playerIndex,
                        card_instance_id = card.instance_id,
                        from_zone = GameZone.Damage,
                        to_zone = GameZone.Damage,
                        resource_operation_type = GameResourceOperationType.CounterCharge,
                        resource_delta = 1
                    });
                }
            }
        }

        private static void AddTriggerCheckAction(
            List<LegalGameAction> actions,
            int playerIndex,
            TriggerCheckSource checkSource,
            string label)
        {
            actions.Add(new LegalGameAction
            {
                label = label,
                action_type = GameActionType.TriggerCheck,
                actor_index = playerIndex,
                from_zone = GameZone.Deck,
                to_zone = GameZone.Trigger,
                trigger_check_source = checkSource
            });
        }

        private static void AddPhaseAction(List<LegalGameAction> actions, GameState state, int playerIndex, GamePhase phase)
        {
            if (state.phase == phase)
            {
                return;
            }

            actions.Add(new LegalGameAction
            {
                label = "Set phase " + phase,
                action_type = GameActionType.SetPhase,
                actor_index = playerIndex,
                phase = phase
            });
        }

        private static void AddGiftMarkerAction(List<LegalGameAction> actions, int playerIndex, GiftMarkerType markerType)
        {
            actions.Add(new LegalGameAction
            {
                label = "Add " + markerType + " marker",
                action_type = GameActionType.AddGiftMarker,
                actor_index = playerIndex,
                gift_marker_type = markerType,
                marker_delta = 1
            });
        }
    }
}
