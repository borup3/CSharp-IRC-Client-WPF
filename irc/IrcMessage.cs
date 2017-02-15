using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CodeCafeIRC.irc
{
    public class IrcMessage
    {
        private const string PREFIX_PATTERN = @"([\w\d\.]+)|([^:\s]+(\s*(![^:]+)?@[^:]+)?)";
        private const string COMMAND_PATTERN = @"([\w]+)|([\d]{3})";
        private const string NOSPCRLFCL = @"[^\r\n\s:]";
        private const string MIDDLE_PATTERN = "(" + NOSPCRLFCL + @"(:|" + NOSPCRLFCL + ")*)";
        private const string TRAILING_PATTERN = @"(:|\s|" + NOSPCRLFCL + ")*";
        private const string PARAMS_PATTERN = @"(\s(?<param>" + MIDDLE_PATTERN + @")){0,14}(\s:(?<param>" + TRAILING_PATTERN + @"))?";
        private const string ENDLINE = @"[\r\n]";
        private const string MESSAGE_NO_PARAMS_PATTERN = @"(:(?<prefix>:" + PREFIX_PATTERN + @"))?\s*(?<command>" + COMMAND_PATTERN + @")";

        private readonly string _original;

        public string Prefix { get; private set; }
        public string Command { get; private set; }
        public IList<string> Parameters { get; private set; }

        public IrcReplyCode REPLY { get; private set; }
        public IrcErrorReplyCode ERR_REPLY { get; private set; }
        public bool IsError { get; private set; }
        public bool IsReply { get; private set; }
        
        public IrcMessage(string message)
        {
            _original = message;
            if (message == null) 
                throw new ArgumentNullException("message");
            
            Match match = Regex.Match(message, MESSAGE_NO_PARAMS_PATTERN);
            if (match == null || !match.Success)
                throw new ArgumentException("message invalid");
            
            Parameters = new List<string>();

            if (match.Groups["prefix"] != null && match.Groups["prefix"].Success)
                Prefix = match.Groups["prefix"].Value;
            else Prefix = string.Empty;

            if (match.Groups["command"] != null && match.Groups["command"].Success)
            {
                Command = match.Groups["command"].Value;
                if (Command.Length < 4)
                    // Might be a reply code
                    ParseReplyCode(Command);
            }
            else Command = string.Empty;

            message = message.Remove(0, match.Groups["command"].Index + match.Groups["command"].Length);
            
            Group group = Regex.Match(message, PARAMS_PATTERN).Groups["param"];
            foreach (Capture capture in group.Captures.Cast<Capture>().Where(c => c.Length > 0 && c.Value != "*"))
                Parameters.Add(capture.Value.Trim());
        }

        private void ParseReplyCode(string msg)
        {
            // It's a reply code!
            IrcReplyCode rpl;
            if (Enum.TryParse(msg, out rpl) && Enum.IsDefined(typeof(IrcReplyCode), rpl))
            {
                // Normal reply code!
                IsError = false;
                IsReply = true;
                REPLY = rpl;
            }

            IrcErrorReplyCode err;
            if (Enum.TryParse(msg, out err) && Enum.IsDefined(typeof(IrcErrorReplyCode), err))
            {
                // Error reply code!
                IsError = true;
                IsReply = false;
                ERR_REPLY = err;
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return _original;
        }
    }
}
