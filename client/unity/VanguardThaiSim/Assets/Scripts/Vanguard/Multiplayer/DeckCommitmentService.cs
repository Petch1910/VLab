using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using VanguardThaiSim.Decks;

namespace VanguardThaiSim.Multiplayer
{
    public static class DeckCommitmentService
    {
        public const string Algorithm = "sha256-canonical-deck-v1";
        public const string RevealProofAlgorithm = "sha256-public-reveal-v1";

        public static string CreateCanonicalDeckJson(VanguardDeck deck)
        {
            if (deck == null)
            {
                throw new ArgumentNullException(nameof(deck));
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("{");
            AppendProperty(builder, "format", deck.format);
            builder.Append(",");
            AppendProperty(builder, "card_pack_id", deck.card_pack_id);
            builder.Append(",");
            AppendProperty(builder, "card_pack_version", deck.card_pack_version);
            builder.Append(",");
            AppendZone(builder, "main", deck.GetEntries(DeckZone.Main));
            builder.Append(",");
            AppendZone(builder, "ride", deck.GetEntries(DeckZone.Ride));
            builder.Append(",");
            AppendZone(builder, "g", deck.GetEntries(DeckZone.G));
            builder.Append("}");
            return builder.ToString();
        }

        public static string ComputeDeckHash(VanguardDeck deck)
        {
            return Sha256Hex(CreateCanonicalDeckJson(deck));
        }

        public static string CreateCommitment(VanguardDeck deck, string playerNonce, string roomId, string packDefinitionHash)
        {
            string canonical = CreateCanonicalDeckJson(deck);
            string input = canonical + "|" + Normalize(playerNonce) + "|" + Normalize(roomId) + "|" + Normalize(packDefinitionHash);
            return Sha256Hex(input);
        }

        public static bool VerifyCommitment(VanguardDeck deck, string playerNonce, string roomId, string packDefinitionHash, string expectedCommitment)
        {
            string actual = CreateCommitment(deck, playerNonce, roomId, packDefinitionHash);
            return string.Equals(actual, expectedCommitment ?? "", StringComparison.OrdinalIgnoreCase);
        }

        public static bool TryVerifyReveal(
            RoomPlayerInfo player,
            VanguardDeck revealedDeck,
            string roomId,
            string packDefinitionHash,
            out string rejectionReason)
        {
            rejectionReason = null;
            if (player == null)
            {
                rejectionReason = "PLAYER_MISSING";
                return false;
            }

            if (revealedDeck == null)
            {
                rejectionReason = "REVEALED_DECK_MISSING";
                return false;
            }

            if (string.IsNullOrWhiteSpace(player.deck_commitment))
            {
                rejectionReason = "DECK_COMMITMENT_MISSING";
                return false;
            }

            if (!string.Equals(player.deck_commitment_algorithm ?? "", Algorithm, StringComparison.Ordinal))
            {
                rejectionReason = "DECK_COMMITMENT_ALGORITHM_MISMATCH";
                return false;
            }

            if (string.IsNullOrWhiteSpace(player.deck_reveal_nonce))
            {
                rejectionReason = "DECK_REVEAL_NONCE_MISSING";
                return false;
            }

            if (!VerifyCommitment(revealedDeck, player.deck_reveal_nonce, roomId, packDefinitionHash, player.deck_commitment))
            {
                rejectionReason = "DECK_REVEAL_COMMITMENT_MISMATCH";
                return false;
            }

            return true;
        }

        public static bool TryCreateRevealProof(
            RoomPlayerInfo player,
            string roomId,
            string packDefinitionHash,
            NetworkPublicGameEvent publicEvent,
            out string revealProof,
            out string rejectionReason)
        {
            revealProof = null;
            rejectionReason = null;
            if (publicEvent == null)
            {
                rejectionReason = "PUBLIC_EVENT_MISSING";
                return false;
            }

            if (!RequiresRevealProof(publicEvent))
            {
                rejectionReason = "PUBLIC_EVENT_NOT_REVEAL";
                return false;
            }

            if (!TryValidateRevealProofContext(player, roomId, packDefinitionHash, out rejectionReason))
            {
                return false;
            }

            revealProof = RevealProofAlgorithm + ":" + Sha256Hex(CreateRevealProofInput(player, roomId, packDefinitionHash, publicEvent));
            return true;
        }

        public static bool TryVerifyRevealProof(
            RoomPlayerInfo player,
            string roomId,
            string packDefinitionHash,
            NetworkPublicGameEvent publicEvent,
            out string rejectionReason)
        {
            rejectionReason = null;
            if (publicEvent == null)
            {
                rejectionReason = "PUBLIC_EVENT_MISSING";
                return false;
            }

            if (string.IsNullOrWhiteSpace(publicEvent.reveal_proof))
            {
                rejectionReason = "REVEAL_PROOF_MISSING";
                return false;
            }

            string expected;
            if (!TryCreateRevealProof(player, roomId, packDefinitionHash, publicEvent, out expected, out rejectionReason))
            {
                return false;
            }

            if (!string.Equals(expected, publicEvent.reveal_proof ?? "", StringComparison.OrdinalIgnoreCase))
            {
                rejectionReason = "REVEAL_PROOF_MISMATCH";
                return false;
            }

            return true;
        }

        private static bool RequiresRevealProof(NetworkPublicGameEvent publicEvent)
        {
            return publicEvent != null &&
                !publicEvent.hides_card_identity &&
                !string.IsNullOrWhiteSpace(publicEvent.public_card_id);
        }

        private static bool TryValidateRevealProofContext(
            RoomPlayerInfo player,
            string roomId,
            string packDefinitionHash,
            out string rejectionReason)
        {
            rejectionReason = null;
            if (player == null)
            {
                rejectionReason = "PLAYER_MISSING";
                return false;
            }

            if (string.IsNullOrWhiteSpace(player.deck_commitment))
            {
                rejectionReason = "DECK_COMMITMENT_MISSING";
                return false;
            }

            if (!string.Equals(player.deck_commitment_algorithm ?? "", Algorithm, StringComparison.Ordinal))
            {
                rejectionReason = "DECK_COMMITMENT_ALGORITHM_MISMATCH";
                return false;
            }

            if (string.IsNullOrWhiteSpace(roomId))
            {
                rejectionReason = "ROOM_ID_MISSING";
                return false;
            }

            if (string.IsNullOrWhiteSpace(packDefinitionHash))
            {
                rejectionReason = "PACK_DEFINITION_HASH_MISSING";
                return false;
            }

            return true;
        }

        private static string CreateRevealProofInput(
            RoomPlayerInfo player,
            string roomId,
            string packDefinitionHash,
            NetworkPublicGameEvent publicEvent)
        {
            return RevealProofAlgorithm +
                "|" + Algorithm +
                "|" + Normalize(player.player_id) +
                "|" + Normalize(player.deck_commitment) +
                "|" + Normalize(roomId) +
                "|" + Normalize(packDefinitionHash) +
                "|" + Normalize(publicEvent.event_id) +
                "|" + Normalize(publicEvent.source_event_id) +
                "|" + publicEvent.actor_index +
                "|" + Normalize(publicEvent.public_card_id) +
                "|" + Normalize(publicEvent.public_card_instance_id) +
                "|" + publicEvent.from_zone +
                "|" + publicEvent.to_zone;
        }

        private static void AppendProperty(StringBuilder builder, string key, string value)
        {
            builder.Append("\"");
            builder.Append(key);
            builder.Append("\":\"");
            builder.Append(Escape(Normalize(value)));
            builder.Append("\"");
        }

        private static void AppendZone(StringBuilder builder, string key, IReadOnlyList<DeckCardEntry> entries)
        {
            builder.Append("\"");
            builder.Append(key);
            builder.Append("\":[");
            SortedDictionary<string, int> quantities = new SortedDictionary<string, int>(StringComparer.Ordinal);
            if (entries != null)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    DeckCardEntry entry = entries[i];
                    if (entry == null || string.IsNullOrWhiteSpace(entry.card_id) || entry.quantity <= 0)
                    {
                        continue;
                    }

                    int current;
                    quantities.TryGetValue(entry.card_id, out current);
                    quantities[entry.card_id] = current + entry.quantity;
                }
            }

            bool first = true;
            foreach (KeyValuePair<string, int> pair in quantities)
            {
                if (!first)
                {
                    builder.Append(",");
                }

                builder.Append("{\"card_id\":\"");
                builder.Append(Escape(pair.Key));
                builder.Append("\",\"quantity\":");
                builder.Append(pair.Value);
                builder.Append("}");
                first = false;
            }

            builder.Append("]");
        }

        private static string Sha256Hex(string value)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value ?? "");
                byte[] hash = sha.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++)
                {
                    builder.Append(hash[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "" : value.Trim();
        }

        private static string Escape(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
