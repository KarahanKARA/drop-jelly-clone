using System;

namespace _TheGame._Scripts.Helpers
{
    public static class Enums
    {
        public enum NutColorType
        {
            None,
            Blue,
            Purple,
            Grey,
            Yellow,
            Green,
            Orange,
            DarkGreen,
            DarkBlue,
            Red,
            Pink
        }
        
        [Serializable]
        public enum Sound
        {
            Kazanma,
            VidaBitirme,
            VidaCikma,
            VidaGirme,
            VidaOturma,
            VidaUzama,
            ButtonClick,
            Menu,
            Hop
        }

        [Serializable]
        public enum ButtonType
        {
            None,
            Classic,
            Rewarded
        }
    }
}
