# Fourth-Slice Recipe Draft Model Spec

Milestone: `M48-03`

## Purpose

`M48-03` converts the `M48-02` fourth-slice review packet into explicit,
reviewable recipe drafts for the selected Royal Paladin G-era expanded source
slice.

The drafts are advisory only. They are inputs for `M48-04` validation and must
not be treated as playable decks, saved decks, UI deck-list entries, runtime
fixtures, or bot playbook data.

## Inputs

- `outputs/target_slice/m48_02_fourth_slice_review_packet.json`
- `outputs/target_slice/m48_01_fourth_slice_fixture_scaffold.json`
- `data/packs/vanguard_th/cards.sqlite`

## Draft Rules

- Use `M48-02` candidate edges as advisory pair anchors.
- Limit the draft batch to the first `25` non-trigger, main-deck-compatible
  candidate edges.
- Exclude Grade 4/G-unit cards from main deck drafts until G Zone support
  exists.
- Exclude trigger cards from pair anchors so the scaffold's 16-trigger profile
  stays intact.
- Build a source-backed base fixture from the `M48-01` scaffold.
- Preserve a 50-card main deck.
- Preserve a 16-trigger profile.
- Force each candidate edge source/target pair to quantity `4`, bounded by
  SQLite `deck_limit`.
- Trim non-trigger scaffold filler cards to keep the draft at 50 cards.
- Keep quantities advisory until `M48-04` validates them.

## Runtime Boundary

This milestone must not:

- create or inject saved decks
- publish drafts to UI deck lists
- promote runtime deck fixtures
- enable bot/playbook integration
- parse live card text
- mutate `GameState`

## Manual Review Boundary

If a draft contains any `M48-02` manual-review card, it must remain blocked by
`manual_card_semantic_review` until a later review/repair milestone clears it.

## Verification

```powershell
python tools\deck\build_fourth_slice_recipe_draft_model.py
python -m unittest tests.test_fourth_slice_recipe_draft_model
python -m unittest discover -s tests -p "test_*.py"
```
