using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace VanguardThaiSim.Decks
{
    public sealed class DeckStorage
    {
        private readonly string deckDirectory;

        public DeckStorage(string deckDirectory = null)
        {
            this.deckDirectory = string.IsNullOrWhiteSpace(deckDirectory)
                ? Path.Combine(Application.persistentDataPath, "decks")
                : deckDirectory;
        }

        public string Save(VanguardDeck deck)
        {
            if (deck == null)
            {
                throw new ArgumentNullException(nameof(deck));
            }

            if (string.IsNullOrWhiteSpace(deck.deck_id))
            {
                throw new InvalidOperationException("Deck id is required before saving.");
            }

            Directory.CreateDirectory(deckDirectory);
            string path = GetDeckPath(deck.deck_id);
            File.WriteAllText(path, deck.ToJson(true), Encoding.UTF8);
            return path;
        }

        public VanguardDeck Load(string deckId)
        {
            string path = GetDeckPath(deckId);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Deck file was not found.", path);
            }

            return VanguardDeck.FromJson(File.ReadAllText(path, Encoding.UTF8));
        }

        public VanguardDeck LoadLatest()
        {
            string latestPath = null;
            DateTime latestWrite = DateTime.MinValue;
            foreach (string path in Directory.Exists(deckDirectory) ? Directory.GetFiles(deckDirectory, "*.json") : Array.Empty<string>())
            {
                DateTime writeTime = File.GetLastWriteTimeUtc(path);
                if (latestPath == null || writeTime > latestWrite)
                {
                    latestPath = path;
                    latestWrite = writeTime;
                }
            }

            if (latestPath == null)
            {
                return null;
            }

            return VanguardDeck.FromJson(File.ReadAllText(latestPath, Encoding.UTF8));
        }

        public bool Delete(string deckId)
        {
            string path = GetDeckPath(deckId);
            if (!File.Exists(path))
            {
                return false;
            }

            File.Delete(path);
            return true;
        }

        public IReadOnlyList<string> ListDeckIds()
        {
            List<string> ids = new List<string>();
            if (!Directory.Exists(deckDirectory))
            {
                return ids;
            }

            foreach (string path in Directory.GetFiles(deckDirectory, "*.json"))
            {
                ids.Add(Path.GetFileNameWithoutExtension(path));
            }

            return ids;
        }

        private string GetDeckPath(string deckId)
        {
            if (string.IsNullOrWhiteSpace(deckId))
            {
                throw new ArgumentException("Deck id is required.", nameof(deckId));
            }

            return Path.Combine(deckDirectory, SanitizeFileName(deckId) + ".json");
        }

        private static string SanitizeFileName(string value)
        {
            string sanitized = value;
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                sanitized = sanitized.Replace(invalid, '_');
            }

            return sanitized;
        }
    }
}
