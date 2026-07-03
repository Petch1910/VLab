"""Build a read-only M57-03 human acceptance request packet."""

from __future__ import annotations

import argparse
import csv
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M57_02_SELECTION = OUTPUT_DIR / "m57_02_sixth_slice_human_selected_recipe_artifact.json"
JSON_OUTPUT = OUTPUT_DIR / "m57_03_sixth_slice_human_acceptance_request_packet.json"
MD_OUTPUT = OUTPUT_DIR / "m57_03_sixth_slice_human_acceptance_request_packet.md"
CSV_OUTPUT = OUTPUT_DIR / "m57_03_sixth_slice_human_acceptance_request_packet.csv"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _acceptance_command_template() -> str:
    return (
        "python tools\\deck\\build_sixth_slice_human_accepted_repair_artifact.py "
        '--acceptance-text "<explicit user/team acceptance text>"'
    )


def _decision_rows(selection: dict[str, Any]) -> list[dict[str, Any]]:
    rows: list[dict[str, Any]] = []
    for option in selection.get("decision_options", []):
        option_id = option.get("option_id", "")
        is_acceptance = option_id == "accept_recipe_repairs_and_keep_g_zone_deferred_for_validation_rerun"
        rows.append(
            {
                "option_id": option_id,
                "label": option.get("label", ""),
                "effect": option.get("effect", ""),
                "m57_03_action": "run_acceptance_artifact" if is_acceptance else "do_not_run_m57_03",
                "command_template": _acceptance_command_template() if is_acceptance else "",
            }
        )
    return rows


def build_sixth_slice_human_acceptance_request_packet(
    selected_artifact: dict[str, Any] | None = None,
) -> dict[str, Any]:
    selected_artifact = selected_artifact or load_json(M57_02_SELECTION)
    selection = selected_artifact.get("selection", {})
    summary = selected_artifact.get("summary", {})
    pair = selection.get("pair", {})
    issues: list[dict[str, Any]] = []

    if selected_artifact.get("version") != "M57-02":
        issues.append(
            {
                "code": "m57_02_selection_artifact_version_mismatch",
                "severity": "blocker",
                "message": "M57-03 acceptance request requires the real M57-02 selected artifact.",
                "details": {"version": selected_artifact.get("version", "")},
            }
        )
    if not summary.get("records_human_selection"):
        issues.append(
            {
                "code": "m57_02_selection_not_recorded",
                "severity": "blocker",
                "message": "M57-02 did not record human selection.",
                "details": {"selected_review_item_id": selection.get("selected_review_item_id", "")},
            }
        )
    if summary.get("records_human_acceptance"):
        issues.append(
            {
                "code": "m57_02_already_records_acceptance",
                "severity": "blocker",
                "message": "M57-02 selected artifact must not already record acceptance.",
                "details": {"selected_recipe_id": summary.get("selected_recipe_id", "")},
            }
        )
    if not summary.get("ready_for_m57_03"):
        issues.append(
            {
                "code": "m57_02_not_ready_for_m57_03",
                "severity": "blocker",
                "message": "M57-02 selected artifact is not ready for M57-03.",
                "details": {"summary": summary},
            }
        )

    decision_rows = _decision_rows(selection)
    return {
        "version": "M57-03-prerequisite",
        "description": "Sixth-slice human acceptance request packet",
        "source_inputs": {
            "m57_02_human_selected_recipe_artifact": str(M57_02_SELECTION.relative_to(ROOT)),
        },
        "scope": {
            "read_only_acceptance_request": True,
            "records_human_selection": bool(summary.get("records_human_selection")),
            "records_human_acceptance": False,
            "records_g_zone_decision": False,
            "creates_m57_03_artifact": False,
            "declares_recipe_valid": False,
            "changes_recipe_draft_file": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
        },
        "selected_recipe": {
            "selected_review_item_id": selection.get("selected_review_item_id", ""),
            "recipe_id": selection.get("recipe_id", ""),
            "source_candidate_edge": selection.get("source_candidate_edge", ""),
            "source_card_id": pair.get("source_card_id", ""),
            "source_name_th": pair.get("source_name_th", ""),
            "target_card_id": pair.get("target_card_id", ""),
            "target_name_th": pair.get("target_name_th", ""),
            "selection_text": selection.get("selection_text", ""),
            "manual_overlap_package_id": selection.get("manual_overlap_package_id", ""),
            "manual_substitution_count": len(selection.get("manual_substitutions", [])),
            "grade_profile_package_id": selection.get("grade_profile_package_id", ""),
            "grade_counts_after": selection.get("grade_counts_after", {}),
            "g_zone_package_id": selection.get("g_zone_package_id", ""),
            "g_zone_deferred": bool(selection.get("g_zone_deferred")),
            "ready_for_human_repair_review": bool(selection.get("ready_for_human_repair_review")),
        },
        "acceptance_policy": {
            "requires_explicit_acceptance_text": True,
            "selection_text_is_not_acceptance_text": True,
            "acceptance_records_repair_preview_only": True,
            "acceptance_does_not_record_g_zone_decision": True,
            "acceptance_does_not_declare_recipe_valid": True,
            "runtime_promotion_allowed": False,
            "m57_03_command_template": _acceptance_command_template(),
        },
        "decision_options": decision_rows,
        "issues": issues,
        "decision": {
            "acceptance_request_ready": not issues,
            "human_acceptance_recorded": False,
            "recommended_milestone": "M57-03" if not issues else "M57-02-repair",
            "recommended_task": (
                "User/team provides non-empty acceptance_text for the selected recipe repairs"
                if not issues
                else "Repair M57-02 selection artifact readiness before acceptance"
            ),
            "runtime_promotion_allowed": False,
        },
        "summary": {
            "selected_review_item_id": selection.get("selected_review_item_id", ""),
            "selected_recipe_id": selection.get("recipe_id", ""),
            "source_card_id": pair.get("source_card_id", ""),
            "target_card_id": pair.get("target_card_id", ""),
            "decision_option_count": len(decision_rows),
            "acceptance_option_count": sum(1 for row in decision_rows if row["m57_03_action"] == "run_acceptance_artifact"),
            "issue_count": len(issues),
            "acceptance_request_ready": not issues,
            "human_selection_recorded": bool(summary.get("records_human_selection")),
            "human_acceptance_recorded": False,
            "ready_for_m57_03": not issues,
        },
        "next_target": {
            "milestone": "M57-03" if not issues else "M57-02-repair",
            "task": (
                "Sixth-slice human-accepted repair artifact"
                if not issues
                else "Repair sixth-slice selected recipe artifact"
            ),
        },
    }


def _write_text_lf(path: Path, text: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="\n") as handle:
        handle.write(text)


def write_json(report: dict[str, Any], path: Path) -> None:
    _write_text_lf(path, json.dumps(report, ensure_ascii=False, indent=2) + "\n")


def write_csv(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    fieldnames = [
        "option_id",
        "label",
        "m57_03_action",
        "command_template",
    ]
    with path.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames, lineterminator="\n")
        writer.writeheader()
        for item in report["decision_options"]:
            writer.writerow({field: item.get(field, "") for field in fieldnames})


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    selected = report["selected_recipe"]
    policy = report["acceptance_policy"]
    lines = [
        "# M57-03 Sixth-Slice Human Acceptance Request Packet",
        "",
        "## Summary",
        "",
        f"- Selected review item: `{summary['selected_review_item_id']}`",
        f"- Selected recipe: `{summary['selected_recipe_id']}`",
        f"- Pair: `{summary['source_card_id']}` -> `{summary['target_card_id']}`",
        f"- Decision options: `{summary['decision_option_count']}`",
        f"- Acceptance options: `{summary['acceptance_option_count']}`",
        f"- Issues: `{summary['issue_count']}`",
        f"- Acceptance request ready: `{summary['acceptance_request_ready']}`",
        f"- Human acceptance recorded: `{summary['human_acceptance_recorded']}`",
        "",
        "## Selected Repair Context",
        "",
        f"- Selection text: `{selected['selection_text']}`",
        f"- Manual package: `{selected['manual_overlap_package_id']}`",
        f"- Manual substitutions: `{selected['manual_substitution_count']}`",
        f"- Grade package: `{selected['grade_profile_package_id']}`",
        f"- Grade counts after: `{selected['grade_counts_after']}`",
        f"- G Zone package: `{selected['g_zone_package_id']}`",
        f"- G Zone deferred: `{selected['g_zone_deferred']}`",
        "",
        "## Required User/Team Action",
        "",
        "If the selected repair is accepted, provide explicit non-empty acceptance text and run:",
        "",
        "```powershell",
        policy["m57_03_command_template"],
        "```",
        "",
        "## Decision Options",
        "",
    ]
    for option in report["decision_options"]:
        lines.append(
            "- `{option}` action=`{action}` label=`{label}`".format(
                option=option["option_id"],
                action=option["m57_03_action"],
                label=option["label"],
            )
        )
    lines.extend(["", "## Issues", ""])
    if report["issues"]:
        for issue in report["issues"]:
            lines.append(f"- `{issue['code']}` severity=`{issue['severity']}` details=`{issue['details']}`")
    else:
        lines.append("- None")
    lines.extend(
        [
            "",
            "## Boundary",
            "",
            "- This packet does not record acceptance.",
            "- This packet does not record a G Zone / Stride decision.",
            "- This packet does not create the real M57-03 artifact.",
            "- This packet does not declare the recipe valid.",
            "- This packet does not create a runtime fixture.",
            "- This packet does not publish saved decks, UI deck lists, or bot playbooks.",
            "- This packet does not mutate GameState.",
            "",
            "## Next",
            "",
            f"`{report['next_target']['milestone']}`: {report['next_target']['task']}.",
            "",
        ]
    )
    _write_text_lf(path, "\n".join(lines))


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M57-03 human acceptance request packet.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_sixth_slice_human_acceptance_request_packet()
    json_path = args.output_dir / JSON_OUTPUT.name
    md_path = args.output_dir / MD_OUTPUT.name
    csv_path = args.output_dir / CSV_OUTPUT.name
    write_json(report, json_path)
    write_markdown(report, md_path)
    write_csv(report, csv_path)
    print(f"M57-03 acceptance request packet wrote {json_path}")
    print(f"M57-03 acceptance request summary wrote {md_path}")
    print(f"M57-03 acceptance request CSV wrote {csv_path}")
    print(
        "acceptance_request_ready={ready} selected_recipe={recipe} next={next_target}".format(
            ready=report["summary"]["acceptance_request_ready"],
            recipe=report["summary"]["selected_recipe_id"],
            next_target=report["next_target"]["milestone"],
        )
    )
    return 0 if report["summary"]["acceptance_request_ready"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
