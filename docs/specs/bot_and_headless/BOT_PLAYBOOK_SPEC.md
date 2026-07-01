# Bot Playbook Spec

## Status

Implemented in `M9-05`.

## Purpose

Give the bot a data-driven way to recognize public archetype/rideline signals
and choose simple strategic hints without hardcoding deck-specific behavior into
the bot controller.

This is not an automatic card-effect system. Playbooks guide heuristics and
search priorities only.

## Playbook Fields

MVP playbook:

- `playbook_id`
- `display_name`
- preferred `BotProfileType`
- `rideline_card_ids`
- `mulligan_keep_card_ids`
- `priority_call_card_ids`
- `battle_plan_notes`

## Matching Rules

The library can match a playbook using visible card ids only:

- current vanguard card ids
- public rideline cards if represented by current state
- other explicitly supplied public card ids

Do not inspect opponent hidden hand, deck, or ride deck to match a playbook.

If no playbook matches, return a default balanced playbook.

## Boundary

Playbooks must not:

- mutate `GameState`
- execute actions
- parse Thai rules text at runtime
- bypass legal action masks
- reveal hidden information

## Acceptance Tests

- exact visible rideline/vanguard id matches a playbook
- no match returns default balanced playbook
- priority call ids preserve deterministic order
- playbook JSON round-trips through Unity `JsonUtility`
- matching does not mutate state

## Future Extensions

- load bundled playbook JSON files from `data/packs`
- custom pack playbook overrides
- archetype matchup notes
- card-feature tags for AbilityCore hooks
- mined combo lines from offline replay/search tooling
