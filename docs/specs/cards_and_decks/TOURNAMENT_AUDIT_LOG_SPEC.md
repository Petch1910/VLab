# Tournament Audit Log Spec

## Status

Implemented in `M13-10` as a public-safe JSON export model and factory.

## Purpose

Tournament/friend-room review needs a shareable match record without exposing
private deck codes, reveal nonces, hidden hand/deck card ids, or true private
card instance ids.

The M13-10 audit log is not a ranked anti-cheat system. It is a deterministic
record format that later server/custom tooling can archive or compare.

## Runtime Model

`TournamentAuditLog` contains:

- protocol version
- audit id and generated timestamp
- room metadata: room id, format, state, visibility, deck privacy mode, host id,
  random seed
- pack hashes from `PackSyncInfo`
- per-player public metadata:
  - player id/display name
  - deck id/hash
  - deck commitment and algorithm
  - deck reveal policy
  - deck/hand counts
  - ready/connected/cursor
- sanitized `NetworkPublicGameEvent` list
- result metadata: status, winner id/index, end reason, ended timestamp

`TournamentAuditLogFactory.Create(...)` is pure. It clones/sanitizes inputs and
does not mutate the source room or public events.

## Privacy Boundary

Allowed:

- pack hashes
- deck hash
- deck commitment
- commitment algorithm
- public reveal card id/instance id
- public event ids and counts

Forbidden:

- `RoomPlayerInfo.deck_code`
- `RoomPlayerInfo.deck_reveal_nonce`
- true private deck/hand card ids
- true private card instance ids
- public card ids on events where `hides_card_identity == true`

For hidden public events, the audit factory clears:

- `public_card_id`
- `public_card_instance_id`
- `reveal_proof`

## Verification

EditMode tests cover:

- room/pack/player/public-event/result export
- JSON round-trip with lists
- source room and public event no-mutation
- null event/result handling
- no deck code, reveal nonce, hidden card id, hidden instance id, or hidden
  public card payload leak
