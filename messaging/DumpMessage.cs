namespace CodeCafeIRC.messaging
{
    public sealed class DumpMessage : Message
    {
        public DumpMessage(string content)
            : base(content)
        {
            ResetFormatting();
        }

        public override void ResetFormatting()
        {
            TimeStampFormat = FormattingRule.TimeStampRule;

            SenderFormat = new FormattingRule();
            SenderFormat.Foreground = "LiveChat Dump Message";

            ContentFormat = new FormattingRule();
            ContentFormat.Foreground = "LiveChat Dump Message";
        }
    }
}
