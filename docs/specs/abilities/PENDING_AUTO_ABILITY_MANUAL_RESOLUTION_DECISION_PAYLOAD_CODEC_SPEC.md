# Pending Auto Ability Manual Resolution Decision Payload Codec Spec

## Goal

Encode and decode pending auto ability manual resolution decisions as
network-ready payloads.

This milestone does not add Photon dispatch, session storage, PlayTable
controls, or actual ability resolution.

## Payload Contract

`NetworkPendingAutoAbilityManualResolutionDecisionPayload` stores:

- `protocol_version`
- `payload_id`
- `room_id`
- `sender_player_index`
- `decision_id`
- `decision_type`
- `selected_index`
- `pending_id`
- `perspective`
- `viewer_player_index`
- serialized `pending_auto_ability_manual_resolution_decision_json`

## Codec Contract

- `Encode` accepts an already-created
  `PendingAutoAbilityManualResolutionDecision`.
- `TryDecode` validates protocol and parses the serialized decision JSON.
- Encoding clones/sanitizes source data and never mutates the source decision.
- Hidden source identity remains hidden even if the caller passes unsafe source
  metadata.

## Rejections

- Missing or empty decision JSON:
  `PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_PAYLOAD_INVALID`
- Protocol mismatch:
  `PROTOCOL_VERSION_MISMATCH`
- Parse failure:
  `PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_PARSE_FAILED: <message>`

## Verification

- Unity compile passes.
- EditMode tests cover visible round-trip, protocol mismatch, invalid payload,
  deterministic encoding, hidden-source sanitization, and no source mutation.
