using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game2048
{
    [DefaultExecutionOrder(-1)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

    [SerializeField] private TileBoard board;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI hiscoreText;
    [SerializeField] private Button backButton;
    [SerializeField] private Button restartButton;

    public int score { get; private set; } = 0;

    private void Awake()
    {
        if (Instance != null) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) {
            Instance = null;
        }
    }

    private void Start()
    {
        if (backButton != null) {
            backButton.onClick.AddListener(BackToMenu);
        }

        if (restartButton != null) {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (GameDataManager.HasSavedGame()) {
            LoadGame();
        } else {
            NewGame();
        }
    }

    public void NewGame()
    {
        // reset score
        SetScore(0);
        if (hiscoreText != null) {
            hiscoreText.text = LoadHiscore().ToString();
        }

        // hide game over screen
        if (gameOverText != null) {
            gameOverText.gameObject.SetActive(false);
        }

        // update board state
        board.ClearBoard();
        StartCoroutine(CreateInitialTiles());
        board.enabled = true;
    }

    private IEnumerator CreateInitialTiles()
    {
        yield return null;
        board.CreateTile();
        yield return null;
        board.CreateTile();
    }

    public void GameOver()
    {
        board.enabled = false;
        if (gameOverText != null) {
            gameOverText.gameObject.SetActive(true);
        }
    }

    public void IncreaseScore(int points)
    {
        SetScore(score + points);
    }

    private void SetScore(int score)
    {
        this.score = score;
        if (scoreText != null) {
            scoreText.text = score.ToString();
        }

        SaveHiscore();
    }

    private void SaveHiscore()
    {
        int hiscore = LoadHiscore();

        if (score > hiscore) {
            PlayerPrefs.SetInt("hiscore", score);
        }
    }

    private int LoadHiscore()
    {
        return PlayerPrefs.GetInt("hiscore", 0);
    }

    public void SaveCurrentGame()
    {
        int[] boardState = board.GetBoardState();
        GameDataManager.SaveGame(score, LoadHiscore(), boardState);
    }

    public void LoadGame()
    {
        GameData data = GameDataManager.LoadGame();
        if (data == null) {
            NewGame();
            return;
        }

        SetScore(data.score);
        if (hiscoreText != null) {
            hiscoreText.text = data.hiscore.ToString();
        }

        if (gameOverText != null) {
            gameOverText.gameObject.SetActive(false);
        }

        board.ClearBoard();
        StartCoroutine(RestoreBoardState(data.boardState));
        board.enabled = true;
    }

    private IEnumerator RestoreBoardState(int[] boardState)
    {
        yield return null;
        board.RestoreFromState(boardState);
    }

    public void BackToMenu()
    {
        SaveCurrentGame();
        SceneManager.LoadScene("Menu");
    }

    public void RestartGame()
    {
        GameDataManager.DeleteGame();
        NewGame();
    }

}
}
