# AI Workflow

## Standard Flow

1. Read context:
   - `AGENTS.md`
   - `docs/AI_CONTEXT_BRIEF.md`
   - `docs/VANGUARD_AI_ENGINE_KNOWLEDGE_SUMMARY.md`
   - `docs/CORE_DEVELOPMENT_GUARDRAILS.md` for core/bot/simulation work
   - related subsystem spec

2. Confirm scope:
   - which subsystem is being changed
   - expected input/output
   - acceptance criteria
   - verification command

3. Plan small slices:
   - each slice should build and test independently
   - avoid big-bang refactors
   - keep UI, data, rules, bot, and offline tooling boundaries clear

4. Implement:
   - follow existing code patterns
   - keep state mutation behind RulesCore/GameAction services
   - keep bot decisions behind legal action APIs
   - keep live rules separate from offline analysis/search

5. Verify:
   - run data validation when data changes
   - run Python tests when tooling changes
   - run Unity compile/EditMode tests when Unity C# changes
   - add or update fixtures for new mechanics

6. Update docs:
   - update subsystem spec when behavior changes
   - update `IMPLEMENTATION_PLAN.md` status when a task is completed
   - update `VANGUARD_AI_ENGINE_KNOWLEDGE_SUMMARY.md` only for architecture-level changes
   - add ADR for structural stack decisions

7. Handoff:
   - use `AI_TASK_HANDOFF_TEMPLATE.md`

## Stop Conditions

Stop and report when:

- credentials, tokens, or private accounts are required
- a step risks deleting or overwriting important data
- a major stack change is needed
- specs conflict
- the change would violate `DO_NOT_DO.md`
- proprietary game execution/decompilation/extraction would be required
