using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Powered Charge / Smash system untuk Pong PvP.
/// Pasang pada GameObject bat (sama dengan PongBatController).
/// 
/// Cara kerja:
///   - Pemain TAHAN tombol charge → charge terisi
///   - Saat bat kena bola dalam kondisi FULLY CHARGED → SMASH
///   - Smash: bola sangat cepat, lurus ke lawan, minimal spin
/// </summary>
public class PongPowerCharge : MonoBehaviour
{
    [Header("Charge Settings")]
    [Tooltip("Durasi tahan tombol untuk full charge (detik)")]
    public float chargeTime = 1.2f;

    [Tooltip("Multiplier kecepatan bola saat smash")]
    public float smashSpeedMultiplier = 2.5f;

    [Tooltip("Setelah smash, kecepatan bola kembali normal setelah jarak ini")]
    public float smashSpeedDecayDistance = 8f;

    [Tooltip("Charge otomatis reset jika tidak dipakai setelah sekian detik")]
    public float chargeResetDelay = 3f;

    [Header("Visual — Bat")]
    [Tooltip("Renderer bat untuk efek warna charge")]
    public Renderer batRenderer;

    [Tooltip("Warna bat saat normal")]
    public Color normalColor = Color.white;

    [Tooltip("Warna bat saat fully charged")]
    public Color chargedColor = new Color(1f, 0.4f, 0f); // oranye

    [Header("Visual — UI")]
    [Tooltip("Image lingkaran charge (fill type = Radial 360)")]
    public Image chargeBarImage;

    [Tooltip("Warna charge bar saat mengisi")]
    public Color chargeBarColor = new Color(1f, 0.8f, 0f);

    [Tooltip("Warna charge bar saat full")]
    public Color chargeBarFullColor = new Color(1f, 0.3f, 0f);

    [Header("Input")]
    [Tooltip("Tombol charge untuk player ini (keyboard — untuk testing)")]
    public KeyCode chargeKey = KeyCode.Space;

    [Tooltip("Gunakan touch input (untuk mobile)")]
    public bool useTouchInput = true;

    [Tooltip("Referensi tombol UI charge (assign Button di Inspector)")]
    public UnityEngine.UI.Button chargeButton;

    // Internal
    private float currentCharge   = 0f;   // 0 = kosong, 1 = penuh
    private bool  isCharging      = false;
    private bool  isFullyCharged  = false;
    private bool  smashReady      = false; // true = akan smash di hit berikutnya
    private float chargeResetTimer = 0f;

    private MaterialPropertyBlock propBlock;
    private Coroutine resetCoroutine;
    private Coroutine batFlashCoroutine;

    // Public getter untuk PongBall
    public bool IsSmashReady => smashReady;
    public float SmashSpeedMultiplier => smashSpeedMultiplier;

    void Awake()
    {
        propBlock = new MaterialPropertyBlock();

        // Setup charge bar
        if (chargeBarImage != null)
        {
            chargeBarImage.type      = Image.Type.Filled;
            chargeBarImage.fillMethod = Image.FillMethod.Radial360;
            chargeBarImage.fillAmount = 0f;
            chargeBarImage.color     = chargeBarColor;
        }

        // Setup tombol UI
        if (chargeButton != null)
        {
            chargeButton.onClick.RemoveAllListeners();
        }
    }

    void Update()
    {
        HandleChargeInput();
        UpdateChargeVisual();

        // Auto reset jika charge penuh tapi tidak dipakai
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

        // Keyboard (testing di editor)
#if UNITY_EDITOR
        if (Input.GetKey(chargeKey))
            holdingCharge = true;
#endif

        // Touch: tahan tombol UI
        // chargeButton di-hold → isCharging diset dari PointerDown/Up
        // (dihandle via EventTrigger di Inspector, lihat setup)

        // Jika sudah smash ready, tidak perlu charge lagi
        if (smashReady) return;

        if (holdingCharge || isCharging)
        {
            currentCharge += Time.deltaTime / chargeTime;
            currentCharge  = Mathf.Clamp01(currentCharge);

            if (currentCharge >= 1f && !isFullyCharged)
            {
                isFullyCharged = true;
                smashReady     = true;
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

    // ── Charge Penuh ──────────────────────────────────────────────────────

    void OnFullyCharged()
    {
        // Flash bat warna charged
        if (batFlashCoroutine != null) StopCoroutine(batFlashCoroutine);
        batFlashCoroutine = StartCoroutine(BatFlashRoutine());

        Debug.Log($"[PowerCharge] {gameObject.name} SMASH READY!");
    }

    IEnumerator BatFlashRoutine()
    {
        // Bat berkedip oranye 3x saat fully charged
        for (int i = 0; i < 3; i++)
        {
            SetBatColor(chargedColor);
            yield return new WaitForSeconds(0.1f);
            SetBatColor(normalColor);
            yield return new WaitForSeconds(0.1f);
        }
        // Tahan warna charged sampai smash dipakai
        SetBatColor(chargedColor);
    }

    // ── Konsumsi Smash (dipanggil dari PongBall saat hit) ─────────────────

    /// <summary>
    /// Dipanggil oleh PongBall saat collision dengan bat ini.
    /// Return true jika smash diaktifkan, false jika hit normal.
    /// </summary>
    public bool ConsumeSmash()
    {
        if (!smashReady) return false;

        ResetCharge();
        StartCoroutine(SmashEffectRoutine());
        return true;
    }

    void ResetCharge()
    {
        currentCharge    = 0f;
        isFullyCharged   = false;
        smashReady       = false;
        isCharging       = false;
        chargeResetTimer = 0f;

        if (batFlashCoroutine != null)
        {
            StopCoroutine(batFlashCoroutine);
            batFlashCoroutine = null;
        }

        SetBatColor(normalColor);

        if (chargeBarImage != null)
            chargeBarImage.color = chargeBarColor;
    }

    IEnumerator SmashEffectRoutine()
    {
        // Bat flash putih saat smash
        SetBatColor(Color.white);
        yield return new WaitForSeconds(0.05f);
        SetBatColor(chargedColor);
        yield return new WaitForSeconds(0.05f);
        SetBatColor(normalColor);
    }

    // ── Visual Update ─────────────────────────────────────────────────────

    void UpdateChargeVisual()
    {
        if (chargeBarImage == null) return;

        chargeBarImage.fillAmount = currentCharge;

        // Warna bar berubah saat penuh
        chargeBarImage.color = isFullyCharged ? chargeBarFullColor : chargeBarColor;

        // Pulse scale bar saat fully charged
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