using System;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class MultiplayerTransportResult
    {
        public bool ok;
        public string error_code;
        public string message;

        public static MultiplayerTransportResult Ok()
        {
            return new MultiplayerTransportResult
            {
                ok = true
            };
        }

        public static MultiplayerTransportResult Fail(string errorCode, string message)
        {
            return new MultiplayerTransportResult
            {
                ok = false,
                error_code = string.IsNullOrWhiteSpace(errorCode) ? "TRANSPORT_ERROR" : errorCode,
                message = message ?? ""
            };
        }
    }
}
