using UnityEngine;

public class LogController : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public int toughness = 10;

    // ======= TIME SLOW =======
    private float originalRotationSpeed;
    private bool isSlowed = false;
    // ==========================

    // ======= FASTER LOG =======
    private bool isFaster = false;
    // ==========================

    void Start()
    {
        originalRotationSpeed = rotationSpeed;
        Debug.Log("Log HP: " + toughness);
    }

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("Log HP: " + toughness);
        toughness -= damage;
        if (toughness <= 0)
        {
            if (GetComponent<Collider>()) GetComponent<Collider>().enabled = false;

            LogShatter shatter = GetComponent<LogShatter>();
            if (shatter != null)
            {
                shatter.Shatter();
            }

            BossLogController boss = GetComponentInParent<BossLogController>();
            if (boss != null)
            {
                boss.OnLayerDestroyed();
                gameObject.SetActive(false);
            }
            else
            {
                GameManager.instance.LogDestroyed();
                Destroy(gameObject);
            }
        }
    }

    // ======= TIME SLOW SYSTEM =======

    /// <summary>
    /// Perlambat rotasi log. Dipanggil oleh PowerUpTimeSlow.
    /// </summary>
    public void ApplyTimeSlow(float multiplier, float duration)
    {
        // Jika sedang faster, hapus dulu faster
        if (isFaster)
        {
            RemoveFasterLog();
        }

        if (isSlowed)
        {
            CancelInvoke(nameof(RemoveTimeSlow));
            Invoke(nameof(RemoveTimeSlow), duration);
            return;
        }

        isSlowed = true;
        originalRotationSpeed = rotationSpeed;
        rotationSpeed *= multiplier;

        Debug.Log($"[TimeSlow] Speed: {originalRotationSpeed} → {rotationSpeed}");

        Invoke(nameof(RemoveTimeSlow), duration);
    }

    private void RemoveTimeSlow()
    {
        if (!isSlowed) return;

        rotationSpeed = originalRotationSpeed;
        isSlowed = false;

        Debug.Log($"[TimeSlow] Speed kembali normal: {rotationSpeed}");
    }

    // =================================

    // ======= FASTER LOG SYSTEM (NERF) =======

    /// <summary>
    /// Percepat rotasi log. Dipanggil oleh NerfFasterLog.
    /// </summary>
    public void ApplyFasterLog(float multiplier, float duration)
    {
        // Jika sedang slow, hapus dulu slow
        if (isSlowed)
        {
            RemoveTimeSlow();
        }

        if (isFaster)
        {
            // Sudah faster, perpanjang durasi saja
            CancelInvoke(nameof(RemoveFasterLog));
            Invoke(nameof(RemoveFasterLog), duration);
            return;
        }

        isFaster = true;
        originalRotationSpeed = rotationSpeed;
        rotationSpeed *= multiplier;

        Debug.Log($"[FasterLog] Speed: {originalRotationSpeed} → {rotationSpeed}");

        Invoke(nameof(RemoveFasterLog), duration);
    }

    private void RemoveFasterLog()
    {
        if (!isFaster) return;

        rotationSpeed = originalRotationSpeed;
        isFaster = false;

        Debug.Log($"[FasterLog] Speed kembali normal: {rotationSpeed}");
    }

    // =========================================
}