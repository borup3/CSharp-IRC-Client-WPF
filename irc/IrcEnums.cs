namespace CodeCafeIRC.irc
{
    /// <summary>
    /// http://www.faqs.org/rfcs/rfc2812.html
    /// </summary>
    enum IrcUserFlags : byte
    {
        NORMAL = 0,
        WALLOPS = 1 << 1,
        INVISIBLE = 1 << 2
    }
    
    public enum IrcClientState : byte
    {
        PENDING_SERVER_CONNECTION,
        CONNECTED_TO_SERVER,
        PENDING_CHANNEL_CONNECTION,
        CONNECTED_TO_CHANNEL
    }

    /// <summary>
    /// http://www.faqs.org/rfcs/rfc2812.html
    /// </summary>
    public enum IrcReplyCode
    {
        RPL_FORWARDINGTOCHANNEL = 470,

        RPL_HELP = 705,

        /// <summary>
        /// Welcome to the network.
        /// </summary>
        RPL_WELCOME = 1,

        /// <summary>
        /// Host information.
        /// </summary>
        RPL_YOURHOST = 2,

        /// <summary>
        /// Server creation date.
        /// </summary>
        RPL_CREATED = 3,

        /// <summary>
        /// Server information.
        /// </summary>
        RPL_MYINFO = 4,

        /// <summary>
        /// Server suggests an alternative server to the user.
        /// </summary>
        RPL_BOUNCE = 5,

        /// <summary>
        /// Reply format used by USERHOST.
        /// nickname [ "*" ] "=" ( "+" / "-" ) hostname
        /// The '*' indicates whether the client has registered as an Operator. The '-' or '+' characters represent whether the client has set an AWAY message or not
        /// respectively.
        /// </summary>
        RPL_USERHOST = 302,

        /// <summary>
        /// ISON reply format.
        /// </summary>
        RPL_ISON = 303,

        /// <summary>
        /// User is away notification.
        /// "nick :away message"
        /// </summary>
        RPL_AWAY = 301,

        /// <summary>
        /// You are no longer marked as away.
        /// </summary>
        RPL_UNAWAY = 305,

        /// <summary>
        /// You are now marked as away.
        /// </summary>
        RPL_NOWAWAY = 306,

        /// <summary>
        /// Who is user.
        /// "nick user host * :real name"
        /// </summary>
        RPL_WHOISUSER = 311,

        /// <summary>
        /// Who is server.
        /// "nick server :server info"
        /// </summary>
        RPL_WHOISSERVER = 312,

        /// <summary>
        /// Who is operator.
        /// "nick :is an IRC operator"
        /// </summary>
        RPL_WHOISOPERATOR = 313,

        /// <summary>
        /// Who is idle.
        /// "nick integer :seconds idle"
        /// </summary>
        RPL_WHOISIDLE = 317,

        /// <summary>
        /// End of WHOIS list.
        /// "nick :End of WHOIS list"
        /// </summary>
        RPL_ENDOFWHOIS = 318,

        /// <summary>
        /// The RPL_ENDOFWHOIS reply is used to mark the end of processing a WHOIS message.
        /// "nick :*( ( "@" / "+" ) channel " " )"
        /// </summary>
        RPL_WHOISCHANNELS = 319,

        /// <summary>
        /// "nick user host * :real name"
        /// </summary>
        RPL_WHOWASUSER = 314,

        /// <summary>
        /// "nick :End of WHOWAS"
        /// </summary>
        RPL_ENDOFWHOWAS = 369,

        /// <summary>
        /// "channel #visible :topic"
        /// </summary>
        RPL_LIST = 322,

        /// <summary>
        /// ":End of LIST"
        /// </summary>
        RPL_LISTEND = 323,

        /// <summary>
        /// "channel nickname"
        /// </summary>
        RPL_UNIQOPIS = 325,

        /// <summary>
        /// "channel mode modeparams"
        /// </summary>
        RPL_CHANNELMODEIS = 324,

        RPL_NOTOPIC = 331,

        RPL_TOPIC = 332,

        /// <summary>
        /// Returned by the server to indicate that the attempted INVITE message was successful and is being passed onto the end client.
        /// </summary>
        RPL_INVITING = 341,

        /// <summary>
        /// Summing user to IRC.
        /// </summary>
        RPL_SUMMONING = 342,

        /// <summary>
        /// "channel invitemask"
        /// </summary>
        RPL_INVITELIST = 346,

        RPL_ENDOFINVITELIST = 347,

        /// <summary>
        /// "channel exceptionmask"
        /// </summary>
        RPL_EXCEPTLIST = 348,

        RPL_ENDOFEXCEPTLIST = 349,

        /// <summary>
        /// "version.debuglevel server :comments"
        /// </summary>
        RPL_VERSION = 351,

        /// <summary>
        /// "channel user host server nick ( "H" / "G" > ["*"] [ ( "@" / "+" ) ] :hopcount realname"
        /// </summary>
        RPL_WHOREPLY = 352,

        RPL_ENDOFWHO = 315,

        /// <summary>
        /// "( "=" / "*" / "@" ) channel :[ "@" / "+" ] nick *( " " [ "@" / "+" ] nick )
        /// - "@" is used for secret channels, "*" for private
        ///   channels, and "=" for others (public channels).
        /// </summary>
        RPL_NAMREPLY = 353,

        RPL_ENDOFNAMES = 366,

        RPL_LINKS = 364,

        RPL_ENDOFLINKS = 365,

        /// <summary>
        /// "channel banmask"
        /// </summary>
        RPL_BANLIST = 367,

        RPL_ENDOFBANLIST = 368,

        RPL_INFO = 371,

        RPL_ENDOFINFO = 374,

        /// <summary>
        /// MOTD start.
        /// </summary>
        RPL_MOTDSTART = 375,

        /// <summary>
        /// Part of MOTD.
        /// </summary>
        RPL_MOTD = 372,

        /// <summary>
        /// MOTD end.
        /// </summary>
        RPL_ENDOFMOTD = 376,

        /// <summary>
        /// You are now an IRC operator.
        /// </summary>
        RPL_YOUREOPER = 381,

        /// <summary>
        /// "config file :Rehashing"
        /// </summary>
        RPL_REHASHING = 382,

        /// <summary>
        /// You are a service.
        /// </summary>
        RPL_YOURESERVICE = 383,

        /// <summary>
        /// "server :string showing server's local time"
        /// </summary>
        RPL_TIME = 391,

        RPL_USERSSTART = 392,

        RPL_USERS = 393,

        RPL_ENDOFUSERS = 394,

        /// <summary>
        /// No users are logged in.
        /// </summary>
        RPL_NOUSERS = 395,

        RPL_TRACELINK = 200,
        RPL_TRACECONNECTING = 201,
        RPL_TRACEHANDSHAKE = 202,
        RPL_TRACEUNKNOWN = 203,
        RPL_TRACEOPERATOR = 204,
        RPL_TRACEUSER = 205,
        RPL_TRACESERVER = 206,
        RPL_TRACESERVICE = 207,
        RPL_TRACENEWTYPE = 208,
        RPL_TRACECLASS = 209,
        RPL_TRACERECONNECT = 210,
        RPL_TRACELOG = 261,
        RPL_TRACEEND = 262,

        RPL_STATSLINKINFO = 211,
        RPL_STATSCOMMANDS = 212,
        RPL_ENDOFSTATS = 219,

        /// <summary>
        /// ":Server Up %d days %d:%02d:%02d"
        /// </summary>
        RPL_STATSUPTIME = 242,

        RPL_STATSOLINE = 243,

        RPL_UMODEIS = 221,

        RPL_SERVLIST = 234,

        RPL_SERVLISTEND = 235,

        RPL_LUSERCLIENT = 251,

        RPL_LUSEROP = 252,

        RPL_LUSERUNKNOWN = 253,

        RPL_LUSERCHANNELS = 254,

        RPL_LUSERME = 255,

        RPL_ADMINME = 256,

        RPL_ADMINLOC1 = 257,

        RPL_ADMINLOC2 = 258,

        RPL_ADMINEMAIL = 259,
        
        RPL_TRYAGAIN = 263,
    }

    /// <summary>
    /// http://www.faqs.org/rfcs/rfc2812.html
    /// </summary>
    public enum IrcErrorReplyCode
    {
        /// <summary>
        /// The nickname parameter in a command is currently unused.
        /// </summary>
        ERR_NOSUCHNICK = 401,

        /// <summary>
        /// The given server name does not exist.
        /// </summary>
        ERR_NOSUCHSERVER = 402,

        /// <summary>
        /// The given channel does not exist.
        /// </summary>
        ERR_NOSUCHCHANNEL = 403,

        /// <summary>
        /// Cannot send to channel.
        /// </summary>
        ERR_CANNOTSENDTOCHAN = 404,

        /// <summary>
        /// You have joined too many channels.
        /// </summary>
        ERR_TOOMANYCHANNELS = 405,

        /// <summary>
        /// Returned by WHOWAS to indicate there is no history for that nickname.
        /// </summary>
        ERR_WASNOSUCHNICK = 406,

        /// <summary>
        /// Too many private message targets.
        /// </summary>
        ERR_TOOMANYTARGETS = 407,

        /// <summary>
        /// Tried to send SQUERY to unexisting service.
        /// </summary>
        ERR_NOSUCHSERVICE = 408,

        /// <summary>
        /// PING or PONG message missing the originator parameter.
        /// </summary>
        ERR_NOORIGIN = 409,

        ERR_NORECIPIENT = 411,

        ERR_NOTEXTTOSEND = 412,

        ERR_NOTOPLEVEL = 413,

        ERR_WILDTOPLEVEL = 414,

        ERR_BADMASK = 415,

        /* 412 - 415 are returned by PRIVMSG to indicate that
           the message wasn't delivered for some reason.
           ERR_NOTOPLEVEL and ERR_WILDTOPLEVEL are errors that
           are returned when an invalid use of
           "PRIVMSG $<server>" or "PRIVMSG #<host>" is attempted.
         */

        ERR_UNKNOWNCOMMAND = 421,

        /// <summary>
        /// Server could not open MOTD file.
        /// </summary>
        ERR_NOMOTD = 422,

        ERR_NOADMININFO = 423,

        ERR_FILEERROR = 424,

        /// <summary>
        /// Returned when a nickname parameter expected for a command and isn't found.
        /// </summary>
        ERR_NONICKNAMEGIVEN = 431,

        /// <summary>
        /// Erroneous nickname. Returned after receiving a NICK message which contains characters which do not fall in the defined set.
        /// </summary>
        ERR_ERRONEUSNICKNAME = 432,

        /// <summary>
        /// Nickname is occupied.
        /// </summary>
        ERR_NICKNAMEINUSE = 433,

        ERR_NICKCOLLISION = 436,

        /// <summary>
        /// The command was blocked by the server delay mechanism.
        /// </summary>
        ERR_UNAVAILRESOURCE = 437,

        /// <summary>
        /// The target user of the command is not in the channel.
        /// </summary>
        ERR_USERNOTINCHANNEL = 441,

        /// <summary>
        /// You are not in that channel.
        /// </summary>
        ERR_NOTONCHANNEL = 442,

        /// <summary>
        /// The invited user is already in this channel.
        /// </summary>
        ERR_USERONCHANNEL = 443,

        /// <summary>
        /// SUMMON was unable to be carried out since they are not logged in.
        /// </summary>
        ERR_NOLOGIN = 444,

        /// <summary>
        /// SUMMON has been disabled.
        /// </summary>
        ERR_SUMMONDISABLED = 445,

        /// <summary>
        /// USERS has been disabled.
        /// </summary>
        ERR_USERSDISABLED = 446,

        /// <summary>
        /// The client MUST be registered.
        /// </summary>
        ERR_NOTREGISTERED = 451,

        /// <summary>
        /// Need more parameters in command.
        /// </summary>
        ERR_NEEDMOREPARAMS = 461,

        /// <summary>
        /// Server blocks second USER attempt.
        /// </summary>
        ERR_ALREADYREGISTRED = 462,

        ERR_NOPERMFORHOST = 463,

        /// <summary>
        /// Password is incorrect.
        /// </summary>
        ERR_PASSWDMISMATCH = 464,

        /// <summary>
        /// You have been banned from that connection.
        /// </summary>
        ERR_YOUREBANNEDCREEP = 465,

        /// <summary>
        /// Your connection will soon be denied.
        /// </summary>
        ERR_YOUWILLBEBANNED = 466,

        ERR_KEYSET = 467,

        ERR_CHANNELISFULL = 471,

        ERR_UNKNOWNMODE = 472,

        /// <summary>
        /// Invite-only channel.
        /// </summary>
        ERR_INVITEONLYCHAN = 473,

        ERR_BANNEDFROMCHAN = 474,

        ERR_BADCHANNELKEY = 475,

        ERR_BADCHANMASK = 476,

        ERR_NOCHANMODES = 477,

        /// <summary>
        /// Channel list is full.
        /// </summary>
        ERR_BANLISTFULL = 478,

        /// <summary>
        /// You're not an operator user.
        /// </summary>
        ERR_NOPRIVILEGES = 481,

        /// <summary>
        /// You need to be channel operator to do that.
        /// </summary>
        ERR_CHANOPRIVSNEEDED = 482,

        /// <summary>
        /// You can't kill a server!
        /// </summary>
        ERR_CANTKILLSERVER = 483,

        /// <summary>
        /// Your connection is restricted.
        /// </summary>
        ERR_RESTRICTED = 484,

        /// <summary>
        /// You're not the original channel operator.
        /// </summary>
        ERR_UNIQOPPRIVSNEEDED = 485,

        ERR_NOOPERHOST = 491,

        /// <summary>
        /// Unknown MODE flag.
        /// </summary>
        ERR_UMODEUNKNOWNFLAG = 501,

        /// <summary>
        /// Cannot change mode for other users.
        /// </summary>
        ERR_USERSDONTMATCH = 502,
    }
}
