namespace CodeCafeIRC.messaging
{
    public sealed class ErrorMessage : Message
    {
        public ErrorMessage(string error)
            : base(error)
        {
            Sender = "Error:";
            ResetFormatting();
        }

        public override void ResetFormatting()
        {
            TimeStampFormat = FormattingRule.TimeStampRule;
            SenderFormat = new FormattingRule();
            SenderFormat.Foreground = "LiveChat Error Message";
            ContentFormat = new FormattingRule();
        }
    }
}
