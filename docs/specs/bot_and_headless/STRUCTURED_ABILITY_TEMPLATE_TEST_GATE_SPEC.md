# Structured Ability Template Test Gate Spec

## Status

Implemented in `M26-04`.

## Purpose

Keep bot and automation work from using structured ability templates that look
available in schema data but do not yet have runtime regression coverage.

This gate is correctness-first. It does not add new ability behavior. It only
declares which template pieces are allowed to run as automated structured
ability logic today, and sends unsupported pieces to the existing manual
fallback path.

## Runtime Surface

`StructuredAbilityTemplateAutomationGate.Validate(ability)` returns:

- accepted automated ability when every cost, target, effect, and required
  duration template is in the tested coverage set
- rejected manual fallback when the ability uses an unsupported template and
  `manual_fallback = true`
- rejected non-fallback when the ability uses an unsupported template and
  `manual_fallback = false`

`StructuredAbilityTemplateAutomationGate.CreateCurrentCoverageReport()` returns
a JSON-serializable coverage report listing the template ids and tests that
justify automation.

`StructuredAbilityFixtureRunner.Run(fixture)` calls the gate before executing
cost, target, effect, or modifier templates.

## Automated Template Set

The current automated set is limited to templates already covered by EditMode
or Python tests:

- costs: `none`, `counter_blast`, `soul_blast`, `energy_blast`,
  `once_per_turn`, `once_per_fight`
- targets: `none`, `self`, `unit`, `card` for supported visible zones only
- effects: `draw`, `move_zone`, `counter_blast`, `counter_charge`,
  `soul_blast`, `soul_charge`, `power_plus`, `critical_plus`
- modifier durations: `until_end_of_turn`, `until_end_of_battle`
- non-modifier duration marker: `instant`

Manual fallback remains required for:

- `discard`
- `manual` effects
- unknown effect/cost/target ids
- target filters, circle targeting, hidden/private target zones
- untested modifier durations such as `continuous` or `manual`

## Boundaries

M26-04 does not:

- parse live card text
- call an LLM during a match
- add new effect templates
- mutate `GameState` from bot or UI
- change actual RulesCore command legality
- replace manual fallback with automation

## Verification

EditMode coverage verifies:

- the current template coverage report is valid
- automated coverage entries list tests
- a missing coverage test rejects the report
- covered draw and power-plus abilities pass the gate
- manual/unknown/untested templates require manual fallback
- fixture execution stops at the gate before unsupported effects run
- the M12 smoke pack still uses only covered templates
- JSON round-trip of gate outputs

## Next Work

`M26-05` keeps the ban on LLM/runtime text parsing for live effect resolution.
