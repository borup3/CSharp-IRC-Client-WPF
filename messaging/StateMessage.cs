namespace CodeCafeIRC.messaging
{
    public class StateMessage : Message
    {
        public StateMessage(string message)
            : base(message)
        {
            Sender = string.Empty;
            ResetFormatting();
        }

        public override void ResetFormatting()
        {
            TimeStampFormat = FormattingRule.TimeStampRule;

            SenderFormat = new FormattingRule();
            SenderFormat.Foreground = "LiveChat Channel Message";

            ContentFormat = new FormattingRule();
            ContentFormat.Foreground = "LiveChat Channel Message";
        }
    }
}
