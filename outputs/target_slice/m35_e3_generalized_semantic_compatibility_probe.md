# M35-E3 Generalized Semantic / Compatibility Probe

## Selected Target

- Slice: `Classic Core`
- Era preset: `classic_part1`
- Group: `โอราเคิล ทิงค์ แทงค์`
- M34-03 rank: `10`

## Readiness

- Generalized selected-slice contract ready: `True`
- Second-slice semantic/compatibility probe passed: `True`
- Runtime/bot promotion allowed: `False`

## Stage Readiness

- `b1_vocabulary_ready`: `True`
- `b2_semantics_ready`: `True`
- `b3_provider_ready`: `True`
- `b4_phase_c_ready`: `True`
- `c1_resource_ready`: `True`
- `c2_timing_ready`: `True`
- `c3_zone_ready`: `True`
- `c4_output_ready`: `True`
- `c5_package_selection_ready`: `True`

## Stage Summaries

- `b1_vocabulary`: `{'ability_types': 3, 'effects': 13, 'missing_source_terms': 0}`
- `b2_semantic_tags`: `{'card_count': 103, 'cards_with_any_semantic_tag': 103, 'manual_review_count': 7}`
- `b3_requirement_provider`: `{'card_count': 103, 'cards_with_requirements': 92, 'cards_with_providers': 84, 'manual_review_count': 7, 'provider_type_count': 15, 'requirement_type_count': 16}`
- `b4_manual_review_queue`: `{'card_count': 103, 'manual_review_count': 7, 'ready_for_phase_c': True}`
- `c1_pair_graph`: `{'node_count': 103, 'edge_count': 2660, 'manual_review_card_count': 7, 'manual_review_edge_count': 343, 'advisory_edge_count': 2317, 'ready_for_m35_c2': True}`
- `c2_resource`: `{'node_count': 103, 'source_edge_count': 2660, 'resource_relevant_edge_count': 2113, 'manual_review_resource_edge_count': 289, 'missing_recovery_resource_count': 1, 'ready_for_m35_c3': True}`
- `c3_timing`: `{'node_count': 103, 'source_edge_count': 2660, 'timing_relevant_edge_count': 2621, 'manual_review_timing_edge_count': 336, 'ready_for_m35_c4': True}`
- `c4_zone`: `{'node_count': 103, 'source_edge_count': 2660, 'zone_relevant_edge_count': 2428, 'manual_review_zone_edge_count': 343, 'unsupported_required_zone_count': 2, 'ready_for_m35_c5': True}`
- `c5_compatibility`: `{'edge_count': 2660, 'status_counts': {'manual_review_required': 343, 'missing_data': 697, 'mixed': 1361, 'synergy': 259}, 'm35_d1_candidate_edge_count': 259, 'ready_for_m35_d1': True}`

## Boundary

- Advisory probe only.
- Does not create or edit player decks.
- Does not mutate runtime card packs.
- Does not publish bot/runtime playbook data.

## Next

`M35-E4`: bot integration gate for reviewed playbook hints only.
