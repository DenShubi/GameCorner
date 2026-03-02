using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Mengontrol Pause system.
/// Tombol Pause → tampilkan panel → Time.timeScale = 0.
/// Sound & Music slider + tombol Resume, Home, Restart.
/// </summary>
public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Tombol Pause di top bar")]
    public Button pauseButton;

    [Tooltip("Panel Pause (parent yang di-toggle)")]
    public GameObject pausePanel;

    [Header("Sliders")]
    [Tooltip("Slider untuk Sound/SFX volume")]
    public Slider soundSlider;

    [Tooltip("Text persentase Sound")]
    public TextMeshProUGUI soundPercentText;

    [Tooltip("Slider untuk Music volume")]
    public Slider musicSlider;

    [Tooltip("Text persentase Music")]
    public TextMeshProUGUI musicPercentText;

    [Header("Buttons")]
    [Tooltip("Tombol Resume (kembali ke game / arrow kiri)")]
    public Button resumeButton;

    [Tooltip("Tombol Home (kembali ke menu utama)")]
    public Button homeButton;

    [Tooltip("Tombol Restart")]
    public Button restartButton;

    [Header("Scene Names")]
    [Tooltip("Nama scene menu utama")]
    public string homeSceneName = "MainMenu";

    // PlayerPrefs keys
    private const string SOUND_VOL_KEY = "KnifeHit_SoundVol";
    private const string MUSIC_VOL_KEY = "KnifeHit_MusicVol";

    private bool isPaused = false;

    void Start()
    {
        // Pastikan pause panel hidden saat mulai
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Load saved volumes
        float savedSound = PlayerPrefs.GetFloat(SOUND_VOL_KEY, 1f);
        float savedMusic = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 1f);

        if (soundSlider != null)
        {
            soundSlider.value = savedSound;
            soundSlider.onValueChanged.AddListener(OnSoundChanged);
            UpdateSoundPercentText(savedSound);
        }

        if (musicSlider != null)
        {
            musicSlider.value = savedMusic;
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
            UpdateMusicPercentText(savedMusic);
        }

        // Setup button listeners
        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (homeButton != null)
            homeButton.onClick.AddListener(GoHome);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
    }

    // ======= PAUSE / RESUME =======

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Debug.Log("[Pause] Game paused");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Debug.Log("[Pause] Game resumed");
    }

    // ======= NAVIGATION =======

    public void GoHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(homeSceneName);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ======= SOUND / MUSIC =======

    private void OnSoundChanged(float value)
    {
        PlayerPrefs.SetFloat(SOUND_VOL_KEY, value);
        PlayerPrefs.Save();
        UpdateSoundPercentText(value);

        // Apply volume ke AudioListener atau AudioMixer jika ada
        // Untuk sekarang, simpan saja ke PlayerPrefs
        Debug.Log($"[Sound] Volume: {Mathf.RoundToInt(value * 100)}%");
    }

    private void OnMusicChanged(float value)
    {
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, value);
        PlayerPrefs.Save();
        UpdateMusicPercentText(value);

        Debug.Log($"[Music] Volume: {Mathf.RoundToInt(value * 100)}%");
    }

    private void UpdateSoundPercentText(float value)
    {
        if (soundPercentText != null)
            soundPercentText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    private void UpdateMusicPercentText(float value)
    {
        if (musicPercentText != null)
            musicPercentText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    // ======= GETTER =======
    public bool IsPaused() => isPaused;
}