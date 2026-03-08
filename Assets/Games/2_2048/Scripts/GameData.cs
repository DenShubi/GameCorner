using UnityEngine;

namespace Game2048
{
    [System.Serializable]
    public class GameData
    {
        public int score;
        public int hiscore;
        public int[] boardState; // 16 tile values (0 = empty)

        public GameData()
        {
            score = 0;
            hiscore = 0;
            boardState = new int[16];
        }

        public GameData(int score, int hiscore, int[] boardState)
        {
            this.score = score;
            this.hiscore = hiscore;
            this.boardState = boardState;
        }
    }

    public static class GameDataManager
    {
        private const string SAVE_KEY = "2048_GameData";

        public static void SaveGame(int score, int hiscore, int[] boardState)
        {
            GameData data = new GameData(score, hiscore, boardState);
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        public static GameData LoadGame()
        {
            if (!PlayerPrefs.HasKey(SAVE_KEY)) {
                return null;
            }

            string json = PlayerPrefs.GetString(SAVE_KEY);
            GameData data = JsonUtility.FromJson<GameData>(json);
            return data;
        }

        public static void DeleteGame()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
        }

        public static bool HasSavedGame()
        {
            return PlayerPrefs.HasKey(SAVE_KEY);
        }
    }
}
