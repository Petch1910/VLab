"""Validate structured Vanguard ability data files.

M12-02 keeps validation in Python before Unity runtime loading starts. Pydantic
is optional in the current workspace; when it is unavailable the validator uses
the same explicit built-in checks so CI can still run without extra packages.
"""

from __future__ import annotations

import argparse
import json
import sys
from dataclasses import dataclass, field
from pathlib import Path
from typing import Any


try:  # pragma: no cover - current workspace may not install pydantic.
    import pydantic as _pydantic  # type: ignore

    PYDANTIC_AVAILABLE = True
    PYDANTIC_VERSION = getattr(_pydantic, "__version__", "unknown")
except Exception:  # pragma: no cover - exercised when dependency is absent.
    PYDANTIC_AVAILABLE = False
    PYDANTIC_VERSION = ""


SCHEMA_VERSION = "ability_schema_v1"
ABILITY_KINDS = {"auto", "act", "cont", "manual"}
TRIGGER_TYPES = {"manual", "on_timing", "on_event", "continuous"}
PHASES = {"Mulligan", "StandAndDraw", "Ride", "Main", "Battle", "End"}
TIMING_WINDOWS = {
    "None",
    "PhaseTransition",
    "StandAndDrawStep",
    "RideStep",
    "MainStep",
    "BattleStep",
    "AttackDeclaration",
    "GuardStep",
    "DriveCheck",
    "DamageCheck",
    "BattleResolution",
    "CloseStep",
    "EndPhase",
    "OnDraw",
    "OnMoveCard",
    "OnSetPhase",
    "OnAddGiftMarker",
    "OnResourceFlip",
    "PendingAutoResolution",
    "CleanupEndOfBattle",
    "CleanupEndOfTurn",
    "ManualTriggerCheck",
}
COST_TYPES = {
    "none",
    "counter_blast",
    "soul_blast",
    "energy_blast",
    "discard",
    "once_per_turn",
    "once_per_fight",
}
TARGET_TYPES = {"none", "self", "unit", "circle", "card"}
TARGET_OWNERS = {"self", "opponent", "any"}
ZONES = {
    "Deck",
    "Hand",
    "RideDeck",
    "Vanguard",
    "RearGuard",
    "Drop",
    "Damage",
    "Bind",
    "Trigger",
    "Order",
    "Soul",
    "GZone",
}
EFFECT_TYPES = {
    "manual",
    "draw",
    "move_zone",
    "counter_charge",
    "counter_blast",
    "soul_charge",
    "soul_blast",
    "power_plus",
    "critical_plus",
}
DURATION_TYPES = {
    "instant",
    "until_end_of_battle",
    "until_end_of_turn",
    "continuous",
    "manual",
}
CLEANUP_TIMINGS = {"none", "end_of_battle", "end_of_turn", "manual"}

REQUIRED_ABILITY_FIELDS = (
    "ability_id",
    "card_id",
    "kind",
    "trigger",
    "timing",
    "costs",
    "targets",
    "effects",
    "duration",
    "manual_fallback",
)


@dataclass
class AbilityValidationReport:
    source_path: str
    ability_count: int = 0
    validator_backend: str = "pydantic" if PYDANTIC_AVAILABLE else "builtin"
    pydantic_version: str = PYDANTIC_VERSION
    errors: list[str] = field(default_factory=list)
    warnings: list[str] = field(default_factory=list)

    @property
    def all_ok(self) -> bool:
        return not self.errors

    def to_dict(self) -> dict[str, Any]:
        return {
            "source_path": self.source_path,
            "ability_count": self.ability_count,
            "validator_backend": self.validator_backend,
            "pydantic_version": self.pydantic_version,
            "errors": self.errors,
            "warnings": self.warnings,
            "all_ok": self.all_ok,
        }


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Validate ability_schema_v1 JSON data.")
    parser.add_argument("source_path", type=Path)
    parser.add_argument("--json", action="store_true", help="Print machine-readable JSON report.")
    return parser.parse_args()


def validate_ability_file(source_path: Path) -> AbilityValidationReport:
    report = AbilityValidationReport(source_path=str(source_path))
    data = read_json(source_path, report)
    if not isinstance(data, dict):
        return report

    schema_version = data.get("schema_version")
    if schema_version != SCHEMA_VERSION:
        report.errors.append(f"`schema_version` must be `{SCHEMA_VERSION}`.")

    abilities = data.get("abilities")
    if not isinstance(abilities, list):
        report.errors.append("`abilities` must be a list.")
        return report

    report.ability_count = len(abilities)
    seen_ability_ids: set[str] = set()
    for index, ability in enumerate(abilities):
        label = ability_label(index, ability)
        if not isinstance(ability, dict):
            report.errors.append(f"{label} must be an object.")
            continue

        validate_ability(index, ability, seen_ability_ids, report)

    return report


def read_json(path: Path, report: AbilityValidationReport) -> Any:
    try:
        with path.open("r", encoding="utf-8") as handle:
            return json.load(handle)
    except FileNotFoundError:
        report.errors.append(f"Missing ability data file: {path}")
    except json.JSONDecodeError as exc:
        report.errors.append(f"Invalid JSON: {exc}")
    return None


def validate_ability(
    index: int,
    ability: dict[str, Any],
    seen_ability_ids: set[str],
    report: AbilityValidationReport,
) -> None:
    label = ability_label(index, ability)
    for field_name in REQUIRED_ABILITY_FIELDS:
        if field_name not in ability:
            report.errors.append(f"{label} is missing required field `{field_name}`.")

    ability_id = require_text(ability, "ability_id", label, report)
    if ability_id:
        if ability_id in seen_ability_ids:
            report.errors.append(f"{label} duplicate `ability_id`: {ability_id}")
        seen_ability_ids.add(ability_id)

    require_text(ability, "card_id", label, report)
    require_enum(ability, "kind", ABILITY_KINDS, label, report)
    require_bool(ability, "manual_fallback", label, report)
    validate_trigger(ability.get("trigger"), label, report)
    validate_timing(ability.get("timing"), label, report)
    validate_costs(ability.get("costs"), label, report)
    validate_targets(ability.get("targets"), label, report)
    validate_effects(ability.get("effects"), label, report)
    validate_duration(ability.get("duration"), label, report)


def validate_trigger(value: Any, label: str, report: AbilityValidationReport) -> None:
    if not isinstance(value, dict):
        report.errors.append(f"{label} `trigger` must be an object.")
        return

    require_enum(value, "type", TRIGGER_TYPES, f"{label}.trigger", report)
    optional_enum(value, "timing_window", TIMING_WINDOWS, f"{label}.trigger", report)
    optional_enum(value, "source_zone", ZONES, f"{label}.trigger", report)


def validate_timing(value: Any, label: str, report: AbilityValidationReport) -> None:
    if not isinstance(value, dict):
        report.errors.append(f"{label} `timing` must be an object.")
        return

    require_enum(value, "phase", PHASES, f"{label}.timing", report)
    require_enum(value, "window", TIMING_WINDOWS, f"{label}.timing", report)
    if "optional" in value:
        require_bool(value, "optional", f"{label}.timing", report)


def validate_costs(value: Any, label: str, report: AbilityValidationReport) -> None:
    if not isinstance(value, list):
        report.errors.append(f"{label} `costs` must be a list.")
        return

    for index, cost in enumerate(value):
        item_label = f"{label}.costs[{index}]"
        if not isinstance(cost, dict):
            report.errors.append(f"{item_label} must be an object.")
            continue

        cost_type = require_enum(cost, "type", COST_TYPES, item_label, report)
        amount = require_non_negative_int(cost, "amount", item_label, report)
        if cost_type in {"once_per_turn", "once_per_fight"} and not str(cost.get("key", "")).strip():
            report.errors.append(f"{item_label} `{cost_type}` requires `key`.")
        if cost_type == "none" and amount not in {0, None}:
            report.errors.append(f"{item_label} `none` cost must use amount 0.")


def validate_targets(value: Any, label: str, report: AbilityValidationReport) -> None:
    if not isinstance(value, list):
        report.errors.append(f"{label} `targets` must be a list.")
        return

    seen_target_ids: set[str] = set()
    for index, target in enumerate(value):
        item_label = f"{label}.targets[{index}]"
        if not isinstance(target, dict):
            report.errors.append(f"{item_label} must be an object.")
            continue

        target_id = require_text(target, "id", item_label, report)
        if target_id:
            if target_id in seen_target_ids:
                report.errors.append(f"{item_label} duplicate target `id`: {target_id}")
            seen_target_ids.add(target_id)
        require_enum(target, "type", TARGET_TYPES, item_label, report)
        require_enum(target, "owner", TARGET_OWNERS, item_label, report)
        require_enum(target, "zone", ZONES, item_label, report)
        require_non_negative_int(target, "count", item_label, report)
        if "optional" in target:
            require_bool(target, "optional", item_label, report)


def validate_effects(value: Any, label: str, report: AbilityValidationReport) -> None:
    if not isinstance(value, list):
        report.errors.append(f"{label} `effects` must be a list.")
        return

    if not value:
        report.errors.append(f"{label} `effects` must contain at least one effect.")
        return

    for index, effect in enumerate(value):
        item_label = f"{label}.effects[{index}]"
        if not isinstance(effect, dict):
            report.errors.append(f"{item_label} must be an object.")
            continue

        require_enum(effect, "type", EFFECT_TYPES, item_label, report)
        if "amount" in effect and not isinstance(effect.get("amount"), int):
            report.errors.append(f"{item_label} `amount` must be an integer when present.")
        optional_enum(effect, "from_zone", ZONES, item_label, report)
        optional_enum(effect, "to_zone", ZONES, item_label, report)


def validate_duration(value: Any, label: str, report: AbilityValidationReport) -> None:
    if not isinstance(value, dict):
        report.errors.append(f"{label} `duration` must be an object.")
        return

    require_enum(value, "type", DURATION_TYPES, f"{label}.duration", report)
    optional_enum(value, "cleanup_timing", CLEANUP_TIMINGS, f"{label}.duration", report)


def require_text(data: dict[str, Any], key: str, label: str, report: AbilityValidationReport) -> str:
    value = data.get(key)
    if not isinstance(value, str) or not value.strip():
        report.errors.append(f"{label} `{key}` must be a non-empty string.")
        return ""
    return value.strip()


def require_bool(data: dict[str, Any], key: str, label: str, report: AbilityValidationReport) -> None:
    if not isinstance(data.get(key), bool):
        report.errors.append(f"{label} `{key}` must be a boolean.")


def require_enum(
    data: dict[str, Any],
    key: str,
    allowed: set[str],
    label: str,
    report: AbilityValidationReport,
) -> str:
    value = data.get(key)
    if not isinstance(value, str) or value not in allowed:
        report.errors.append(f"{label} `{key}` must be one of: {', '.join(sorted(allowed))}.")
        return ""
    return value


def optional_enum(
    data: dict[str, Any],
    key: str,
    allowed: set[str],
    label: str,
    report: AbilityValidationReport,
) -> None:
    if key in data and data.get(key) is not None:
        require_enum(data, key, allowed, label, report)


def require_non_negative_int(
    data: dict[str, Any],
    key: str,
    label: str,
    report: AbilityValidationReport,
) -> int | None:
    value = data.get(key)
    if not isinstance(value, int) or value < 0:
        report.errors.append(f"{label} `{key}` must be a non-negative integer.")
        return None
    return value


def ability_label(index: int, ability: Any) -> str:
    if isinstance(ability, dict) and str(ability.get("ability_id", "")).strip():
        return f"Ability {index} ({ability['ability_id']})"
    return f"Ability {index}"


def main() -> int:
    args = parse_args()
    report = validate_ability_file(args.source_path)
    if args.json:
        print(json.dumps(report.to_dict(), ensure_ascii=False, indent=2))
    else:
        print(f"Ability data: {report.source_path}")
        print(f"Abilities: {report.ability_count}")
        print(f"Validator backend: {report.validator_backend}")
        if report.pydantic_version:
            print(f"Pydantic: {report.pydantic_version}")
        print(f"Errors: {len(report.errors)}")
        for error in report.errors:
            print(f"  ERROR: {error}")
        print(f"Warnings: {len(report.warnings)}")
        for warning in report.warnings:
            print(f"  WARNING: {warning}")
        print(f"All OK: {report.all_ok}")
    return 0 if report.all_ok else 1


if __name__ == "__main__":
    sys.exit(main())
