using System.Collections;
using UnityEngine;

public class PongBallVisuals : MonoBehaviour
{
    [Header("References")]
    public Renderer ballRenderer;
    public TrailRenderer trail;

    [Header("Ball Color Settings")]
    [Tooltip("Warna bola saat kecepatan minimum")]
    public Color colorSlow = Color.white;

    [Tooltip("Warna bola saat kecepatan sedang")]
    public Color colorMid = Color.yellow;

    [Tooltip("Warna bola saat kecepatan maksimum")]
    public Color colorFast = new Color(1f, 0.2f, 0f);

    [Header("Speed Reference")]
    public float minSpeed = 5f;
    public float maxSpeed = 12f;

    [Header("Trail Settings")]
    [Tooltip("Waktu trail (panjang ekor) saat speed minimum")]
    public float trailTimeMin = 0.08f;

    [Tooltip("Waktu trail (panjang ekor) saat speed maksimum")]
    public float trailTimeMax = 0.35f;

    [Tooltip("Lebar trail di ujung belakang (lebar = dekat bola)")]
    public float trailWidthNearBall = 0.4f;

    [Tooltip("Lebar trail di ujung depan (tipis = ujung ekor)")]
    public float trailWidthTip = 0f;

    [Tooltip("Warna trail (putih sesuai referensi)")]
    public Color trailColor = Color.white;

    [Tooltip("Opacity maksimum trail (0-1)")]
    [Range(0f, 1f)]
    public float trailMaxAlpha = 0.75f;

    [Header("Pulse Settings")]
    public float pulseScale   = 1.35f;
    public float pulseDuration = 0.12f;

    // Internal
    private Rigidbody rb;
    private Vector3 originalScale;
    private Coroutine pulseCoroutine;
    private MaterialPropertyBlock propBlock;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (ballRenderer == null)
            ballRenderer = GetComponent<Renderer>();

        propBlock     = new MaterialPropertyBlock();
        originalScale = transform.localScale;

        SetupTrail();
    }

    void SetupTrail()
    {
        if (trail == null) return;

        trail.time = trailTimeMin;

        // ── Lebar: tipis di ujung, lebar dekat bola ────────────────────
        // AnimationCurve: t=0 adalah ujung paling depan (tip),
        //                 t=1 adalah posisi bola saat ini (near)
        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0f, trailWidthTip);       // ujung ekor = tipis
        widthCurve.AddKey(1f, trailWidthNearBall);  // dekat bola = lebar
        trail.widthCurve = widthCurve;

        // ── Warna: putih opak dekat bola, transparan di ujung ──────────
        ApplyTrailGradient();
    }

    void ApplyTrailGradient()
    {
        if (trail == null) return;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                // t=0 = ujung ekor (tip), t=1 = dekat bola
                new GradientColorKey(trailColor, 0f),
                new GradientColorKey(trailColor, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0f,             0f),   // ujung = transparan
                new GradientAlphaKey(trailMaxAlpha,  1f)    // dekat bola = opak
            }
        );
        trail.colorGradient = gradient;
    }

    // ── Update setiap frame ───────────────────────────────────────────────

    void Update()
    {
        float speed = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;
        float t     = Mathf.InverseLerp(minSpeed, maxSpeed, speed);

        UpdateBallColor(t);
        UpdateTrailLength(t);
    }

    // ── Ball Color ────────────────────────────────────────────────────────

    void UpdateBallColor(float t)
    {
        Color targetColor = t < 0.5f
            ? Color.Lerp(colorSlow, colorMid,  t * 2f)
            : Color.Lerp(colorMid,  colorFast, (t - 0.5f) * 2f);

        ballRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_BaseColor", targetColor); // URP
        propBlock.SetColor("_Color",     targetColor); // Built-in
        ballRenderer.SetPropertyBlock(propBlock);
    }

    // ── Trail Length ────────────────────���─────────────────────────────────

    void UpdateTrailLength(float t)
    {
        if (trail == null) return;
        trail.time = Mathf.Lerp(trailTimeMin, trailTimeMax, t);
    }

    // ── Pulse ─────────────────────────────────────────────────────────────

    public void PlayHitPulse()
    {
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseRoutine());
    }

    IEnumerator PulseRoutine()
    {
        float elapsed  = 0f;
        float halfTime = pulseDuration * 0.5f;

        while (elapsed < halfTime)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / halfTime;
            transform.localScale = Vector3.Lerp(originalScale, originalScale * pulseScale, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfTime)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / halfTime;
            transform.localScale = Vector3.Lerp(originalScale * pulseScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
        pulseCoroutine       = null;
    }

    // ── Clear ─────────────────────────��───────────────────────────────────

    public void ClearTrail()
    {
        if (trail != null)
            trail.Clear();
    }

        /// <summary>
    /// Efek visual khusus saat bola kena SMASH.
    /// Bola flash merah + pulse besar.
    /// </summary>
    public void PlaySmashEffect()
    {
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(SmashEffectRoutine());
    }

    IEnumerator SmashEffectRoutine()
    {
        // Flash merah
        ballRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_BaseColor", Color.red);
        propBlock.SetColor("_Color",     Color.red);
        ballRenderer.SetPropertyBlock(propBlock);

        // Scale besar
        float elapsed  = 0f;
        float halfTime = 0.08f;

        while (elapsed < halfTime)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / halfTime;
            transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.6f, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfTime)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / halfTime;
            transform.localScale = Vector3.Lerp(originalScale * 1.6f, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
        pulseCoroutine = null;
    }
}