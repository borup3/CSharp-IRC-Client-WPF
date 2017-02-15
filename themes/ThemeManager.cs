using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using CodeCafeIRC.themes.Theme;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;

namespace CodeCafeIRC.themes
{
    internal static class ThemeManager
    {
        /// <summary>
        /// Sets the active color scheme.
        /// </summary>
        /// <param name="colorScheme"></param>
        public static void SetColorScheme(ColorScheme colorScheme)
        {
            ResourceDictionary dictionary = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/themes/Theme/ColorScheme.xaml", UriKind.RelativeOrAbsolute)
            };

            dictionary["ControlSelected"] = new SolidColorBrush(colorScheme.ControlSelected);
            dictionary["ControlActive"] = new SolidColorBrush(colorScheme.ControlActive);
            dictionary["ControlNormalLighter"] = new SolidColorBrush(colorScheme.ControlNormalLighter);
            dictionary["ControlNormalLight"] = new SolidColorBrush(colorScheme.ControlNormalLight);
            dictionary["ControlNormal"] = new SolidColorBrush(colorScheme.ControlNormal);
            dictionary["ControlNormalDark"] = new SolidColorBrush(colorScheme.ControlNormalDark);
            dictionary["ControlNormalDarker"] = new SolidColorBrush(colorScheme.ControlNormalDarker);
            dictionary["ControlDisabled"] = new SolidColorBrush(colorScheme.ControlDisabled);
            dictionary["ControlBorder"] = new SolidColorBrush(colorScheme.ControlBorder);
            dictionary["ControlWindowActiveBorder"] = new SolidColorBrush(colorScheme.ControlWindowActiveBorder);
            dictionary["ControlWindowFrame"] = new SolidColorBrush(colorScheme.ControlWindowFrame);

            dictionary["ControlFontNormal"] = new SolidColorBrush(colorScheme.ControlFontNormal);
            dictionary["ControlFontActive"] = new SolidColorBrush(colorScheme.ControlFontActive);
            dictionary["ControlFontHighlight"] = new SolidColorBrush(colorScheme.ControlFontHighlight);
            dictionary["ControlFontSelected"] = new SolidColorBrush(colorScheme.ControlFontSelected);
            dictionary["ControlFontSelectedLight"] = new SolidColorBrush(colorScheme.ControlFontSelectedLight);
            dictionary["ControlFontDisabled"] = new SolidColorBrush(colorScheme.ControlFontDisabled);
            dictionary["ControlFontNormalDark"] = new SolidColorBrush(colorScheme.ControlFontNormalDark);
            dictionary["ControlFontWindowFrame"] = new SolidColorBrush(colorScheme.ControlFontWindowFrame);

            dictionary["LiveChat Private Message"] = new SolidColorBrush(Color.FromRgb(127, 195, 225));
            dictionary["LiveChat You"] = new SolidColorBrush(Color.FromRgb(127, 195, 225));
            dictionary["LiveChat Timestamp"] = new SolidColorBrush(Color.FromRgb(125, 125, 128));
            dictionary["LiveChat Dump Message"] = new SolidColorBrush(Color.FromRgb(125, 125, 128));
            dictionary["LiveChat Own Message"] = new SolidColorBrush(Color.FromRgb(145, 145, 148));
            dictionary["LiveChat Hyperlink"] = new SolidColorBrush(Color.FromRgb(127, 195, 225));
            dictionary["LiveChat Channel Message"] = new SolidColorBrush(Color.FromRgb(127, 195, 225));
            dictionary["LiveChat System Message"] = new SolidColorBrush(Color.FromRgb(210, 210, 210));
            dictionary["LiveChat Error Message"] = new SolidColorBrush(Color.FromRgb(210, 55, 55));
            dictionary["LiveChat Mentioned You"] = new SolidColorBrush(Color.FromRgb(55, 165, 55));

            Application.Current.Resources.MergedDictionaries.RemoveAt(0);
            Application.Current.Resources.MergedDictionaries.Insert(0, dictionary);
        }

        public static void AddUserColor(string name)
        {
            if (Application.Current.Resources.MergedDictionaries[0].Contains(name)) return; // has color
            KnownColor[] goodColors = { KnownColor.Red, KnownColor.IndianRed, KnownColor.AntiqueWhite, KnownColor.LightGreen, KnownColor.Beige, KnownColor.BlanchedAlmond, KnownColor.Violet, KnownColor.Brown, KnownColor.CadetBlue, KnownColor.Chocolate, KnownColor.Coral, KnownColor.CornflowerBlue, KnownColor.Crimson, KnownColor.DarkGoldenrod, KnownColor.DarkSeaGreen, KnownColor.DodgerBlue, KnownColor.Firebrick, KnownColor.ForestGreen, KnownColor.Gold, KnownColor.Goldenrod, KnownColor.GreenYellow, KnownColor.HotPink, KnownColor.IndianRed, KnownColor.Khaki,KnownColor.LightBlue, KnownColor.Aquamarine, KnownColor.LightPink, KnownColor.OliveDrab, KnownColor.Orchid,KnownColor.PaleVioletRed, KnownColor.PeachPuff };
            uint index = 1;
            foreach (char ch in name) index *= ch;
            System.Drawing.Color c = System.Drawing.Color.FromKnownColor(goodColors[index % goodColors.Length]);
            Application.Current.Resources.MergedDictionaries[0][name] = new SolidColorBrush(Color.FromRgb(c.R, c.G, c.B));
        }
    }
}