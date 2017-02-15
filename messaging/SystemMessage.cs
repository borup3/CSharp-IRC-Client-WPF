namespace CodeCafeIRC.messaging
{
    public sealed class SystemMessage : Message
    {
        public SystemMessage(string content)
            : base(content)
        {
            ResetFormatting();
        }

        public override void ResetFormatting()
        {
            TimeStampFormat = FormattingRule.TimeStampRule;

            SenderFormat = new FormattingRule();
            SenderFormat.Foreground = "LiveChat System Message";

            ContentFormat = new FormattingRule();
            ContentFormat.Foreground = "LiveChat System Message";
        }
    }
}
