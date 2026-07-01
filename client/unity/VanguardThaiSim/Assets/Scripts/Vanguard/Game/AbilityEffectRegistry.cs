using System.Collections.Generic;

namespace VanguardThaiSim.Game
{
    public delegate AbilityResolutionResult AbilityEffectHandler(
        GameState state,
        int playerIndex,
        AbilityDefinition ability,
        AbilityEffectDefinition effect);

    public static class AbilityEffectRegistry
    {
        private sealed class HandlerEntry
        {
            public AbilityEffectHandler handler;
            public AbilityEffectHandlerPolicy policy;
        }

        private static readonly Dictionary<string, HandlerEntry> Handlers = new Dictionary<string, HandlerEntry>();

        public static void Register(string effectId, AbilityEffectHandler handler)
        {
            Register(
                effectId,
                handler,
                AbilityEffectHandlerPolicy.StructuredCommandOnly("AbilityEffectRegistry.Register"));
        }

        public static void Register(
            string effectId,
            AbilityEffectHandler handler,
            AbilityEffectHandlerPolicy policy)
        {
            if (string.IsNullOrWhiteSpace(effectId))
            {
                throw new System.ArgumentException("Effect id is required.", nameof(effectId));
            }

            if (handler == null)
            {
                throw new System.ArgumentNullException(nameof(handler));
            }

            LiveEffectResolutionPolicyValidationResult policyResult =
                LiveEffectResolutionTextParsingGuard.ValidateCustomEffectPolicy(effectId, policy);
            if (!policyResult.accepted)
            {
                throw new System.InvalidOperationException(policyResult.rejection_reason);
            }

            Handlers[effectId] = new HandlerEntry
            {
                handler = handler,
                policy = LiveEffectResolutionTextParsingGuard.ClonePolicy(policy)
            };
        }

        public static bool TryResolve(
            string effectId,
            GameState state,
            int playerIndex,
            AbilityDefinition ability,
            AbilityEffectDefinition effect,
            out AbilityResolutionResult result)
        {
            if (!string.IsNullOrWhiteSpace(effectId) && Handlers.TryGetValue(effectId, out HandlerEntry entry))
            {
                result = entry.handler(state, playerIndex, ability, effect);
                return true;
            }

            result = null;
            return false;
        }

        public static AbilityEffectHandlerPolicy GetPolicy(string effectId)
        {
            if (!string.IsNullOrWhiteSpace(effectId) && Handlers.TryGetValue(effectId, out HandlerEntry entry))
            {
                return LiveEffectResolutionTextParsingGuard.ClonePolicy(entry.policy);
            }

            return null;
        }

        public static void Unregister(string effectId)
        {
            if (!string.IsNullOrWhiteSpace(effectId))
            {
                Handlers.Remove(effectId);
            }
        }
    }
}
