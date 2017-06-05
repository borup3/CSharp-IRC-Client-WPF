using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Reflection;

namespace CodeCafeIRC
{
    enum eSound
    {
        Mentioned,
    }

    static class Sounds
    {
        private static IDictionary<eSound, SoundPlayer> soundPlayers = new Dictionary<eSound, SoundPlayer>();

        public static void Play(eSound sound)
        {
            if (soundPlayers == null)
            {
                Debug.Assert(false, "dont");
                return;
            }

            SoundPlayer player;
            if (soundPlayers.TryGetValue(sound, out player))
            {
                player.Play();
                return;
            }

            player = new SoundPlayer();

            if (sound == eSound.Mentioned)
            {
                string path;
                if (General.TryGetOption("notification_wav_path_relative", out path))
                {
                    path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), path.Trim(' ', '/', '\\'));
                    if (File.Exists(path)) player.SoundLocation = path;
                    else return;
                }
                else return;
            }

            player.Load();
            player.Play();
            soundPlayers[sound] = player;
        }

        public static void Free()
        {
            foreach (var pair in soundPlayers)
                pair.Value.Dispose();
            soundPlayers = null;
        }
    }
}
