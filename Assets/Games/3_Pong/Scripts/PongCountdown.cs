using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Countdown 3-2-1-GO! sebelum bola diluncurkan.
/// Pasang script ini pada GameManager atau GameObject terpisah.
/// </summary>
public class PongCountdown : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text untuk menampilkan angka countdown")]
    public TextMeshProUGUI countdownText;

    [Tooltip("Durasi tiap angka ditampilkan (detik)")]
    public float countdownInterval = 1f;

    [Tooltip("Durasi teks GO! ditampilkan sebelum menghilang")]
    public float goDuration = 0.6f;

    [Header("Animation")]
    [Tooltip("Ukuran font saat angka muncul")]
    public float punchScale = 1.4f;

    [Tooltip("Kecepatan animasi scale")]
    public float animSpeed = 8f;

    [Header("References")]
    public PongBall ball;

    void Start()
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        // Pastikan bola tidak bergerak dulu
        if (ball != null)
            ball.enabled = false;

        StartCoroutine(RunCountdown());
    }

    IEnumerator RunCountdown()
    {
        // Tunggu 1 frame agar scene siap
        yield return null;

        countdownText.gameObject.SetActive(true);

        // 3 → 2 → 1
        string[] steps = { "3", "2", "1", "GO!" };

        for (int i = 0; i < steps.Length; i++)
        {
            countdownText.text = steps[i];

            // Punch scale animation
            yield return StartCoroutine(PunchScaleAnim());

            // Tunggu interval (kecuali GO! lebih singkat)
            float wait = (i < steps.Length - 1) ? countdownInterval : goDuration;
            yield return new WaitForSeconds(wait);
        }

        // Sembunyikan teks
        countdownText.gameObject.SetActive(false);

        // Aktifkan dan luncurkan bola
        if (ball != null)
        {
            ball.enabled = true;
            ball.LaunchBall();
        }
    }

    IEnumerator PunchScaleAnim()
    {
        // Scale dari besar → normal
        float elapsed = 0f;
        float duration = countdownInterval * 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Ease out: mulai besar, mengecil ke 1
            float scale = Mathf.Lerp(punchScale, 1f, t);
            countdownText.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        countdownText.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// Panggil ini untuk countdown ulang setelah gol.
    /// Biasanya dipanggil dari PongGameManager.
    /// </summary>
    public void RestartCountdown()
    {
        StopAllCoroutines();

        if (ball != null)
            ball.enabled = false;

        StartCoroutine(RunCountdown());
    }
}