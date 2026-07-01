# Human-Accepted Recipe Artifact Spec

Milestone: `M38-03`

## Purpose

`M38-03` records explicit human acceptance for the repaired accepted seed
recipe `recipe_003`.

This artifact combines:

- the accepted trigger repair from `M37-02`
- the selected grade-profile repair candidate from `M38-02`
- the user's go-ahead message for the review decision

It does not promote the recipe to runtime. It only creates a reviewed artifact
that `M38-04` can inspect.

## Inputs

- `outputs/target_slice/m36_02_deck_recipe_draft_model.json`
- `outputs/target_slice/m37_02_trigger_package_repair_proposal.json`
- `outputs/target_slice/m37_05_revised_recipe_validation_rerun.json`
- `outputs/target_slice/m38_01_accepted_seed_human_review_packet.json`
- `outputs/target_slice/m38_02_grade_profile_repair_candidates.json`
- `data/packs/vanguard_th/cards.sqlite`

## Accepted Decision

Default accepted grade package:

- `m38_02_grade_pkg_001`

The default acceptance text is the user message:

```text
งั้นจัดไป
```

If the user later selects a different package explicitly, regenerate this
artifact with `--accepted-grade-package-id`.

## Output

- `outputs/target_slice/m38_03_human_accepted_recipe_artifact.json`
- `outputs/target_slice/m38_03_human_accepted_recipe_artifact.md`

## Required Checks

The accepted artifact must show:

- main deck count is `50`
- trigger count is `16`
- trigger package is `Critical=4`, `Draw=4`, `Heal=4`, `Stand=4`
- grade profile is `G0=17`, `G1=14`, `G2=11`, `G3=8`
- no blocking validation issues
- human acceptance is recorded
- grade-profile review is cleared
- runtime promotion remains disabled until `M38-04`

## Runtime Boundary

This milestone must not:

- mutate `m36_02_deck_recipe_draft_model.json`
- create a runtime deck
- promote playbook hints
- enable bot integration
- parse live card text
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_human_accepted_recipe_artifact.py
python -m unittest tests.test_human_accepted_recipe_artifact
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M38-03` is done when it emits the reviewed accepted recipe artifact, all
accepted-recipe checks pass, and the next target is `M38-04`.
