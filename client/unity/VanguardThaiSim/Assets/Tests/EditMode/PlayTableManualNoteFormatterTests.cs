using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PlayTableManualNoteFormatterTests
    {
        [Test]
        public void EmptyNotesFormatExistingMessage()
        {
            Assert.AreEqual(
                PlayTableManualNoteFormatter.EmptyMessage,
                PlayTableManualNoteFormatter.FormatLatest(null));
            Assert.AreEqual(
                PlayTableManualNoteFormatter.EmptyMessage,
                PlayTableManualNoteFormatter.FormatList(new List<PlayTableManualNote>(), 3));
        }

        [Test]
        public void NoteFormatsPhaseSelectedCardAndTarget()
        {
            PlayTableManualNote note = PlayTableManualNoteFactory.Create(
                1,
                GamePhase.Battle,
                "BT01-001TH",
                GameZone.Vanguard,
                "BT02-001TH",
                GameZone.RearGuard);

            Assert.AreEqual(
                "#1 Battle | BT01-001TH from Vanguard | BT02-001TH from opponent RearGuard",
                PlayTableManualNoteFormatter.Format(note));
        }

        [Test]
        public void MissingSelectionAndTargetUsePlayerFacingPlaceholders()
        {
            PlayTableManualNote note = PlayTableManualNoteFactory.Create(
                2,
                GamePhase.Main,
                string.Empty,
                GameZone.Hand,
                string.Empty,
                GameZone.Vanguard);

            Assert.AreEqual(
                "#2 Main | no selected card | no target",
                PlayTableManualNoteFormatter.Format(note));
        }

        [Test]
        public void LatestUsesLastNoteAndDoesNotExposeInstanceIds()
        {
            var notes = new List<PlayTableManualNote>
            {
                PlayTableManualNoteFactory.Create(1, GamePhase.Main, "BT01-001TH", GameZone.Hand, string.Empty, GameZone.Vanguard),
                PlayTableManualNoteFactory.Create(2, GamePhase.Battle, "BT01-002TH", GameZone.RearGuard, "BT01-003TH", GameZone.Vanguard)
            };

            string formatted = PlayTableManualNoteFormatter.FormatLatest(notes);

            Assert.AreEqual(
                "#2 Battle | BT01-002TH from RearGuard | BT01-003TH from opponent Vanguard",
                formatted);
            Assert.IsFalse(formatted.Contains("instance"));
        }

        [Test]
        public void ListFormatsNewestFirstWithLimit()
        {
            var notes = new List<PlayTableManualNote>
            {
                PlayTableManualNoteFactory.Create(1, GamePhase.Main, "A", GameZone.Hand, string.Empty, GameZone.Vanguard),
                PlayTableManualNoteFactory.Create(2, GamePhase.Battle, "B", GameZone.RearGuard, "C", GameZone.Vanguard),
                PlayTableManualNoteFactory.Create(3, GamePhase.End, string.Empty, GameZone.Hand, string.Empty, GameZone.Vanguard)
            };

            Assert.AreEqual(
                "#3 End | no selected card | no target\n" +
                "#2 Battle | B from RearGuard | C from opponent Vanguard\n",
                PlayTableManualNoteFormatter.FormatList(notes, 2));
        }
    }
}
