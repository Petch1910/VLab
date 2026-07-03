"""Build the M72-01 gated fixture artifact materialization audit."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any


ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_four_fixture_scale_decision import load_json  # noqa: E402


OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M71_01_PLAN = OUTPUT_DIR / "m71_01_post_nine_fixture_queue_plan.json"
JSON_OUTPUT = OUTPUT_DIR / "m72_01_gated_fixture_artifact_materialization_audit.json"
MD_OUTPUT = OUTPUT_DIR / "m72_01_gated_fixture_artifact_materialization_audit.md"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


PRIMARY_ARTIFACTS: list[dict[str, str]] = [
    {
        "id": "m39_01_first_schema",
        "fixture_chain": "first_fixture_nova_grappler",
        "milestone": "M39-01",
        "role": "schema_validation",
        "path": "outputs/target_slice/m39_01_offline_fixture_schema_validation.json",
    },
    {
        "id": "m39_02_first_deck_text",
        "fixture_chain": "first_fixture_nova_grappler",
        "milestone": "M39-02",
        "role": "deck_text_export",
        "path": "outputs/target_slice/m39_02_fixture_deck_text_export.json",
    },
    {
        "id": "m39_03_first_headless_smoke",
        "fixture_chain": "first_fixture_nova_grappler",
        "milestone": "M39-03",
        "role": "headless_load_smoke",
        "path": "outputs/target_slice/m39_03_headless_fixture_load_smoke.json",
    },
    {
        "id": "m39_04_first_scale_decision",
        "fixture_chain": "first_fixture_nova_grappler",
        "milestone": "M39-04",
        "role": "scale_decision",
        "path": "outputs/target_slice/m39_04_second_slice_recipe_scale_decision.json",
    },
    {
        "id": "m42_01_second_schema",
        "fixture_chain": "second_fixture_oracle_think_tank",
        "milestone": "M42-01",
        "role": "schema_validation",
        "path": "outputs/target_slice/m42_01_second_fixture_schema_validation.json",
    },
    {
        "id": "m42_02_second_deck_text",
        "fixture_chain": "second_fixture_oracle_think_tank",
        "milestone": "M42-02",
        "role": "deck_text_export",
        "path": "outputs/target_slice/m42_02_second_fixture_deck_text_export.json",
    },
    {
        "id": "m42_03_second_headless_smoke",
        "fixture_chain": "second_fixture_oracle_think_tank",
        "milestone": "M42-03",
        "role": "headless_load_smoke",
        "path": "outputs/target_slice/m42_03_second_fixture_load_smoke.json",
    },
    {
        "id": "m42_04_second_scale_decision",
        "fixture_chain": "second_fixture_oracle_think_tank",
        "milestone": "M42-04",
        "role": "scale_decision",
        "path": "outputs/target_slice/m42_04_multi_fixture_scale_decision.json",
    },
    {
        "id": "m46_01_third_schema",
        "fixture_chain": "third_fixture_bermuda_triangle",
        "milestone": "M46-01",
        "role": "schema_validation",
        "path": "outputs/target_slice/m46_01_third_fixture_schema_validation.json",
    },
    {
        "id": "m46_02_third_deck_text",
        "fixture_chain": "third_fixture_bermuda_triangle",
        "milestone": "M46-02",
        "role": "deck_text_export",
        "path": "outputs/target_slice/m46_02_third_fixture_deck_text_export.json",
    },
    {
        "id": "m46_03_third_headless_smoke",
        "fixture_chain": "third_fixture_bermuda_triangle",
        "milestone": "M46-03",
        "role": "headless_load_smoke",
        "path": "outputs/target_slice/m46_03_third_fixture_load_smoke.json",
    },
    {
        "id": "m46_04_third_scale_decision",
        "fixture_chain": "third_fixture_bermuda_triangle",
        "milestone": "M46-04",
        "role": "scale_decision",
        "path": "outputs/target_slice/m46_04_three_fixture_scale_decision.json",
    },
    {
        "id": "m50_01_fourth_schema",
        "fixture_chain": "fourth_fixture_royal_paladin_g_series",
        "milestone": "M50-01",
        "role": "schema_validation",
        "path": "outputs/target_slice/m50_01_fourth_fixture_schema_validation.json",
    },
    {
        "id": "m50_02_fourth_deck_text",
        "fixture_chain": "fourth_fixture_royal_paladin_g_series",
        "milestone": "M50-02",
        "role": "deck_text_export",
        "path": "outputs/target_slice/m50_02_fourth_fixture_deck_text_export.json",
    },
    {
        "id": "m50_03_fourth_headless_smoke",
        "fixture_chain": "fourth_fixture_royal_paladin_g_series",
        "milestone": "M50-03",
        "role": "headless_load_smoke",
        "path": "outputs/target_slice/m50_03_fourth_fixture_load_smoke.json",
    },
    {
        "id": "m50_04_fourth_scale_decision",
        "fixture_chain": "fourth_fixture_royal_paladin_g_series",
        "milestone": "M50-04",
        "role": "scale_decision",
        "path": "outputs/target_slice/m50_04_four_fixture_scale_decision.json",
    },
    {
        "id": "m54_01_fifth_schema",
        "fixture_chain": "fifth_fixture_gold_paladin",
        "milestone": "M54-01",
        "role": "schema_validation",
        "path": "outputs/target_slice/m54_01_fifth_fixture_schema_validation.json",
    },
    {
        "id": "m54_02_fifth_deck_text",
        "fixture_chain": "fifth_fixture_gold_paladin",
        "milestone": "M54-02",
        "role": "deck_text_export",
        "path": "outputs/target_slice/m54_02_fifth_fixture_deck_text_export.json",
    },
    {
        "id": "m54_03_fifth_headless_smoke",
        "fixture_chain": "fifth_fixture_gold_paladin",
        "milestone": "M54-03",
        "role": "headless_load_smoke",
        "path": "outputs/target_slice/m54_03_fifth_fixture_load_smoke.json",
    },
    {
        "id": "m54_04_fifth_scale_decision",
        "fixture_chain": "fifth_fixture_gold_paladin",
        "milestone": "M54-04",
        "role": "scale_decision",
        "path": "outputs/target_slice/m54_04_five_fixture_scale_decision.json",
    },
    {
        "id": "m58_01_sixth_schema",
        "fixture_chain": "sixth_fixture_shadow_paladin",
        "milestone": "M58-01",
        "role": "schema_validation",
        "path": "outputs/target_slice/m58_01_sixth_fixture_schema_validation.json",
    },
    {
        "id": "m58_02_sixth_deck_text",
        "fixture_chain": "sixth_fixture_shadow_paladin",
        "milestone": "M58-02",
        "role": "deck_text_export",
        "path": "outputs/target_slice/m58_02_sixth_fixture_deck_text_export.json",
    },
    {
        "id": "m58_03_sixth_headless_smoke",
        "fixture_chain": "sixth_fixture_shadow_paladin",
        "milestone": "M58-03",
        "role": "headless_load_smoke",
        "path": "outputs/target_slice/m58_03_sixth_fixture_load_smoke.json",
    },
    {
        "id": "m58_04_sixth_scale_decision",
        "fixture_chain": "sixth_fixture_shadow_paladin",
        "milestone": "M58-04",
        "role": "scale_decision",
        "path": "outputs/target_slice/m58_04_six_fixture_scale_decision.json",
    },
    {
        "id": "m62_01_seventh_schema",
        "fixture_chain": "seventh_fixture_neo_nectar",
        "milestone": "M62-01",
        "role": "schema_validation",
        "path": "outputs/target_slice/m62_01_seventh_fixture_schema_validation.json",
    },
    {
        "id": "m62_02_seventh_deck_text",
        "fixture_chain": "seventh_fixture_neo_nectar",
        "milestone": "M62-02",
        "role": "deck_text_export",
        "path": "outputs/target_slice/m62_02_seventh_fixture_deck_text_export.json",
    },
    {
        "id": "m62_03_seventh_headless_smoke",
        "fixture_chain": "seventh_fixture_neo_nectar",
        "milestone": "M62-03",
        "role": "headless_load_smoke",
        "path": "outputs/target_slice/m62_03_seventh_fixture_load_smoke.json",
    },
    {
        "id": "m62_04_seventh_scale_decision",
        "fixture_chain": "seventh_fixture_neo_nectar",
        "milestone": "M62-04",
        "role": "scale_decision",
        "path": "outputs/target_slice/m62_04_seven_fixture_scale_decision.json",
    },
    {
        "id": "m66_01_eighth_schema",
        "fixture_chain": "eighth_fixture_kagero",
        "milestone": "M66-01",
        "role": "schema_validation",
        "path": "outputs/target_slice/m66_01_eighth_fixture_schema_validation.json",
    },
    {
        "id": "m66_02_eighth_deck_text",
        "fixture_chain": "eighth_fixture_kagero",
        "milestone": "M66-02",
        "role": "deck_text_export",
        "path": "outputs/target_slice/m66_02_eighth_fixture_deck_text_export.json",
    },
    {
        "id": "m66_03_eighth_headless_smoke",
        "fixture_chain": "eighth_fixture_kagero",
        "milestone": "M66-03",
        "role": "headless_load_smoke",
        "path": "outputs/target_slice/m66_03_eighth_fixture_load_smoke.json",
    },
    {
        "id": "m66_04_eighth_scale_decision",
        "fixture_chain": "eighth_fixture_kagero",
        "milestone": "M66-04",
        "role": "scale_decision",
        "path": "outputs/target_slice/m66_04_eight_fixture_scale_decision.json",
    },
    {
        "id": "m70_01_ninth_schema",
        "fixture_chain": "ninth_fixture_aqua_force",
        "milestone": "M70-01",
        "role": "schema_validation",
        "path": "outputs/target_slice/m70_01_ninth_fixture_schema_validation.json",
    },
    {
        "id": "m70_02_ninth_deck_text",
        "fixture_chain": "ninth_fixture_aqua_force",
        "milestone": "M70-02",
        "role": "deck_text_export",
        "path": "outputs/target_slice/m70_02_ninth_fixture_deck_text_export.json",
    },
    {
        "id": "m70_03_ninth_headless_smoke",
        "fixture_chain": "ninth_fixture_aqua_force",
        "milestone": "M70-03",
        "role": "headless_load_smoke",
        "path": "outputs/target_slice/m70_03_ninth_fixture_load_smoke.json",
    },
    {
        "id": "m70_04_ninth_scale_decision",
        "fixture_chain": "ninth_fixture_aqua_force",
        "milestone": "M70-04",
        "role": "scale_decision",
        "path": "outputs/target_slice/m70_04_nine_fixture_scale_decision.json",
    },
    {
        "id": "m71_01_post_nine_queue_plan",
        "fixture_chain": "post_nine_queue",
        "milestone": "M71-01",
        "role": "queue_plan",
        "path": "outputs/target_slice/m71_01_post_nine_fixture_queue_plan.json",
    },
]


RUNTIME_BOUNDARY_FLAGS = [
    "selects_tenth_slice",
    "creates_runtime_fixture",
    "materializes_real_artifacts",
    "saved_deck_injection",
    "ui_deck_list_publication",
    "bot_playbook",
    "g_zone_runtime_enabled",
    "stride_runtime_enabled",
    "aqua_force_battle_order_runtime_enabled",
    "bloom_token_runtime_enabled",
    "lock_runtime_enabled",
    "unlock_runtime_enabled",
    "legion_runtime_enabled",
    "mate_identity_check_enabled",
    "live_card_text_parsing",
    "GameState_mutation",
]


def _normalize_path(path: str) -> str:
    return path.replace("\\", "/")


def _path_exists(path: str, existing_paths: set[str] | None) -> bool:
    normalized = _normalize_path(path)
    if existing_paths is not None:
        return normalized in {_normalize_path(item) for item in existing_paths}
    return (ROOT / path).exists()


def _m71_ready(queue_plan: dict[str, Any]) -> bool:
    summary = queue_plan.get("summary", {})
    decision = queue_plan.get("decision", {})
    next_target = queue_plan.get("next_target", {})
    return (
        queue_plan.get("version") == "M71-01"
        and bool(summary.get("m71_01_ready"))
        and bool(summary.get("ready_for_m72_01"))
        and bool(decision.get("opens_m72_01"))
        and decision.get("recommended_milestone") == "M72-01"
        and next_target.get("milestone") == "M72-01"
    )


def _audit_artifacts(existing_paths: set[str] | None = None) -> list[dict[str, Any]]:
    rows: list[dict[str, Any]] = []
    for item in PRIMARY_ARTIFACTS:
        present = _path_exists(item["path"], existing_paths)
        rows.append(
            {
                "id": item["id"],
                "fixture_chain": item["fixture_chain"],
                "milestone": item["milestone"],
                "role": item["role"],
                "path": item["path"],
                "present": present,
                "status": "materialized" if present else "missing_real_artifact",
            }
        )
    return rows


def _group_chains(artifact_rows: list[dict[str, Any]]) -> list[dict[str, Any]]:
    groups: dict[str, list[dict[str, Any]]] = {}
    for item in artifact_rows:
        groups.setdefault(item["fixture_chain"], []).append(item)

    result: list[dict[str, Any]] = []
    for chain, rows in groups.items():
        required = len(rows)
        materialized = sum(1 for item in rows if item["present"])
        missing = required - materialized
        result.append(
            {
                "fixture_chain": chain,
                "required_artifact_count": required,
                "materialized_artifact_count": materialized,
                "missing_artifact_count": missing,
                "complete": missing == 0,
                "missing_artifacts": [item["id"] for item in rows if not item["present"]],
            }
        )
    return sorted(result, key=lambda item: item["fixture_chain"])


def build_gated_fixture_artifact_materialization_audit(
    queue_plan: dict[str, Any] | None = None,
    existing_paths: set[str] | None = None,
) -> dict[str, Any]:
    queue_plan = queue_plan or load_json(M71_01_PLAN)
    m71_ready = _m71_ready(queue_plan)
    artifact_rows = _audit_artifacts(existing_paths)
    chain_rows = _group_chains(artifact_rows)
    materialized_count = sum(1 for item in artifact_rows if item["present"])
    missing_count = len(artifact_rows) - materialized_count
    chain_complete_count = sum(1 for item in chain_rows if item["complete"])
    chain_missing_count = len(chain_rows) - chain_complete_count
    audit_ready = m71_ready
    ready_for_materialization_queue = audit_ready and missing_count > 0
    ready_for_tenth_slice_gate = audit_ready and missing_count == 0

    scope = {
        "artifact_materialization_audit": True,
        "uses_m71_01_queue_plan": True,
        "primary_json_artifact_audit_only": True,
    }
    for flag in RUNTIME_BOUNDARY_FLAGS:
        scope[flag] = False

    if not m71_ready:
        recommended = {
            "milestone": "M71-repair",
            "task": "Repair post-nine fixture queue plan evidence",
            "reason": "M71-01 does not prove readiness for M72-01.",
        }
    elif missing_count > 0:
        recommended = {
            "milestone": "M72-02",
            "task": "Materialize missing sixth-through-ninth fixture artifact chain",
            "reason": "Primary fixture JSON artifacts are still missing after the fifth fixture chain.",
        }
    else:
        recommended = {
            "milestone": "M73-01",
            "task": "Tenth-slice selection gate",
            "reason": "All primary fixture materialization artifacts are present.",
        }

    return {
        "version": "M72-01",
        "description": "Gated fixture artifact materialization audit",
        "source_inputs": {
            "post_nine_fixture_queue_plan": str(M71_01_PLAN.relative_to(ROOT)),
        },
        "scope": scope,
        "input_summary": {
            "m71_version": queue_plan.get("version"),
            "m71_ready_for_m72": m71_ready,
            "m71_recommended_milestone": queue_plan.get("decision", {}).get("recommended_milestone"),
            "m71_next_target": queue_plan.get("next_target", {}).get("milestone"),
        },
        "artifact_audit": artifact_rows,
        "fixture_chain_audit": chain_rows,
        "decision": {
            "audit_complete": audit_ready,
            "recommended_milestone": recommended["milestone"],
            "recommended_task": recommended["task"],
            "recommended_reason": recommended["reason"],
            "ready_for_m72_02": ready_for_materialization_queue,
            "ready_for_m73_01": ready_for_tenth_slice_gate,
            "tenth_slice_selected_now": False,
            "runtime_promotion_allowed": False,
            "real_artifact_materialization_allowed_in_this_slice": False,
            "requires_explicit_materialization_step": missing_count > 0,
        },
        "summary": {
            "m72_01_ready": audit_ready,
            "required_artifact_count": len(artifact_rows),
            "materialized_artifact_count": materialized_count,
            "missing_artifact_count": missing_count,
            "complete_fixture_chain_count": chain_complete_count,
            "incomplete_fixture_chain_count": chain_missing_count,
            "ready_for_m72_02": ready_for_materialization_queue,
            "ready_for_m73_01": ready_for_tenth_slice_gate,
            "blocking_issue_count": 0 if audit_ready else 1,
            "issue_count": 0 if audit_ready else 1,
        },
        "next_target": {
            "milestone": recommended["milestone"],
            "task": recommended["task"],
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    decision = report["decision"]
    lines = [
        "# M72-01 Gated Fixture Artifact Materialization Audit",
        "",
        "## Summary",
        "",
        f"- Required primary JSON artifacts: `{summary['required_artifact_count']}`",
        f"- Materialized artifacts: `{summary['materialized_artifact_count']}`",
        f"- Missing artifacts: `{summary['missing_artifact_count']}`",
        f"- Complete fixture chains: `{summary['complete_fixture_chain_count']}`",
        f"- Incomplete fixture chains: `{summary['incomplete_fixture_chain_count']}`",
        f"- Ready for M72-02: `{summary['ready_for_m72_02']}`",
        f"- Ready for M73-01: `{summary['ready_for_m73_01']}`",
        "",
        "## Chain Audit",
        "",
    ]
    for chain in report["fixture_chain_audit"]:
        lines.append(
            "- `{chain}` materialized `{done}/{required}` missing `{missing}` complete=`{complete}`".format(
                chain=chain["fixture_chain"],
                done=chain["materialized_artifact_count"],
                required=chain["required_artifact_count"],
                missing=chain["missing_artifact_count"],
                complete=chain["complete"],
            )
        )
    lines.extend(
        [
            "",
            "## Missing Primary Artifacts",
            "",
        ]
    )
    missing = [item for item in report["artifact_audit"] if not item["present"]]
    if missing:
        for item in missing:
            lines.append(f"- `{item['milestone']}` `{item['role']}` -> `{item['path']}`")
    else:
        lines.append("- None.")
    lines.extend(
        [
            "",
            "## Decision",
            "",
            f"- Recommended milestone: `{decision['recommended_milestone']}`",
            f"- Recommended task: `{decision['recommended_task']}`",
            f"- Tenth slice selected now: `{decision['tenth_slice_selected_now']}`",
            f"- Runtime promotion allowed: `{decision['runtime_promotion_allowed']}`",
            f"- Real artifact materialization allowed in this slice: `{decision['real_artifact_materialization_allowed_in_this_slice']}`",
            "",
            "## Boundary",
            "",
            "- Audit primary JSON artifacts only.",
            "- Do not write or materialize missing artifacts in this slice.",
            "- Do not select a tenth slice.",
            "- Do not create runtime fixtures.",
            "- Do not publish saved decks or UI decks.",
            "- Do not enable bot/playbooks.",
            "- Do not enable G Zone, Stride, Aqua Force battle-order, Bloom/token, Lock, Unlock, Legion, or Mate runtime.",
            "- Do not parse live card text.",
            "- Do not mutate GameState.",
            "",
            "## Next",
            "",
            f"`{report['next_target']['milestone']}`: {report['next_target']['task']}.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Any = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M72-01 gated fixture artifact materialization audit.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Any = None) -> int:
    args = parse_args(argv)
    report = build_gated_fixture_artifact_materialization_audit()
    json_path = args.output_dir / JSON_OUTPUT.name
    md_path = args.output_dir / MD_OUTPUT.name
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M72-01 gated fixture artifact materialization audit wrote {json_path}")
    print(f"M72-01 gated fixture artifact materialization audit summary wrote {md_path}")
    print(
        "missing_artifacts={missing} next={next_target}".format(
            missing=report["summary"]["missing_artifact_count"],
            next_target=report["next_target"]["milestone"],
        )
    )
    return 0 if report["summary"]["m72_01_ready"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
