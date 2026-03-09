using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PongPowerCharge : MonoBehaviour
{
    [Header("Charge Settings")]
    public float chargeTime          = 1.2f;
    public float smashSpeedMultiplier = 2.5f;
    public float smashSpeedDecayDistance = 8f;
    public float chargeResetDelay    = 3f;

    [Header("Visual — Bat")]
    public Renderer batRenderer;
    public Color normalColor  = Color.white;
    public Color chargedColor = new Color(1f, 0.4f, 0f);

    [Header("Visual — UI")]
    public Image  chargeBarImage;
    public Color  chargeBarColor     = new Color(1f, 0.8f, 0f);
    public Color  chargeBarFullColor = new Color(1f, 0.3f, 0f);

    [Header("Visual — Indicator")]                        // ← field baru
    [Tooltip("GameObject teks SMASH! (default nonaktif)")]
    public GameObject smashReadyIndicator;

    [Header("Input")]
    public KeyCode chargeKey      = KeyCode.Space;
    public bool    useTouchInput  = true;
    public Button  chargeButton;                          // ← tidak dipakai jika pakai EventTrigger

    // ── Public getter untuk PongBall ──────────────────────────────────────
    public bool  IsSmashReady        => smashReady;
    public float SmashSpeedMultiplier => smashSpeedMultiplier;

    // ── Internal ──────────────────────────────────────────────────────────
    private float     currentCharge    = 0f;
    private bool      isCharging       = false;
    private bool      isFullyCharged   = false;
    private bool      smashReady       = false;
    private float     chargeResetTimer = 0f;

    private MaterialPropertyBlock propBlock;
    private Coroutine batFlashCoroutine;

    void Awake()
    {
        propBlock = new MaterialPropertyBlock();

        if (chargeBarImage != null)
        {
            chargeBarImage.type       = Image.Type.Filled;
            chargeBarImage.fillMethod = Image.FillMethod.Vertical;
            chargeBarImage.fillOrigin = (int)Image.OriginVertical.Bottom;
            chargeBarImage.fillAmount = 0f;
            chargeBarImage.color      = chargeBarColor;
        }

        // Pastikan indicator hidden di awal
        if (smashReadyIndicator != null)
            smashReadyIndicator.SetActive(false);
    }

    void Update()
    {
        HandleChargeInput();
        UpdateChargeVisual();

        // Auto reset jika smash tidak dipakai terlalu lama
        if (smashReady)
        {
            chargeResetTimer -= Time.deltaTime;
            if (chargeResetTimer <= 0f)
                ResetCharge();
        }
    }

    // ── Input ─────────────────────────────────────────────────────────────

    void HandleChargeInput()
    {
        bool holdingCharge = false;

#if UNITY_EDITOR
        if (Input.GetKey(chargeKey))
            holdingCharge = true;
#endif

        if (smashReady) return;

        if (holdingCharge || isCharging)
        {
            currentCharge += Time.deltaTime / chargeTime;
            currentCharge  = Mathf.Clamp01(currentCharge);

            if (currentCharge >= 1f && !isFullyCharged)
            {
                isFullyCharged   = true;
                smashReady       = true;
                chargeResetTimer = chargeResetDelay;
                OnFullyCharged();
            }
        }
        else if (!isFullyCharged)
        {
            // Lepas sebelum full → charge berkurang perlahan
            currentCharge -= Time.deltaTime / (chargeTime * 0.5f);
            currentCharge  = Mathf.Clamp01(currentCharge);
        }
    }

    // ── Dipanggil dari EventTrigger tombol UI ─────────────────────────────

    public void OnChargeButtonDown()
    {
        if (smashReady) return;
        isCharging = true;
    }

    public void OnChargeButtonUp()
    {
        isCharging = false;
    }

    // ── Fully Charged ─────────────────────────────────────────────────────

    void OnFullyCharged()
    {
        // Tampilkan indikator SMASH!
        if (smashReadyIndicator != null)
            smashReadyIndicator.SetActive(true);

        if (batFlashCoroutine != null) StopCoroutine(batFlashCoroutine);
        batFlashCoroutine = StartCoroutine(BatFlashRoutine());

        Debug.Log($"[PowerCharge] {gameObject.name} SMASH READY!");
    }

    IEnumerator BatFlashRoutine()
    {
        for (int i = 0; i < 3; i++)
        {
            SetBatColor(chargedColor);
            yield return new WaitForSeconds(0.1f);
            SetBatColor(normalColor);
            yield return new WaitForSeconds(0.1f);
        }
        SetBatColor(chargedColor);
    }

    // ── Konsumsi Smash ────────────────────────────────────────────────────

    public bool ConsumeSmash()
    {
        if (!smashReady) return false;
        ResetCharge();
        StartCoroutine(SmashEffectRoutine());
        return true;
    }

    IEnumerator SmashEffectRoutine()
    {
        SetBatColor(Color.white);
        yield return new WaitForSeconds(0.05f);
        SetBatColor(chargedColor);
        yield return new WaitForSeconds(0.05f);
        SetBatColor(normalColor);
    }

    // ── Reset ─────────────────────────────────────────────────────────────

    void ResetCharge()
    {
        currentCharge    = 0f;
        isFullyCharged   = false;
        smashReady       = false;
        isCharging       = false;
        chargeResetTimer = 0f;

        // Sembunyikan indikator SMASH!
        if (smashReadyIndicator != null)
            smashReadyIndicator.SetActive(false);

        if (batFlashCoroutine != null)
        {
            StopCoroutine(batFlashCoroutine);
            batFlashCoroutine = null;
        }

        SetBatColor(normalColor);

        if (chargeBarImage != null)
        {
            chargeBarImage.fillAmount = 0f;
            chargeBarImage.color      = chargeBarColor;
        }
    }

    // ── Visual Update ─────────────────────────────────────────────────────

    void UpdateChargeVisual()
    {
        if (chargeBarImage == null) return;

        chargeBarImage.fillAmount = currentCharge;
        chargeBarImage.color      = isFullyCharged
                                    ? chargeBarFullColor
                                    : chargeBarColor;

        // Pulse saat fully charged
        if (isFullyCharged)
        {
            float pulse = 1f + Mathf.Sin(Time.time * 8f) * 0.08f;
            chargeBarImage.transform.localScale = Vector3.one * pulse;
        }
        else
        {
            chargeBarImage.transform.localScale = Vector3.one;
        }
    }

    void SetBatColor(Color color)
    {
        if (batRenderer == null) return;
        batRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_BaseColor", color);
        propBlock.SetColor("_Color",     color);
        batRenderer.SetPropertyBlock(propBlock);
    }
}