using UnityEngine;

namespace Main
{
    public static class SettingsPrefs
    {
        public static string Volume = "Volume";
        public static string Sound = "Sound";
        
        public static string Death = "Death";
        public static string Best = "Best";
        
        // Snake Settings
        public static string Powers = "Powers"; // +1
        public static string Seeker = "Seeker"; // +1
        public static string Snake = "Snake"; // Increase +1 {Speed, Start size, rotation speed}
        
        // Planet
        public static string Level1 = "Level1";
        public static string Level2 = "Level2";

        public static int GetPrefs(string prefs) => PlayerPrefs.GetInt(prefs, 0);
        public static int GetUpdatePrefs(string prefs) => PlayerPrefs.GetInt(prefs, 1);

        public static void SavePrefs(string prefs, int val)
        {
            PlayerPrefs.SetInt(prefs, val);
            PlayerPrefs.Save();
        }
    }
}
