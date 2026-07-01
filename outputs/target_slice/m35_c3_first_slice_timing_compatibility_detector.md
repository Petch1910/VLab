# M35-C3 Timing Compatibility Detector

## Selected Target

- Slice: `Classic Core`
- Era preset: `classic_part1`
- Group: `โนว่า เกรปเปอร์`

## Summary

- Nodes: `112`
- Source graph edges: `3919`
- Timing-relevant edges: `3876`
- Manual-review timing edges: `363`
- Timing window types: `7`
- Ready for M35-C4: `True`

## Verdict Counts

- `provider_after_consumer_window`: `990`
- `same_window_requires_ordering`: `804`
- `source_timing_unknown_or_static`: `943`
- `target_timing_not_constrained`: `317`
- `timing_can_precede`: `822`

## Timing Window Counts

- `attack_declaration`: `40`
- `attack_hit`: `26`
- `boost_step`: `18`
- `end_phase`: `6`
- `on_call`: `11`
- `on_ride`: `12`
- `trigger_check`: `19`

## Timing Findings To Review First

- `BT01-064TH->BT01-032TH` verdict=`provider_after_consumer_window` gap=`-40` source=`trigger_check` target=`boost_step, end_phase, on_ride, trigger_check`
- `BT01-064TH->BT06-042TH` verdict=`provider_after_consumer_window` gap=`-40` source=`trigger_check` target=`attack_declaration, attack_hit, boost_step, on_ride`
- `BT01-064TH->EB01-016TH` verdict=`provider_after_consumer_window` gap=`-40` source=`trigger_check` target=`attack_declaration, on_ride`
- `BT01-064TH->EB01-020TH` verdict=`provider_after_consumer_window` gap=`-40` source=`trigger_check` target=`boost_step, on_ride, trigger_check`
- `BT01-064TH->EB04-019TH` verdict=`provider_after_consumer_window` gap=`-40` source=`trigger_check` target=`attack_declaration, on_ride`
- `BT01-064TH->EB04-027TH` verdict=`provider_after_consumer_window` gap=`-40` source=`trigger_check` target=`attack_declaration, attack_hit, boost_step, on_ride`
- `BT01-064TH->EB04-034TH` verdict=`provider_after_consumer_window` gap=`-40` source=`trigger_check` target=`boost_step, end_phase, on_ride, trigger_check`
- `BT01-064TH->EB04-035TH` verdict=`provider_after_consumer_window` gap=`-40` source=`trigger_check` target=`boost_step, end_phase, on_ride, trigger_check`
- `BT01-065TH->BT01-032TH` verdict=`provider_after_consumer_window` gap=`-40` source=`trigger_check` target=`boost_step, end_phase, on_ride, trigger_check`
- `BT01-065TH->EB01-020TH` verdict=`provider_after_consumer_window` gap=`-40` source=`trigger_check` target=`boost_step, on_ride, trigger_check`
- `BT01-065TH->EB04-034TH` verdict=`provider_after_consumer_window` gap=`-40` source=`trigger_check` target=`boost_step, end_phase, on_ride, trigger_check`
- `BT01-065TH->EB04-035TH` verdict=`provider_after_consumer_window` gap=`-40` source=`trigger_check` target=`boost_step, end_phase, on_ride, trigger_check`

## Scope

- Advisory timing detector only.
- Uses card-level timing tags as proxy until structured effect timing exists.
- No zone/target compatibility verdict yet.
- No deck skeleton or bot playbook promotion.
