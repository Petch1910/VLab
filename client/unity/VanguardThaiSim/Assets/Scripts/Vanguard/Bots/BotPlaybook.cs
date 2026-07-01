using System;
using System.Collections.Generic;
using UnityEngine;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Bots
{
    [Serializable]
    public sealed class BotPlaybook
    {
        public string playbook_id;
        public string display_name;
        public BotProfileType preferred_profile;
        public List<string> rideline_card_ids = new List<string>();
        public List<string> mulligan_keep_card_ids = new List<string>();
        public List<string> priority_call_card_ids = new List<string>();
        public List<string> battle_plan_notes = new List<string>();

        public void EnsureLists()
        {
            if (rideline_card_ids == null) rideline_card_ids = new List<string>();
            if (mulligan_keep_card_ids == null) mulligan_keep_card_ids = new List<string>();
            if (priority_call_card_ids == null) priority_call_card_ids = new List<string>();
            if (battle_plan_notes == null) battle_plan_notes = new List<string>();
        }

        public static BotPlaybook CreateDefault()
        {
            return new BotPlaybook
            {
                playbook_id = "default_balanced",
                display_name = "Default Balanced",
                preferred_profile = BotProfileType.Balanced,
                battle_plan_notes = new List<string>
                {
                    "Use balanced generic Vanguard heuristics."
                }
            };
        }
    }

    [Serializable]
    public sealed class BotPlaybookLibrary
    {
        public List<BotPlaybook> playbooks = new List<BotPlaybook>();
        public BotPlaybook default_playbook = BotPlaybook.CreateDefault();

        public void EnsureLists()
        {
            if (playbooks == null) playbooks = new List<BotPlaybook>();
            if (default_playbook == null) default_playbook = BotPlaybook.CreateDefault();
            default_playbook.EnsureLists();
            for (int i = 0; i < playbooks.Count; i++)
            {
                playbooks[i]?.EnsureLists();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static BotPlaybookLibrary FromJson(string json)
        {
            BotPlaybookLibrary library = JsonUtility.FromJson<BotPlaybookLibrary>(json);
            if (library == null)
            {
                throw new ArgumentException("Bot playbook library JSON could not be parsed.", "json");
            }

            library.EnsureLists();
            return library;
        }

        public BotPlaybook MatchFromState(GameState state, int playerIndex)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            PlayerGameState player = state.GetPlayer(playerIndex);
            var visibleIds = new List<string>();
            AddVisibleKnownIds(visibleIds, player.vanguard);
            AddVisibleKnownIds(visibleIds, player.rear_guard);
            AddVisibleKnownIds(visibleIds, player.drop);
            AddVisibleKnownIds(visibleIds, player.damage);
            return MatchFromVisibleCards(visibleIds);
        }

        public BotPlaybook MatchFromVisibleCards(IEnumerable<string> visibleCardIds)
        {
            EnsureLists();
            var visible = new HashSet<string>(StringComparer.Ordinal);
            if (visibleCardIds != null)
            {
                foreach (string cardId in visibleCardIds)
                {
                    if (!string.IsNullOrEmpty(cardId) && cardId != GameStateViewFactory.HiddenCardId)
                    {
                        visible.Add(cardId);
                    }
                }
            }

            BotPlaybook best = null;
            int bestScore = 0;
            for (int i = 0; i < playbooks.Count; i++)
            {
                BotPlaybook playbook = playbooks[i];
                if (playbook == null)
                {
                    continue;
                }

                playbook.EnsureLists();
                int score = CountMatches(playbook.rideline_card_ids, visible);
                if (score <= 0)
                {
                    continue;
                }

                if (best == null ||
                    score > bestScore ||
                    (score == bestScore && string.CompareOrdinal(playbook.playbook_id, best.playbook_id) < 0))
                {
                    best = playbook;
                    bestScore = score;
                }
            }

            return best ?? default_playbook;
        }

        private static int CountMatches(IEnumerable<string> cardIds, ISet<string> visible)
        {
            int score = 0;
            if (cardIds == null)
            {
                return score;
            }

            foreach (string cardId in cardIds)
            {
                if (!string.IsNullOrEmpty(cardId) && visible.Contains(cardId))
                {
                    score++;
                }
            }

            return score;
        }

        private static void AddVisibleKnownIds(List<string> visibleIds, IList<GameCardInstance> cards)
        {
            if (cards == null)
            {
                return;
            }

            for (int i = 0; i < cards.Count; i++)
            {
                GameCardInstance card = cards[i];
                if (card != null &&
                    card.face_up &&
                    !string.IsNullOrEmpty(card.card_id) &&
                    card.card_id != GameStateViewFactory.HiddenCardId)
                {
                    visibleIds.Add(card.card_id);
                }
            }
        }
    }
}
