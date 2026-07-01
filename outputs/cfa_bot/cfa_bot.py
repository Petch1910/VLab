from __future__ import annotations

import argparse
import collections
import dataclasses
import math
import random
import re
import struct
from pathlib import Path


TEXT_ENCODING = "utf-8-sig"


@dataclasses.dataclass(frozen=True)
class Card:
    card_id: int
    name: str
    text: str = ""
    grade: int | None = None
    power: int | None = None
    shield: int | None = None
    source: str = ""

    @property
    def trigger(self) -> str | None:
        haystack = f"{self.name}\n{self.text}".lower()
        for kind in ("over", "heal", "critical", "front", "draw", "stand"):
            if f"{kind} trigger" in haystack:
                return kind
        return None

    @property
    def is_order(self) -> bool:
        return "order" in self.text.lower().split("\n", 1)[0].lower()


def read_text(path: Path) -> str:
    try:
        return path.read_text(encoding=TEXT_ENCODING)
    except UnicodeDecodeError:
        return path.read_text(encoding="cp1252", errors="replace")


def parse_card_file(path: Path) -> dict[int, Card]:
    text = read_text(path)
    starts = list(re.finditer(r"(?m)^CardStat\s*=\s*(\d+)\s*$", text))
    cards: dict[int, Card] = {}

    for idx, match in enumerate(starts):
        card_id = int(match.group(1))
        end = starts[idx + 1].start() if idx + 1 < len(starts) else len(text)
        block = text[match.end() : end]

        strings = dict(re.findall(r"global\.(\w+)\[CardStat\]\s*=\s*'([\s\S]*?)'", block))
        numbers = {}
        for key, value in re.findall(r"global\.(\w+)\[CardStat\]\s*=\s*(-?\d+)", block):
            numbers[key] = int(value)

        name = strings.get("CardName", f"Unknown card {card_id}")
        cards[card_id] = Card(
            card_id=card_id,
            name=name,
            text=strings.get("CardText", ""),
            grade=numbers.get("UnitGrade"),
            power=numbers.get("PowerStat"),
            shield=numbers.get("DefensePowerStat"),
            source=path.name,
        )

    return cards


def load_cards(game_dir: Path) -> dict[int, Card]:
    text_dir = game_dir / "Text"
    if not text_dir.is_dir():
        raise FileNotFoundError(f"Text folder not found: {text_dir}")

    cards: dict[int, Card] = {}
    for path in sorted(text_dir.glob("*.txt")):
        cards.update(parse_card_file(path))
    return cards


def decode_area_real(hex16: str) -> int:
    raw = bytes.fromhex(hex16)
    # Cardfight Area stores the two 32-bit words swapped compared with a normal
    # little-endian double: e.g. 0013C04000000000 -> 8230.0.
    value = struct.unpack("<d", raw[4:8] + raw[0:4])[0]
    if not math.isfinite(value) or value <= 0:
        return 0
    return int(round(value))


def parse_deck(path: Path) -> list[int]:
    payload = path.read_bytes().decode("ascii").strip()
    if len(payload) < 32:
        raise ValueError(f"Deck is too short: {path}")
    if len(payload) % 16 != 0:
        raise ValueError(f"Unexpected deck length: {path} has {len(payload)} hex chars")

    card_ids = []
    for offset in range(32, len(payload), 16):
        chunk = payload[offset : offset + 16]
        if len(chunk) < 16:
            continue
        card_id = decode_area_real(chunk)
        if card_id:
            card_ids.append(card_id)
    return card_ids


def deck_counter(deck_ids: list[int], cards: dict[int, Card]) -> collections.Counter[str]:
    names = [cards.get(card_id, Card(card_id, f"Unknown card {card_id}")).name for card_id in deck_ids]
    return collections.Counter(names)


def print_deck_summary(deck_ids: list[int], cards: dict[int, Card]) -> None:
    grades = collections.Counter()
    triggers = collections.Counter()
    unknown = []

    for card_id in deck_ids:
        card = cards.get(card_id)
        if card is None:
            unknown.append(card_id)
            continue
        grades[card.grade if card.grade is not None else "unknown"] += 1
        if card.trigger:
            triggers[card.trigger] += 1

    print(f"Cards: {len(deck_ids)}")
    print("Grades:", ", ".join(f"{grade}={count}" for grade, count in sorted(grades.items(), key=lambda x: str(x[0]))))
    print("Triggers:", ", ".join(f"{kind}={count}" for kind, count in sorted(triggers.items())) or "none detected")
    if unknown:
        print("Unknown ids:", ", ".join(map(str, unknown)))

    print("\nCard list:")
    for name, count in deck_counter(deck_ids, cards).most_common():
        print(f"{count:>2}x {name}")


def opening_hand(deck_ids: list[int], seed: int | None) -> list[int]:
    rng = random.Random(seed)
    shuffled = list(deck_ids)
    rng.shuffle(shuffled)
    return shuffled[:5]


def advise_mulligan(hand: list[Card]) -> list[str]:
    grades = {card.grade for card in hand}
    advice = []
    for target in (1, 2, 3):
        if target not in grades:
            advice.append(f"หา grade {target} สำหรับ ride line")

    keep = []
    for card in hand:
        if card.grade in (1, 2, 3):
            keep.append(card.name)
        elif card.trigger is None and (card.power or 0) >= 8000:
            keep.append(card.name)

    if keep:
        advice.append("เก็บ: " + ", ".join(dict.fromkeys(keep)))

    bottom = [card.name for card in hand if card.trigger and card.trigger != "over"]
    if bottom:
        advice.append("พิจารณา mulligan trigger ปกติ: " + ", ".join(dict.fromkeys(bottom)))

    if not advice:
        advice.append("มือนี้เล่นได้ตาม curve พื้นฐาน")
    return advice


def advise_turn(hand: list[Card]) -> list[str]:
    by_grade = collections.defaultdict(list)
    attackers = []
    boosters = []
    orders = []

    for card in hand:
        by_grade[card.grade].append(card)
        if card.is_order:
            orders.append(card)
        if card.grade in (2, 3) and (card.power or 0) >= 9000:
            attackers.append(card)
        if card.grade in (0, 1) and (card.power or 0) >= 5000 and not card.trigger:
            boosters.append(card)

    lines = []
    for grade in (1, 2, 3):
        if by_grade[grade]:
            names = ", ".join(card.name for card in by_grade[grade][:3])
            lines.append(f"ride candidate grade {grade}: {names}")

    if attackers:
        lines.append("front-row candidates: " + ", ".join(card.name for card in attackers[:3]))
    if boosters:
        lines.append("back-row candidates: " + ", ".join(card.name for card in boosters[:3]))
    if orders:
        lines.append("อ่าน order ก่อนเล่น: " + ", ".join(card.name for card in orders[:3]))

    lines.append("battle heuristic: เริ่มโจมตีด้วย rear-guard ก่อน แล้วปิดด้วย vanguard ถ้าต้องการ drive check หลังเห็น guard")
    return lines


@dataclasses.dataclass
class BotState:
    cards: dict[int, Card]
    deck: list[int]
    hand: list[int]
    drop: list[int]
    vanguard: int | None = None
    front: list[int | None] = dataclasses.field(default_factory=lambda: [None, None, None])
    back: list[int | None] = dataclasses.field(default_factory=lambda: [None, None, None])

    def card(self, card_id: int | None) -> Card | None:
        if card_id is None:
            return None
        return self.cards.get(card_id, Card(card_id, f"Unknown card {card_id}"))

    def draw(self, count: int = 1) -> list[Card]:
        drawn = []
        for _ in range(count):
            if not self.deck:
                break
            card_id = self.deck.pop(0)
            self.hand.append(card_id)
            drawn.append(self.card(card_id))
        return [card for card in drawn if card is not None]

    def remove_from_hand(self, card_id: int) -> None:
        self.hand.remove(card_id)


def choose_from_hand(state: BotState, predicate) -> int | None:
    candidates = [card_id for card_id in state.hand if predicate(state.card(card_id))]
    if not candidates:
        return None
    candidates.sort(
        key=lambda cid: (
            state.card(cid).trigger is not None,
            -(state.card(cid).power or 0),
            state.card(cid).name,
        )
    )
    return candidates[0]


def bot_turn(state: BotState, turn: int) -> list[str]:
    lines = [f"Turn {turn}"]
    drawn = state.draw(1)
    if drawn:
        lines.append(f"draw: {drawn[0].name}")

    current_grade = state.card(state.vanguard).grade if state.vanguard else -1
    target_grade = min((current_grade or 0) + 1, 3)
    ride_id = choose_from_hand(state, lambda card: card is not None and card.grade == target_grade)
    if ride_id is not None:
        old_vanguard = state.vanguard
        state.remove_from_hand(ride_id)
        if old_vanguard is not None:
            state.drop.append(old_vanguard)
        state.vanguard = ride_id
        lines.append(f"ride: {state.card(ride_id).name}")
    elif state.vanguard is None:
        starter = choose_from_hand(state, lambda card: card is not None and card.grade == 0)
        if starter is not None:
            state.remove_from_hand(starter)
            state.vanguard = starter
            lines.append(f"start vanguard: {state.card(starter).name}")

    max_call_grade = state.card(state.vanguard).grade if state.vanguard else 0

    for idx, slot in enumerate(state.front):
        if slot is not None:
            continue
        call_id = choose_from_hand(
            state,
            lambda card: card is not None
            and card.grade in (1, 2, 3)
            and card.grade <= max_call_grade
            and not card.is_order
            and card.trigger is None,
        )
        if call_id is None:
            break
        state.remove_from_hand(call_id)
        state.front[idx] = call_id
        lines.append(f"call front {idx + 1}: {state.card(call_id).name}")

    for idx, slot in enumerate(state.back):
        if slot is not None:
            continue
        call_id = choose_from_hand(
            state,
            lambda card: card is not None
            and card.grade in (0, 1)
            and card.grade <= max_call_grade
            and not card.is_order
            and card.trigger is None,
        )
        if call_id is None:
            break
        state.remove_from_hand(call_id)
        state.back[idx] = call_id
        lines.append(f"call back {idx + 1}: {state.card(call_id).name}")

    attacks = []
    for idx, front_id in enumerate(state.front):
        front_card = state.card(front_id)
        if front_card is None:
            continue
        back_card = state.card(state.back[idx])
        power = (front_card.power or 0) + (back_card.power or 0 if back_card else 0)
        booster = f" + boost {back_card.name}" if back_card else ""
        attacks.append(f"{front_card.name}{booster} ({power})")

    vg_card = state.card(state.vanguard)
    if vg_card is not None:
        attacks.append(f"vanguard {vg_card.name} ({vg_card.power or 0})")

    if attacks:
        lines.append("attacks:")
        lines.extend(f"- {attack}" for attack in attacks)
    else:
        lines.append("no attacks available")

    hand_names = [state.card(card_id).name for card_id in state.hand if state.card(card_id)]
    lines.append(f"hand left: {len(state.hand)}" + (f" ({', '.join(hand_names[:6])})" if hand_names else ""))
    return lines


def command_play(args: argparse.Namespace) -> None:
    cards = load_cards(Path(args.game))
    deck_ids = parse_deck(Path(args.deck))
    starter_id = None
    for idx, card_id in enumerate(deck_ids):
        card = cards.get(card_id)
        if card is not None and card.grade == 0 and card.trigger is None and not card.is_order:
            starter_id = deck_ids.pop(idx)
            break

    rng = random.Random(args.seed)
    rng.shuffle(deck_ids)
    state = BotState(cards=cards, deck=deck_ids, hand=[], drop=[], vanguard=starter_id)
    state.draw(5)

    if starter_id is not None:
        starter = state.card(starter_id)
        print(f"Starting vanguard: {starter.name if starter else starter_id}")
    print("Opening hand:")
    for card_id in state.hand:
        card = state.card(card_id)
        print(f"- {card.name}" if card else f"- Unknown card {card_id}")
    print()

    for turn in range(1, args.turns + 1):
        for line in bot_turn(state, turn):
            print(line)
        print()


def command_scan(args: argparse.Namespace) -> None:
    cards = load_cards(Path(args.game))
    grades = collections.Counter(card.grade for card in cards.values())
    print(f"Loaded cards: {len(cards)}")
    print("Grade distribution:", ", ".join(f"{grade}={count}" for grade, count in sorted(grades.items(), key=lambda x: str(x[0]))))


def command_deck(args: argparse.Namespace) -> None:
    cards = load_cards(Path(args.game))
    deck_ids = parse_deck(Path(args.deck))
    print_deck_summary(deck_ids, cards)


def command_suggest(args: argparse.Namespace) -> None:
    cards = load_cards(Path(args.game))
    deck_ids = parse_deck(Path(args.deck))
    hand_ids = opening_hand(deck_ids, args.seed)
    hand = [cards.get(card_id, Card(card_id, f"Unknown card {card_id}")) for card_id in hand_ids]

    print("Opening hand:")
    for card in hand:
        bits = []
        if card.grade is not None:
            bits.append(f"G{card.grade}")
        if card.power is not None:
            bits.append(f"P{card.power}")
        if card.trigger:
            bits.append(f"{card.trigger} trigger")
        meta = f" ({', '.join(bits)})" if bits else ""
        print(f"- {card.name}{meta}")

    print("\nMulligan advice:")
    for line in advise_mulligan(hand):
        print(f"- {line}")

    print("\nTurn plan:")
    for line in advise_turn(hand):
        print(f"- {line}")


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="External bot prototype for Cardfight Area")
    sub = parser.add_subparsers(required=True)

    scan = sub.add_parser("scan", help="Load and summarize the card database")
    scan.add_argument("--game", required=True, help="Path to Cardfight Area folder")
    scan.set_defaults(func=command_scan)

    deck = sub.add_parser("deck", help="Decode and summarize a .prfl deck")
    deck.add_argument("--game", required=True, help="Path to Cardfight Area folder")
    deck.add_argument("--deck", required=True, help="Path to .prfl deck")
    deck.set_defaults(func=command_deck)

    suggest = sub.add_parser("suggest", help="Draw an opening hand and print bot advice")
    suggest.add_argument("--game", required=True, help="Path to Cardfight Area folder")
    suggest.add_argument("--deck", required=True, help="Path to .prfl deck")
    suggest.add_argument("--seed", type=int, default=None, help="Deterministic shuffle seed")
    suggest.set_defaults(func=command_suggest)

    play = sub.add_parser("play", help="Run a simple console bot simulation")
    play.add_argument("--game", required=True, help="Path to Cardfight Area folder")
    play.add_argument("--deck", required=True, help="Path to .prfl deck")
    play.add_argument("--seed", type=int, default=None, help="Deterministic shuffle seed")
    play.add_argument("--turns", type=int, default=4, help="Number of bot turns to simulate")
    play.set_defaults(func=command_play)

    return parser


def main() -> None:
    parser = build_parser()
    args = parser.parse_args()
    args.func(args)


if __name__ == "__main__":
    main()
