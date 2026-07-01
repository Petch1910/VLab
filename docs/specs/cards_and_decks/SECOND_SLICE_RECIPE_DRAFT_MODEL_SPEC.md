# Second-Slice Recipe Draft Model Spec

Milestone: `M40-02`

## Purpose

`M40-02` converts the `M40-01` Oracle Think Tank review packet into explicit,
reviewable recipe drafts.

The second slice does not have the first-slice M35-D2/D3 skeleton artifacts.
Therefore these drafts are pair-anchored and fixture-scaffolded:

- use the top reviewed candidate edges from `M40-01`
- start from the accepted M35-E2 second-slice fixture
- force the candidate source/target cards into the draft
- trim non-trigger fixture filler cards to keep a 50-card main deck

The drafts are advisory inputs for `M40-03`; they are not legal deck claims and
must not be used as runtime decks.

## Inputs

- `outputs/target_slice/m40_01_second_slice_review_packet.json`
- `outputs/target_slice/m35_e2_second_slice_fixture_readiness.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m40_02_second_slice_recipe_draft_model.json`
- `outputs/target_slice/m40_02_second_slice_recipe_draft_model.md`

## Draft Rules

- Use `M40-01` candidate edges as advisory pair anchors.
- Limit this slice to the first `25` candidate-edge drafts.
- Use the accepted second-slice fixture as a scaffold.
- Preserve a 50-card main deck.
- Preserve a 16-trigger profile.
- Use SQLite `deck_limit` as the maximum quantity guard.
- Keep quantities advisory until `M40-03` validates them.

## Runtime Boundary

This milestone must not:

- create or inject saved decks
- publish drafts to UI deck lists
- promote runtime deck fixtures
- enable bot/playbook integration
- parse live card text
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_second_slice_recipe_draft_model.py
python -m unittest tests.test_second_slice_recipe_draft_model
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M40-02` is done when:

- `25` second-slice recipe drafts are generated
- all drafts are quantity-complete at `50` cards
- all drafts preserve `16` triggers
- saved deck, UI, runtime, and bot promotion remain blocked
- `ready_for_m40_03=true`

