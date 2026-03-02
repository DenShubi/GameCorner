using UnityEngine;

/// <summary>
/// Controller untuk Boss Log berlapis.
/// Mengelola 3 lapis log (outer → middle → inner).
/// Setiap lapis hancur → lapis berikutnya aktif dengan rotasi balik & lebih cepat.
/// </summary>
public class BossLogController : MonoBehaviour
{
    [Header("Layer References (drag dari child)")]
    [Tooltip("Lapis luar (paling besar) — Layer 2")]
    public GameObject layerOuter;

    [Tooltip("Lapis tengah — Layer 1")]
    public GameObject layerMiddle;

    [Tooltip("Lapis dalam (paling kecil) — Core")]
    public GameObject layerInner;

    [Header("Layer Toughness")]
    [Tooltip("Base toughness lapis luar")]
    public int outerToughness = 8;

    [Tooltip("Base toughness lapis tengah")]
    public int middleToughness = 6;

    [Tooltip("Base toughness lapis dalam")]
    public int innerToughness = 4;

    [Header("Layer Rotation Speed")]
    [Tooltip("Kecepatan rotasi lapis luar")]
    public float outerRotationSpeed = 80f;

    [Tooltip("Kecepatan rotasi lapis tengah (negatif = balik arah)")]
    public float middleRotationSpeed = -120f;

    [Tooltip("Kecepatan rotasi lapis dalam")]
    public float innerRotationSpeed = 160f;

    [Header("Layer Obstacles")]
    [Tooltip("Jumlah obstacle lapis luar")]
    public int outerObstacles = 2;

    [Tooltip("Jumlah obstacle lapis tengah")]
    public int middleObstacles = 3;

    [Tooltip("Jumlah obstacle lapis dalam")]
    public int innerObstacles = 4;

    [Header("Boss Scaling (per boss ke-n)")]
    [Tooltip("Tambahan toughness per boss encounter")]
    public int toughnessPerBoss = 3;

    [Tooltip("Tambahan rotation speed per boss encounter")]
    public float rotationSpeedPerBoss = 15f;

    [Header("Score")]
    [Tooltip("Bonus score per lapis hancur")]
    public int scorePerLayer = 75;

    [Tooltip("Bonus score saat boss kalah (semua lapis hancur)")]
    public int bossDefeatedBonus = 200;

    // Internal tracking
    private int currentLayerIndex = 0; // 0=outer, 1=middle, 2=inner
    private LogController activeLogController;

    /// <summary>
    /// Inisialisasi boss. Dipanggil oleh GameManager setelah spawn.
    /// bossNumber = boss ke berapa (1, 2, 3, ...)
    /// </summary>
    public void InitBoss(int bossNumber)
    {
        int extraToughness = toughnessPerBoss * (bossNumber - 1);
        float extraSpeed = rotationSpeedPerBoss * (bossNumber - 1);

        // Hitung toughness yang di-scale
        int scaledOuterTough = outerToughness + extraToughness;
        int scaledMiddleTough = middleToughness + extraToughness;
        int scaledInnerTough = innerToughness + extraToughness;

        // Hitung rotation speed yang di-scale
        float scaledOuterSpeed = outerRotationSpeed + (outerRotationSpeed > 0 ? extraSpeed : -extraSpeed);
        float scaledMiddleSpeed = middleRotationSpeed + (middleRotationSpeed > 0 ? extraSpeed : -extraSpeed);
        float scaledInnerSpeed = innerRotationSpeed + (innerRotationSpeed > 0 ? extraSpeed : -extraSpeed);

        // Setup semua layer LogController
        SetupLayer(layerOuter, scaledOuterTough, scaledOuterSpeed);
        SetupLayer(layerMiddle, scaledMiddleTough, scaledMiddleSpeed);
        SetupLayer(layerInner, scaledInnerTough, scaledInnerSpeed);

        // Mulai dari layer outer, matikan yang lain
        layerMiddle.SetActive(false);
        layerInner.SetActive(false);
        currentLayerIndex = 0;

        // Aktifkan layer outer
        ActivateLayer(layerOuter, outerObstacles);

        Debug.Log($"[Boss] Boss #{bossNumber} spawned! " +
                  $"Outer: {scaledOuterTough}HP/{scaledOuterSpeed}spd, " +
                  $"Middle: {scaledMiddleTough}HP/{scaledMiddleSpeed}spd, " +
                  $"Inner: {scaledInnerTough}HP/{scaledInnerSpeed}spd");
    }

    private void SetupLayer(GameObject layer, int toughness, float speed)
    {
        if (layer == null) return;

        LogController logCtrl = layer.GetComponent<LogController>();
        if (logCtrl == null)
        {
            logCtrl = layer.AddComponent<LogController>();
        }
        logCtrl.toughness = toughness;
        logCtrl.rotationSpeed = speed;
    }

    private void ActivateLayer(GameObject layer, int obstacleCount)
    {
        if (layer == null) return;

        layer.SetActive(true);

        activeLogController = layer.GetComponent<LogController>();

        // Spawn obstacles pada layer ini
        LogObstacleSpawner obsSpawner = layer.GetComponent<LogObstacleSpawner>();
        if (obsSpawner != null && obstacleCount > 0)
        {
            obsSpawner.obstacleCount = obstacleCount;
            obsSpawner.SpawnObstacles();
        }

        // Spawn power-up pada layer ini
        GameManager.instance.SpawnPowerUpOnLog(layer);
    }

    /// <summary>
    /// Dipanggil oleh LogController saat layer hancur (toughness <= 0).
    /// Menggantikan GameManager.LogDestroyed() untuk boss.
    /// </summary>
    public void OnLayerDestroyed()
    {
        // Score per layer
        GameManager.instance.AddScore(scorePerLayer);

        // Scatter knife dari layer yang hancur
        GameManager.instance.ScatterStuckKnives();

        currentLayerIndex++;

        if (currentLayerIndex == 1)
        {
            // Outer hancur → aktifkan Middle
            Debug.Log("[Boss] Layer Outer hancur! Layer Middle aktif!");
            ActivateLayer(layerMiddle, middleObstacles);
        }
        else if (currentLayerIndex == 2)
        {
            // Middle hancur → aktifkan Inner
            Debug.Log("[Boss] Layer Middle hancur! Layer Inner (Core) aktif!");
            ActivateLayer(layerInner, innerObstacles);
        }
        else
        {
            // Inner hancur → Boss Defeated!
            Debug.Log("[Boss] BOSS DEFEATED! 🎉");
            GameManager.instance.AddScore(bossDefeatedBonus);
            GameManager.instance.BossDefeated();
        }
    }
}