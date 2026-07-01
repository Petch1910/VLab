# Third-Slice Recipe Draft Model Spec

Milestone: `M44-03`

## Purpose

`M44-03` converts the `M44-02` third-slice review packet into explicit,
reviewable recipe drafts for the selected Link Joker / Legion Mate slice.

The drafts are advisory only. They are inputs for `M44-04` validation and must
not be treated as playable decks, saved decks, UI deck-list entries, runtime
fixtures, or bot playbook data.

## Inputs

- `outputs/target_slice/m44_02_third_slice_review_packet.json`
- `outputs/target_slice/m44_01_third_slice_fixture_scaffold.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m44_03_third_slice_recipe_draft_model.json`
- `outputs/target_slice/m44_03_third_slice_recipe_draft_model.md`

## Draft Rules

- Use `M44-02` candidate edges as advisory pair anchors.
- Limit the draft batch to the first `25` candidate edges.
- Build a source-backed base fixture from the `M44-01` scaffold.
- Preserve a 50-card main deck.
- Preserve a 16-trigger profile.
- Force each candidate edge source/target pair to quantity `4`, bounded by
  SQLite `deck_limit`.
- Trim non-trigger scaffold filler cards to keep the draft at 50 cards.
- Keep quantities advisory until `M44-04` validates them.

## Runtime Boundary

This milestone must not:

- create or inject saved decks
- publish drafts to UI deck lists
- promote runtime deck fixtures
- enable bot/playbook integration
- parse live card text
- mutate `GameState`

## Manual Review Boundary

If a draft contains any `M44-02` manual-review card, it must remain blocked by
`manual_card_semantic_review` until a later review/repair milestone clears it.

## Verification

```powershell
python tools\deck\build_third_slice_recipe_draft_model.py
python -m unittest tests.test_third_slice_recipe_draft_model
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M44-03` is done when:

- `25` third-slice recipe drafts are generated
- all drafts are quantity-complete at `50` cards
- all drafts preserve `16` triggers
- manual-review overlap is reported and remains blocking
- saved deck, UI, runtime, and bot promotion remain blocked
- `ready_for_m44_04=true`
