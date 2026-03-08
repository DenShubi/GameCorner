using System.Collections;
using UnityEngine;
using TMPro;

public class PongCountdown : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI countdownText;

    [Header("Timing")]
    public float countdownInterval = 1f;
    public float goDuration = 0.6f;

    [Header("Animation")]
    public float punchScale = 1.4f;

    [Header("References")]
    public PongBall ball;

    void Start()
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        StartCoroutine(RunCountdown());
    }

    public void RestartCountdown()
    {
        StopAllCoroutines();

        // Reset posisi bola, TIDAK launch
        ball?.ResetPosition();

        StartCoroutine(RunCountdown());
    }

    IEnumerator RunCountdown()
    {
        // Pastikan bola diam dulu
        ball?.StopBall();

        yield return null; // tunggu 1 frame

        countdownText.gameObject.SetActive(true);

        string[] steps = { "3", "2", "1", "GO!" };

        for (int i = 0; i < steps.Length; i++)
        {
            countdownText.text = steps[i];
            yield return StartCoroutine(PunchScaleAnim(
                i < steps.Length - 1 ? countdownInterval : goDuration
            ));
        }

        countdownText.gameObject.SetActive(false);

        // Countdown selesai → baru launch bola
        ball?.LaunchBall();
    }

    IEnumerator PunchScaleAnim(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t     = elapsed / duration;
            float scale = Mathf.Lerp(punchScale, 1f, t);
            countdownText.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        countdownText.transform.localScale = Vector3.one;
    }
}