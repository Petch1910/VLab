using System;
using VanguardThaiSim.Decks;

namespace VanguardThaiSim.Multiplayer
{
    public static class DeckRevealResponseVerifier
    {
        public static bool TryVerify(
            RoomPlayerInfo committedPlayer,
            NetworkDeckRevealResponse response,
            string expectedRoomId,
            string expectedPackDefinitionHash,
            out VanguardDeck revealedDeck,
            out string rejectionReason)
        {
            revealedDeck = null;
            rejectionReason = null;
            if (committedPlayer == null)
            {
                rejectionReason = "PLAYER_MISSING";
                return false;
            }

            if (response == null)
            {
                rejectionReason = "DECK_REVEAL_RESPONSE_MISSING";
                return false;
            }

            if (!string.Equals(response.room_id ?? "", expectedRoomId ?? "", StringComparison.Ordinal))
            {
                rejectionReason = "DECK_REVEAL_ROOM_MISMATCH";
                return false;
            }

            if (!string.Equals(response.player_id ?? "", committedPlayer.player_id ?? "", StringComparison.Ordinal))
            {
                rejectionReason = "DECK_REVEAL_PLAYER_MISMATCH";
                return false;
            }

            if (!string.Equals(response.pack_definition_hash ?? "", expectedPackDefinitionHash ?? "", StringComparison.Ordinal))
            {
                rejectionReason = "DECK_REVEAL_PACK_MISMATCH";
                return false;
            }

            if (!string.Equals(response.deck_commitment ?? "", committedPlayer.deck_commitment ?? "", StringComparison.OrdinalIgnoreCase))
            {
                rejectionReason = "DECK_REVEAL_COMMITMENT_FIELD_MISMATCH";
                return false;
            }

            if (!string.Equals(response.deck_commitment_algorithm ?? "", committedPlayer.deck_commitment_algorithm ?? "", StringComparison.Ordinal))
            {
                rejectionReason = "DECK_REVEAL_ALGORITHM_MISMATCH";
                return false;
            }

            if (string.IsNullOrWhiteSpace(response.revealed_deck_code))
            {
                rejectionReason = "DECK_REVEAL_DECK_CODE_MISSING";
                return false;
            }

            try
            {
                revealedDeck = DeckCodeCodec.Import(response.revealed_deck_code);
            }
            catch (Exception exception)
            {
                rejectionReason = "DECK_REVEAL_DECK_CODE_INVALID: " + exception.Message;
                return false;
            }

            RoomPlayerInfo revealPlayer = new RoomPlayerInfo
            {
                player_id = committedPlayer.player_id,
                deck_commitment = committedPlayer.deck_commitment,
                deck_commitment_algorithm = committedPlayer.deck_commitment_algorithm,
                deck_reveal_nonce = response.deck_reveal_nonce
            };

            return DeckCommitmentService.TryVerifyReveal(
                revealPlayer,
                revealedDeck,
                expectedRoomId,
                expectedPackDefinitionHash,
                out rejectionReason);
        }
    }
}
