# Eighth-Slice Recipe Draft Model Spec

Milestone: `M64-03`

## Purpose

`M64-03` converts the `M64-02` eighth-slice review packet into explicit,
reviewable recipe drafts for the selected Kagero Link Joker / Legion Mate
source slice.

The drafts are advisory only. They are inputs for `M64-04` validation and must
not be treated as playable decks, saved decks, UI deck-list entries, runtime
fixtures, or bot playbook data.

## Inputs

- `outputs/target_slice/m64_02_eighth_slice_review_packet.json`
- `outputs/target_slice/m64_01_eighth_slice_fixture_scaffold.json`
- `data/packs/vanguard_th/cards.sqlite`

Tests may pass in-memory reports until the real upstream artifacts exist.

## Draft Rules

- Use `M64-02` candidate edges as advisory pair anchors.
- Limit the draft batch to up to `25` non-trigger, main-deck-compatible
  candidate edges.
- Exclude trigger cards from pair anchors so the scaffold's 16-trigger profile
  stays intact.
- Exclude Grade 4 cards from main-deck drafts until format support exists.
- Build a source-backed base fixture from the `M64-01` scaffold.
- Preserve a 50-card main deck.
- Preserve a 16-trigger profile.
- Force each candidate edge source/target pair to quantity `4`, bounded by
  SQLite `deck_limit`.
- Trim non-trigger scaffold filler cards to keep the draft at 50 cards.
- Keep quantities advisory until `M64-04` validates them.

Current in-memory M64-03 evidence produces `25` quantity-complete drafts and
skips `5` candidate edges because they are trigger, Grade 4/format deferred, or
missing from the current main-deck-compatible pool.

## Runtime Boundary

This milestone must not:

- create or inject saved decks
- publish drafts to UI deck lists
- promote runtime deck fixtures
- enable bot/playbook integration
- parse live card text
- mutate `GameState`

## Manual Review Boundary

If a draft contains any `M64-02` manual-review card, it must remain blocked by
`manual_card_semantic_review` until a later review/repair milestone clears it.

Lock/Unlock and Legion/Mate text remains blocked behind manual review until
dedicated rules support exists.

## Verification

```powershell
python -m unittest tests.test_eighth_slice_recipe_draft_model
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M64-02 and M64-01 outputs exist:

```powershell
python tools\deck\build_eighth_slice_recipe_draft_model.py
```
