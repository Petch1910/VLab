using System.Collections.Generic;
using System.Text;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public sealed class PlayTableManualNote
    {
        public int note_index;
        public GamePhase phase;
        public string selected_card_id;
        public GameZone selected_zone;
        public string target_card_id;
        public GameZone target_zone;
    }

    public static class PlayTableManualNoteFactory
    {
        public static PlayTableManualNote Create(
            int noteIndex,
            GamePhase phase,
            string selectedCardId,
            GameZone selectedZone,
            string targetCardId,
            GameZone targetZone)
        {
            return new PlayTableManualNote
            {
                note_index = noteIndex,
                phase = phase,
                selected_card_id = selectedCardId ?? string.Empty,
                selected_zone = selectedZone,
                target_card_id = targetCardId ?? string.Empty,
                target_zone = targetZone
            };
        }
    }

    public static class PlayTableManualNoteFormatter
    {
        public const string EmptyMessage = "No manual notes.";

        public static string FormatLatest(IReadOnlyList<PlayTableManualNote> notes)
        {
            if (notes == null || notes.Count == 0)
            {
                return EmptyMessage;
            }

            return Format(notes[notes.Count - 1]);
        }

        public static string FormatList(IReadOnlyList<PlayTableManualNote> notes, int maxEntries)
        {
            if (notes == null || notes.Count == 0)
            {
                return EmptyMessage;
            }

            int safeMax = maxEntries <= 0 ? notes.Count : maxEntries;
            var builder = new StringBuilder();
            for (int i = notes.Count - 1; i >= 0 && notes.Count - i <= safeMax; i--)
            {
                builder.Append(Format(notes[i])).Append('\n');
            }

            return builder.ToString();
        }

        public static string Format(PlayTableManualNote note)
        {
            if (note == null)
            {
                return EmptyMessage;
            }

            string selected = string.IsNullOrWhiteSpace(note.selected_card_id)
                ? "no selected card"
                : note.selected_card_id + " from " + note.selected_zone;
            string target = string.IsNullOrWhiteSpace(note.target_card_id)
                ? "no target"
                : note.target_card_id + " from opponent " + note.target_zone;
            return "#" + note.note_index + " " + note.phase + " | " + selected + " | " + target;
        }
    }
}
