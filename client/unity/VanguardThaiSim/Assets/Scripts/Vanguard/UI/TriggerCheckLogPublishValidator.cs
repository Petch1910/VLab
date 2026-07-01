namespace VanguardThaiSim.UI
{
    public sealed class TriggerCheckLogPublishValidation
    {
        public bool accepted;
        public string rejection_reason;
    }

    public static class TriggerCheckLogPublishValidator
    {
        public const string OnlineOnlyMessage = "Trigger log publish is only available online.";

        public static TriggerCheckLogPublishValidation Validate(bool isOnline)
        {
            if (!isOnline)
            {
                return Reject(OnlineOnlyMessage);
            }

            return new TriggerCheckLogPublishValidation
            {
                accepted = true,
                rejection_reason = string.Empty
            };
        }

        private static TriggerCheckLogPublishValidation Reject(string rejectionReason)
        {
            return new TriggerCheckLogPublishValidation
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty
            };
        }
    }
}
