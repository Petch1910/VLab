using NUnit.Framework;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class OnlineRoomTransportPolicyTests
    {
        [Test]
        public void CurrentPolicyKeepsPhotonTrustedClientRoom()
        {
            OnlineRoomTransportPolicy policy = OnlineRoomTransportPolicy.Current();

            Assert.AreEqual("Photon Realtime", policy.selected_transport);
            Assert.AreEqual("trusted-client friend room", policy.trust_mode);
            Assert.IsFalse(policy.ranked_secure);
            Assert.IsFalse(policy.custom_server_enabled);
            Assert.IsTrue(policy.requires_adr_for_transport_switch);
        }

        [Test]
        public void PolicyFormatterShowsTransportTrustAndAdrGate()
        {
            string formatted = OnlineRoomTransportPolicyFormatter.Format();

            StringAssert.Contains("Photon Realtime", formatted);
            StringAssert.Contains("trusted-client friend room", formatted);
            StringAssert.Contains("no ranked security", formatted);
            StringAssert.Contains("custom server paused", formatted);
            StringAssert.Contains("ADR required", formatted);
        }

        [Test]
        public void LobbyConnectionStatusIncludesPolicyLine()
        {
            string formatted = MultiplayerLobbyStatusFormatter.FormatConnectionStatus(
                "Photon Realtime",
                MultiplayerTransportStatus.Disconnected,
                false,
                "PHOTON_APP_ID_MISSING");

            StringAssert.Contains("Policy: Photon Realtime", formatted);
            StringAssert.Contains("trusted-client friend room", formatted);
            StringAssert.Contains("no ranked security", formatted);
        }
    }
}

