# Offline Combo Discovery Output Spec

## Scope

`M14-07` expands the offline combo discovery report from a helper summary into
an exportable advisory artifact. It still does not execute actions or claim full
combo legality beyond the current visible battle/search helpers.

## Inputs

Same as `ComboDiscoveryRunner.Analyze`:

- visible `GameState`
- player and opponent indices
- `ICardRepository`
- `BotPlaybookLibrary`
- trigger pool counts
- battle search options

## Output

The report keeps the M9 fields and adds:

- `combo_lines`
- `source_event_count`

Each `ComboDiscoveryLine` includes:

- line id
- rank
- battle candidate id
- total score
- guard pressure
- trigger pressure
- trigger risk
- replay reference
- explanation

## Replay Reference

The replay reference is a stable text pointer:

```text
<game_id>:events:<source_event_count>:candidate:<candidate_id>
```

It references the source event-log position used to create the advisory report.
It does not execute or append actions.

## Hidden-State And Mutation Policy

- Caller decides whether to pass true state or masked player view.
- Report generation never mutates `GameState`.
- Report generation never consumes RNG.
- Report generation never applies trigger outcomes.
- Combo line explanations are inherited from v2 battle search and avoid card
  ids/instance ids.

## Verification

EditMode tests must cover:

- report includes combo lines and replay references
- JSON round-trip preserves combo line fields
- repeated reports are deterministic
- source `GameState` is unchanged

## Non-Goals

- No JSONL batch exporter yet.
- No file writer yet.
- No full action replay synthesis yet.
