namespace _TheGame._Scripts.Constants
{
    public static class StringConstants
    {
        //SAVE------------------
        public const string MUSIC_STATUS_KEY = "MusicStatusKey";
        public const string VIBRATION_STATUS_KEY = "VibrationStatusKey";
        public const string LEVEL_KEY = "LevelKey";
        public const string STREAK_COUNT_KEY = "StreakCountKey";
        public const string PREVIOUS_LEVEL_KEY = "PreviousLevelKey";
        public const string SaveFileName = "gamesave.dat";
        public const string MOVE_BACK_COUNT_KEY = "MoveBackCountKey";
        public const string KEY_COUNT_KEY = "KeyCountKey";

        //AD--------------------
        public const string AdjustAppToken = "hzlz34rsov7k";
#if UNITY_ANDROID
        public const string BannerAdUnitId = "c6bdaef9c43a7e6e";
        public const string InterAdUnitId = "c73741a69a4ca830";
        public const string RewardedAdUnitId = "252ba7e5fccb4e54";
#endif
#if PLATFORM_IOS      
        public const string BannerAdUnitId = "0a94b6d7b36c4d3d";
        public const string InterAdUnitId = "1d5c609e58cb0fb9";
        public const string RewardedAdUnitId = "67069970d336a9d6";
#endif
       
        public const string MaxSdkKey = "ma9TV33_iQtMqnJt8_uQzl3HZPZCe1ofJK6pbNsE9frpX1GJ0e2XLFmo3x9NTdV6APWtSMzXECTTdKkcWJcNhu";
        //----------------------
    }
}