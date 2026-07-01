using System;

namespace VanguardThaiSim.Decks
{
    public enum DeckValidationSeverity
    {
        Warning,
        Error
    }

    public readonly struct DeckValidationIssue
    {
        public readonly DeckValidationSeverity Severity;
        public readonly string Code;
        public readonly string Message;
        public readonly string CardId;
        public readonly DeckZone? Zone;

        public DeckValidationIssue(
            DeckValidationSeverity severity,
            string code,
            string message,
            string cardId = null,
            DeckZone? zone = null)
        {
            Severity = severity;
            Code = code;
            Message = message;
            CardId = cardId;
            Zone = zone;
        }
    }
}
