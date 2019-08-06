using Great.Models;
using System;
using System.Windows;

namespace Great.Utils
{
    public class SkinResourceDictionary : ResourceDictionary
    {
        private Uri _darkSource;

        public Uri DarkSource
        {
            get { return _darkSource; }
            set
            {
                _darkSource = value;
                base.Source = _darkSource;
            }
        }

        private Uri _lightSource;

        public Uri LightSource
        {
            get { return _lightSource; }
            set
            {
                _lightSource = value;
                base.Source = _lightSource;
            }
        }

        public void UpdateSource()
        {
            Uri val = null;
            if (UserSettings.Themes.Skin == ESkin.Light) val = LightSource;
            if (UserSettings.Themes.Skin == ESkin.Dark) val = DarkSource;
            if (val != null)
                base.Source = val;
        }
    }
}
