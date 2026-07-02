# Ninth-Slice Recipe Draft Model Spec

Milestone: `M68-03`

## Purpose

`M68-03` converts the `M68-02` ninth-slice review packet into explicit,
reviewable recipe drafts for the selected Aqua Force G-series source slice.

The drafts are advisory only. They are inputs for `M68-04` validation and must
not be treated as playable decks, saved decks, UI deck-list entries, runtime
fixtures, or bot playbook data.

## Inputs

- `outputs/target_slice/m68_02_ninth_slice_review_packet.json`
- `outputs/target_slice/m68_01_ninth_slice_fixture_scaffold.json`
- `data/packs/vanguard_th/cards.sqlite`

Tests may pass in-memory reports until the real upstream artifacts exist.

## Draft Rules

- Use `M68-02` candidate edges as advisory pair anchors.
- Limit the draft batch to up to `25` non-trigger, main-deck-compatible
  candidate edges.
- Exclude trigger cards from pair anchors so the scaffold's 16-trigger profile
  stays intact.
- Exclude Grade 4/G-unit cards from main-deck drafts until G Zone support
  exists.
- Build a source-backed base fixture from the `M68-01` scaffold.
- Preserve a 50-card main deck.
- Preserve a 16-trigger profile.
- Force each candidate edge source/target pair to quantity `4`, bounded by
  SQLite `deck_limit`.
- Trim non-trigger scaffold filler cards to keep the draft at 50 cards.
- Keep quantities advisory until `M68-04` validates them.

Current in-memory M68-03 evidence produces `25` quantity-complete drafts and
skips `1` candidate edge because it is trigger, Grade 4/G-zone deferred, or
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

If a draft contains any `M68-02` manual-review card, it must remain blocked by
`manual_card_semantic_review` until a later review/repair milestone clears it.

G Zone, Stride, G-unit, Generation Break, and Aqua Force battle-order text
remain blocked behind manual review until dedicated rules support exists.

## Verification

```powershell
python -m unittest tests.test_ninth_slice_recipe_draft_model
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M68-02 and M68-01 outputs exist:

```powershell
python tools\deck\build_ninth_slice_recipe_draft_model.py
```
