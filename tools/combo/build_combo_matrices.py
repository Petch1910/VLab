"""Build review-friendly matrices from offline combo discovery reports."""

from __future__ import annotations

import argparse
import csv
import json
import sys
from pathlib import Path
from typing import Any, Sequence

ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.combo.discover_clan_combos import ERA_SET_SPECS, OUTPUT_DIR


DEFAULT_PRESETS = tuple(ERA_SET_SPECS.keys())


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_preset_reports(report_dir: Path, presets: Sequence[str] = DEFAULT_PRESETS) -> list[tuple[str, dict[str, Any]]]:
    reports: list[tuple[str, dict[str, Any]]] = []
    for preset in presets:
        path = report_dir / f"{preset}_clan_combos.json"
        if not path.exists():
            continue
        reports.append((preset, json.loads(path.read_text(encoding="utf-8"))))
    return reports


def group_name(group_report: dict[str, Any]) -> str:
    return group_report.get("group") or group_report.get("clan") or "Unknown"


def group_field(group_report: dict[str, Any]) -> str:
    return group_report.get("group_field") or "unknown"


def build_era_summary_rows(reports: Sequence[tuple[str, dict[str, Any]]]) -> list[dict[str, Any]]:
    rows: list[dict[str, Any]] = []
    for preset, report in reports:
        groups = report.get("clans", [])
        rows.append(
            {
                "preset": preset,
                "card_count": report.get("card_count", 0),
                "group_count": report.get("group_count", report.get("clan_count", 0)),
                "nonempty_group_count": sum(1 for group in groups if group.get("combo_pair_count", 0) > 0),
                "candidate_pair_count": sum(group.get("combo_pair_count", 0) for group in groups),
                "missing_set_count": len(report.get("scope", {}).get("missing_set_codes", [])),
            }
        )
    return rows


def build_group_matrix_rows(
    reports: Sequence[tuple[str, dict[str, Any]]],
    value_key: str,
) -> list[dict[str, Any]]:
    preset_names = [preset for preset, _ in reports]
    grouped: dict[str, dict[str, Any]] = {}
    for preset, report in reports:
        for group in report.get("clans", []):
            name = group_name(group)
            row = grouped.setdefault(name, {"group": name, "group_field": group_field(group)})
            if value_key == "combo_pair_count":
                row[preset] = group.get("combo_pair_count", 0)
            elif value_key == "card_count":
                row[preset] = group.get("card_count", 0)
            elif value_key == "top_pair_score":
                top_pairs = group.get("top_pairs", [])
                row[preset] = top_pairs[0].get("score", 0) if top_pairs else 0
            else:
                raise ValueError(f"Unsupported matrix value key: {value_key}")

    rows = []
    for name in sorted(grouped):
        row = grouped[name]
        for preset in preset_names:
            row.setdefault(preset, 0)
        rows.append(row)
    return rows


def build_synergy_tag_matrix_rows(reports: Sequence[tuple[str, dict[str, Any]]]) -> tuple[list[str], list[dict[str, Any]]]:
    tags: set[str] = set()
    rows: list[dict[str, Any]] = []
    for preset, report in reports:
        for group in report.get("clans", []):
            row: dict[str, Any] = {
                "preset": preset,
                "group": group_name(group),
                "group_field": group_field(group),
                "top_pair_count": len(group.get("top_pairs", [])),
            }
            for pair in group.get("top_pairs", []):
                for tag in pair.get("synergy_tags", []):
                    tags.add(tag)
                    row[tag] = row.get(tag, 0) + 1
            rows.append(row)

    ordered_tags = sorted(tags)
    for row in rows:
        for tag in ordered_tags:
            row.setdefault(tag, 0)
    rows.sort(key=lambda item: (item["preset"], item["group"]))
    return ordered_tags, rows


def write_csv(path: Path, rows: Sequence[dict[str, Any]], fieldnames: Sequence[str]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8-sig", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        for row in rows:
            writer.writerow({field: row.get(field, "") for field in fieldnames})


def write_matrices(report_dir: Path, output_dir: Path, presets: Sequence[str] = DEFAULT_PRESETS) -> dict[str, Any]:
    reports = load_preset_reports(report_dir, presets)
    preset_names = [preset for preset, _ in reports]
    output_dir.mkdir(parents=True, exist_ok=True)

    era_summary = build_era_summary_rows(reports)
    group_candidate_rows = build_group_matrix_rows(reports, "combo_pair_count")
    group_card_rows = build_group_matrix_rows(reports, "card_count")
    group_top_score_rows = build_group_matrix_rows(reports, "top_pair_score")
    synergy_tags, synergy_rows = build_synergy_tag_matrix_rows(reports)

    era_summary_path = output_dir / "combo_matrix_era_summary.csv"
    group_candidates_path = output_dir / "combo_matrix_group_candidates.csv"
    group_cards_path = output_dir / "combo_matrix_group_cards.csv"
    top_scores_path = output_dir / "combo_matrix_top_pair_scores.csv"
    synergy_path = output_dir / "combo_matrix_synergy_tags.csv"
    summary_path = output_dir / "combo_matrix_summary.json"

    write_csv(
        era_summary_path,
        era_summary,
        ["preset", "card_count", "group_count", "nonempty_group_count", "candidate_pair_count", "missing_set_count"],
    )
    write_csv(group_candidates_path, group_candidate_rows, ["group", "group_field", *preset_names])
    write_csv(group_cards_path, group_card_rows, ["group", "group_field", *preset_names])
    write_csv(top_scores_path, group_top_score_rows, ["group", "group_field", *preset_names])
    write_csv(synergy_path, synergy_rows, ["preset", "group", "group_field", "top_pair_count", *synergy_tags])

    summary = {
        "schema_version": 1,
        "generator": "tools/combo/build_combo_matrices.py",
        "source_report_dir": str(report_dir),
        "preset_count": len(reports),
        "presets": preset_names,
        "matrices": {
            "era_summary_csv": str(era_summary_path),
            "group_candidates_csv": str(group_candidates_path),
            "group_cards_csv": str(group_cards_path),
            "top_pair_scores_csv": str(top_scores_path),
            "synergy_tags_csv": str(synergy_path),
        },
        "era_summary": era_summary,
        "note": "offline advisory matrices; synergy tag counts are based on top_pairs stored in each report",
    }
    summary_path.write_text(json.dumps(summary, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    return summary


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build combo discovery matrix CSV/JSON artifacts.")
    parser.add_argument("--report-dir", type=Path, default=OUTPUT_DIR)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    parser.add_argument("--presets", nargs="+", default=list(DEFAULT_PRESETS))
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    summary = write_matrices(args.report_dir, args.output_dir, args.presets)
    print(
        "Combo matrices wrote "
        + str(args.output_dir)
        + f" | presets={summary['preset_count']}"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
