using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace CodeCafeIRC.irc
{
    public class UserCredentials : INotifyPropertyChanged
    {
        private string _username;
        public string ChosenName
        {
            get { return _username; }
            set
            {
                string sValue = value;
                if (sValue != null)
                {
                    sValue = StripUnicode(value);
                    if (sValue == _username)
                        return;
                }

                _username = sValue;
                OnPropertyChanged();
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                string sValue = StripUnicode(value);
                if (sValue == _password)
                    return;

                _password = sValue;
                OnPropertyChanged();
            }
        }

        private string _realName;

        public string RealName
        {
            get { return _realName; }
            private set
            {
                string sValue = StripUnicode(value);
                if (value == _realName)
                    return;

                _realName = sValue;
                OnPropertyChanged();
            }
        }

        public UserCredentials(string nickname, string password)
        {
            // NOTE The RFC says no unicode for these values
            ChosenName = nickname;
            Password = password;
            RealName = nickname;
        }

        public UserCredentials(string nickname, string password, string realname)
            : this(nickname, password)
        {
            RealName = realname;
        }

        public UserCredentials(UserCredentials credentials)
            : this(credentials.ChosenName, credentials.Password, credentials.RealName)
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if(PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string StripUnicode(string value)
        {
            return Regex.Replace(value, @"[^\u0000-\u007F]", string.Empty);
        }
    }
}
