"""Build second-slice fixture readiness evidence for M35-E2.

This reuses the narrow M35-A3 fixture builder against the M35-E1 selected
slice. It remains an offline planning/verification artifact and does not
mutate runtime card data or create player decks.
"""

from __future__ import annotations

import argparse
import json
import sys
from copy import deepcopy
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_first_slice_deck_fixtures import build_fixtures  # noqa: E402
from tools.deck.select_second_target_slice import OUTPUT_DIR  # noqa: E402


SECOND_SLICE_REPORT = OUTPUT_DIR / "m35_e1_second_target_slice_report.json"

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_second_slice_report(path: Path = SECOND_SLICE_REPORT) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"M35-E1 report not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def rewrite_fixture_ids(fixtures: list[dict[str, Any]]) -> list[dict[str, Any]]:
    rewritten = deepcopy(fixtures)
    for fixture in rewritten:
        fixture_id = str(fixture.get("fixture_id", ""))
        fixture["fixture_id"] = fixture_id.replace(
            "classic_core_selected_group_",
            "classic_core_second_slice_",
            1,
        )
    return rewritten


def build_readiness_report(second_slice_report: dict[str, Any]) -> dict[str, Any]:
    fixtures = build_fixtures(second_slice_report)
    fixture_list = rewrite_fixture_ids(fixtures["fixtures"])
    all_expectations_met = all(bool(fixture.get("expectation_met")) for fixture in fixture_list)
    policy_reuse_decision = (
        "reuse_classic_core_policy_for_second_slice"
        if all_expectations_met
        else "blocked_until_fixture_failures_are_resolved"
    )

    fixture_policy = deepcopy(fixtures["fixture_policy"])
    fixture_policy["policy_reuse_decision"] = policy_reuse_decision
    fixture_policy["new_format_or_mechanic_fixtures_required"] = False
    fixture_policy["reason"] = (
        "The M35-E1 second slice uses the same Classic Core era preset and "
        "no new mechanic family beyond the first-slice policy."
    )

    return {
        "version": "M35-E2",
        "description": "Second-slice fixture/format readiness check before semantic scale-out",
        "based_on": str(SECOND_SLICE_REPORT.relative_to(ROOT)),
        "selected_target": second_slice_report["selected_target"],
        "previous_target": second_slice_report.get("previous_target", {}),
        "fixture_policy": fixture_policy,
        "fixtures": fixture_list,
        "readiness": {
            "all_fixture_expectations_met": all_expectations_met,
            "selected_group_fixture_ready": all_expectations_met,
            "classic_core_policy_reusable": policy_reuse_decision == "reuse_classic_core_policy_for_second_slice",
            "semantic_scaleout_ready": all_expectations_met,
            "runtime_or_bot_promotion_allowed": False,
        },
        "runtime_boundary": {
            "offline_fixture_readiness_only": True,
            "does_not_create_deck": True,
            "does_not_mutate_runtime_pack": True,
            "does_not_publish_to_bot": True,
        },
        "next_target": {
            "milestone": "M35-E3",
            "task": "Generalize semantic and compatibility tools after two Classic Core slices prove repeated patterns",
            "must_create": [
                "second-slice semantic extraction plan",
                "generalized selected-slice input contract",
                "decision on whether B-D tools become parameterized by selected report path",
                "no bot/runtime use until reviewed outputs pass the same gates",
            ],
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    target = report["selected_target"]
    readiness = report["readiness"]
    lines = [
        "# M35-E2 Second Slice Fixture Readiness",
        "",
        "## Selected Target",
        "",
        f"- Slice: `{target['slice']}`",
        f"- Era preset: `{target['era_preset']}`",
        f"- Group: `{target['group']}`",
        f"- M34-03 rank: `{target['rank']}`",
        "",
        "## Readiness",
        "",
        f"- All fixture expectations met: `{readiness['all_fixture_expectations_met']}`",
        f"- Classic Core policy reusable: `{readiness['classic_core_policy_reusable']}`",
        f"- Semantic scale-out ready: `{readiness['semantic_scaleout_ready']}`",
        f"- Runtime/bot promotion allowed: `{readiness['runtime_or_bot_promotion_allowed']}`",
        "",
        "## Fixtures",
        "",
    ]
    for fixture in report["fixtures"]:
        validation = fixture["validation"]
        status = "accepted" if validation["accepted"] else "rejected"
        lines.append(
            f"- `{fixture['fixture_id']}` expected `{fixture['expected']}` -> "
            f"`{status}`; reasons: `{', '.join(validation['reasons']) or 'none'}`"
        )
    lines.extend(
        [
            "",
            "## Decision",
            "",
            "`M35-E2` reuses the Classic Core policy for the second slice because the",
            "selected group uses the same era preset and all fixture expectations pass.",
            "",
            "## Next",
            "",
            "`M35-E3`: generalize semantic/compatibility tooling around a selected-report",
            "input contract before scaling more groups.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M35-E2 second-slice fixture readiness report.")
    parser.add_argument("--second-slice-report", type=Path, default=SECOND_SLICE_REPORT)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    second_slice_report = load_second_slice_report(args.second_slice_report)
    report = build_readiness_report(second_slice_report)
    json_path = args.output_dir / "m35_e2_second_slice_fixture_readiness.json"
    md_path = args.output_dir / "m35_e2_second_slice_fixture_readiness.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M35-E2 fixture readiness wrote {json_path}")
    print(f"M35-E2 fixture readiness summary wrote {md_path}")
    print(f"expectations_met={report['readiness']['all_fixture_expectations_met']}")
    return 0 if report["readiness"]["all_fixture_expectations_met"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
