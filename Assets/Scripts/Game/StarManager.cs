using UnityEngine;

namespace Game
{
    public static class StarManager
    {
        private const string StarKey = "StarKey";
        private const string AppleKey = "AppleKey";
        // Star
        public static int GetStar() => PlayerPrefs.GetInt(StarKey, 0);
        public static void AddStars(int val)
        {
            var star = PlayerPrefs.GetInt(StarKey, 0);
            PlayerPrefs.SetInt(StarKey, star + val);
            PlayerPrefs.Save();
        }

        public static void RefreshStars(int val)
        {
            PlayerPrefs.SetInt(StarKey, val);
            PlayerPrefs.Save();
        }
        
        // Apple
        public static int GetApple() => PlayerPrefs.GetInt(AppleKey, 0);
        public static void AddApple(int val)
        {
            var apple = PlayerPrefs.GetInt(AppleKey, 0);
            PlayerPrefs.SetInt(AppleKey, apple + val);
            PlayerPrefs.Save();
        }
    }
}
