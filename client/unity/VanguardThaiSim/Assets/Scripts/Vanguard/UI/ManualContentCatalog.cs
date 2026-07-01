using System;
using System.Collections.Generic;
using System.Text;

namespace VanguardThaiSim.UI
{
    [Serializable]
    public sealed class ManualSection
    {
        public string section_id;
        public string category;
        public string title;
        public string body;
        public string related_screen;
        public string loading_tip;
    }

    public static class ManualContentCatalog
    {
        public static IReadOnlyList<ManualSection> Sections()
        {
            return new[]
            {
                Section(
                    "home",
                    "App Guide",
                    "Home",
                    "Home is the starting point for Solo Play, Deck Builder, Card Browser, Online Room, Settings, and future replay or CPU features. Build or load a playable deck before starting Solo Play. Online Room is a casual trusted-client room mode for playing with friends.",
                    "Home",
                    "A playable deck is required before Solo Play can start."),
                Section(
                    "card_browser",
                    "App Guide",
                    "Card Browser",
                    "Use the browser to search the local Vanguard TH runtime pack by name, Thai text, series, clan or nation. Card details and images come from local pack data. If an image path is missing, the program uses a fallback instead of blocking the flow.",
                    "Card Browser",
                    "Card images come from the local runtime pack."),
                Section(
                    "deck_builder",
                    "App Guide",
                    "Deck Builder",
                    "Deck Builder manages Main, Ride, and G zone cards, deck counters, rule badges, validation messages, deck tools, and deck accessories. Cosmetics are separate from legality. Deck import and export are for sharing local deck lists.",
                    "Deck Builder",
                    "Cosmetic accessories do not change deck legality."),
                Section(
                    "play_table",
                    "App Guide",
                    "PlayTable",
                    "PlayTable is a manual table. Use setup, phase buttons, hand strip, selected-card preview, board zones, common actions, battle flow status, and match log to play. Unsupported card effects should be resolved manually. Diagnostics and automation helpers stay in the Advanced drawer.",
                    "PlayTable",
                    "Use the selected-card preview to confirm what the next action will affect."),
                Section(
                    "online_room",
                    "App Guide",
                    "Online Room",
                    "Online Room lets friends connect, host or join a room, check deck and pack readiness, start a table, recover from reconnect when possible, and later rematch. This build is trusted-client mode, not ranked-secure server play. Private deck and hand data must stay hidden from opponent and spectator views.",
                    "Online Room",
                    "Online rooms are casual trusted-client rooms in this build."),
                Section(
                    "replay",
                    "App Guide",
                    "Replay",
                    "Replay and log viewing are based on player-readable event logs. The log should explain actions without exposing private ids, technical connection details, or hidden information.",
                    "Replay",
                    "The match log should read like a player event list."),
                Section(
                    "custom_packs",
                    "App Guide",
                    "Custom Packs",
                    "Custom packs are local imports that must pass validation before use. Unsupported fields produce warnings, failed imports must not mutate the active pack, and user-provided images require manifest, hash, and fallback handling. Public third-party pack auto-download is outside the current scope.",
                    "Custom Packs",
                    "Missing private assets fall back instead of blocking play."),
                Section(
                    "playing_field",
                    "Vanguard Rules Basics",
                    "Playing Field",
                    "The main field includes the Vanguard circle, rear-guard circles, guardian circle, deck, hand, drop, damage, soul, bind, order area, trigger zone, ride deck, and G zone where the selected format uses it.",
                    "PlayTable",
                    "Zone counts help confirm where cards moved."),
                Section(
                    "turn_flow",
                    "Vanguard Rules Basics",
                    "Turn Flow",
                    "A typical turn moves through Stand and Draw, Ride, Main, Battle, and End. The first player has attack restrictions at a high level, and exact legality is handled by the rules profile and current phase.",
                    "PlayTable",
                    "Phase buttons should match the current table flow."),
                Section(
                    "deck_and_setup",
                    "Vanguard Rules Basics",
                    "Deck And Setup",
                    "Setup uses the main deck, ride deck, and G zone depending on format. Players prepare an opening hand, mulligan as needed, and place the first Vanguard before normal play begins.",
                    "Deck Builder",
                    "Check deck validation before starting a game."),
                Section(
                    "combat_basics",
                    "Vanguard Rules Basics",
                    "Combat Basics",
                    "Combat moves from attack declaration to target selection, boost, guard step, drive check, damage check when the attack hits a Vanguard, battle resolution, and close step.",
                    "PlayTable",
                    "Manual resolution is expected for unsupported card effects."),
                Section(
                    "triggers",
                    "Vanguard Rules Basics",
                    "Triggers",
                    "Trigger types include Critical, Draw, Front, Heal, Over, and legacy Stand where relevant. Trigger effects grant power or other bonuses according to the current check and card text.",
                    "PlayTable",
                    "Trigger text labels are used before official icon assets are available."),
                Section(
                    "resources",
                    "Vanguard Rules Basics",
                    "Resources",
                    "Common resources include Counter-Blast, Counter-Charge, Soul, Soul-Blast, Energy where relevant, and once-per-turn or once-per-fight limits. Card text and structured ability support decide exact costs.",
                    "PlayTable",
                    "Track resource costs before resolving effects."),
                Section(
                    "formats",
                    "Vanguard Rules Basics",
                    "Formats",
                    "Standard, V-Premium, and Premium use different feature flags. Format behavior should come from ruleset profiles rather than ad hoc UI toggles.",
                    "Deck Builder",
                    "The selected format should match the deck rules."),
                Section(
                    "markers_and_tokens",
                    "Vanguard Rules Basics",
                    "Markers And Tokens",
                    "Gift markers include Force, Accel, and Protect. Quick Shield, crest, and persona-related options are represented as high-level table concepts; exact card-specific behavior still comes from card text and supported rules.",
                    "PlayTable",
                    "Markers are table state, not deck legality cosmetics.")
            };
        }

        public static IReadOnlyList<string> LoadingTips()
        {
            List<string> tips = new List<string>();
            foreach (ManualSection section in Sections())
            {
                if (!string.IsNullOrEmpty(section.loading_tip))
                {
                    tips.Add(section.loading_tip);
                }
            }

            return tips;
        }

        public static string FormatAllSections()
        {
            return ManualContentFilter.FormatSections(Sections());
        }

        private static ManualSection Section(
            string sectionId,
            string category,
            string title,
            string body,
            string relatedScreen,
            string loadingTip)
        {
            return new ManualSection
            {
                section_id = sectionId,
                category = category,
                title = title,
                body = body,
                related_screen = relatedScreen,
                loading_tip = loadingTip
            };
        }
    }
}
