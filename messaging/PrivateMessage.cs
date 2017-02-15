namespace CodeCafeIRC.messaging
{
    public sealed class PrivateMessage : Message
    {
        public PrivateMessage(string sender, string recipient, string message)
            : base(sender, message)
        {
            Sender = "[Private] " + Sender + ">" + recipient + ":";
            ResetFormatting();
        }

        public override void ResetFormatting()
        {
            TimeStampFormat = FormattingRule.TimeStampRule;

            SenderFormat = new FormattingRule();
            SenderFormat.Foreground = "LiveChat Private Message";
            
            ContentFormat = new FormattingRule();
        }
    }
}
