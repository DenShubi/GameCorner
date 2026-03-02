using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Mengontrol overlay kabut di layar.
/// Ditaruh di Canvas sebagai Image full-screen.
/// Fade in → tahan → fade out.
/// </summary>
public class FogScreenUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Image overlay kabut (full-screen)")]
    public Image fogImage;

    [Header("Fade Settings")]
    [Tooltip("Durasi fade-in dalam detik")]
    public float fadeInDuration = 0.3f;

    [Tooltip("Durasi fade-out dalam detik")]
    public float fadeOutDuration = 0.8f;

    private Coroutine fogCoroutine;

    void Start()
    {
        // Pastikan fog tersembunyi saat mulai
        if (fogImage != null)
        {
            Color c = fogImage.color;
            c.a = 0f;
            fogImage.color = c;
            fogImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Aktifkan fog. Dipanggil oleh NerfFogScreen.
    /// Jika fog sudah aktif, reset durasi (perpanjang).
    /// </summary>
    public void ActivateFog(float duration, float opacity)
    {
        // Jika sudah aktif, stop dan restart
        if (fogCoroutine != null)
        {
            StopCoroutine(fogCoroutine);
        }

        fogCoroutine = StartCoroutine(FogRoutine(duration, opacity));
    }

    private IEnumerator FogRoutine(float duration, float targetOpacity)
    {
        if (fogImage == null) yield break;

        fogImage.gameObject.SetActive(true);

        // ===== FADE IN =====
        float elapsed = 0f;
        Color c = fogImage.color;
        float startAlpha = c.a;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            c.a = Mathf.Lerp(startAlpha, targetOpacity, t);
            fogImage.color = c;
            yield return null;
        }

        c.a = targetOpacity;
        fogImage.color = c;

        // ===== TAHAN SELAMA DURASI =====
        yield return new WaitForSeconds(duration);

        // ===== FADE OUT =====
        elapsed = 0f;
        startAlpha = c.a;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            c.a = Mathf.Lerp(startAlpha, 0f, t);
            fogImage.color = c;
            yield return null;
        }

        c.a = 0f;
        fogImage.color = c;
        fogImage.gameObject.SetActive(false);

        fogCoroutine = null;

        Debug.Log("[FogScreen] Kabut hilang!");
    }
}