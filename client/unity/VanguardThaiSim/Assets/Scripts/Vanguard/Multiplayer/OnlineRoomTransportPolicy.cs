namespace VanguardThaiSim.Multiplayer
{
    public sealed class OnlineRoomTransportPolicy
    {
        public string selected_transport;
        public string trust_mode;
        public bool ranked_secure;
        public bool custom_server_enabled;
        public bool requires_adr_for_transport_switch;

        public static OnlineRoomTransportPolicy Current()
        {
            return new OnlineRoomTransportPolicy
            {
                selected_transport = "Photon Realtime",
                trust_mode = "trusted-client friend room",
                ranked_secure = false,
                custom_server_enabled = false,
                requires_adr_for_transport_switch = true
            };
        }
    }

    public static class OnlineRoomTransportPolicyFormatter
    {
        public static string Format(OnlineRoomTransportPolicy policy = null)
        {
            policy = policy ?? OnlineRoomTransportPolicy.Current();
            string transport = string.IsNullOrWhiteSpace(policy.selected_transport)
                ? "unknown transport"
                : policy.selected_transport.Trim();
            string trustMode = string.IsNullOrWhiteSpace(policy.trust_mode)
                ? "unknown trust mode"
                : policy.trust_mode.Trim();
            string ranked = policy.ranked_secure ? "ranked security enabled" : "no ranked security";
            string server = policy.custom_server_enabled ? "custom server enabled" : "custom server paused";
            string adr = policy.requires_adr_for_transport_switch ? "ADR required for transport switch" : "transport switch not gated";
            return "Policy: " + transport + " / " + trustMode + " / " + ranked + " / " + server + " / " + adr;
        }
    }
}

