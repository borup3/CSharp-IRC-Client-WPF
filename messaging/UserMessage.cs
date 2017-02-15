namespace CodeCafeIRC.messaging
{
    public sealed class UserMessage : Message
    {
        private string m_username;
        public UserMessage(string username, string sender, string content)
            : base(sender, content)
        {
            m_username = username;
            Sender += ":";
            ResetFormatting();
        }

        public override void ResetFormatting()
        {
            TimeStampFormat = FormattingRule.TimeStampRule;

            SenderFormat = new FormattingRule();
            SenderFormat.Foreground = m_username;

            ContentFormat = new FormattingRule();
        }
    }
}
