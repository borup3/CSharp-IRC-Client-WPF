namespace CodeCafeIRC.messaging
{
    public sealed class ServerMessage : Message
    {
        public ServerMessage(string content)
            : base(content)
        {
            ResetFormatting();
        }

        public override void ResetFormatting()
        {
            TimeStampFormat = FormattingRule.TimeStampRule;

            SenderFormat = new FormattingRule();

            ContentFormat = new FormattingRule();
        }
    }
}
