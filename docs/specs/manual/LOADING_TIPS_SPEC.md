# Loading Tips Spec

Milestone: `M23-03`

## Purpose

Add short player-facing loading tips to the Windows program flows that can feel
unclear while data or assets are being refreshed.

## Scope

M23-03 covers tips for:

- Startup data reload from Home.
- Card image fallback/cache behavior in Card Browser details.
- Deck load operations from Deck Tools.

## Rules

- Tips must be original text for this project.
- Tips must not expose private ids, raw protocol payloads, local absolute
  paths, or hidden state.
- Tips must be short enough to fit existing status panels.
- Tips must not change card data, deck legality, image cache behavior, or
  RulesCore state.
- Tips are UI text only; async loading rewrites are out of scope for this
  milestone.

## Runtime Contract

Use a central catalog so Home, Card Browser, and Deck Tools do not drift:

```text
LoadingTipCatalog
- DataReload
- CardImages
- DeckLoad
- Get(context)
- Format(context)
- AppendTip(message, context)
```

## Acceptance

- Home reload status includes the data reload tip.
- Card image fallback detail status can include the card image tip.
- Deck load success/failure status can include the deck load tip.
- Existing formatter APIs remain compatible unless explicitly extended.
- EditMode tests cover catalog text and all three UI integration points.
