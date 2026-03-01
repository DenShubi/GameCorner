using UnityEngine;

public class LogController : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public int toughness = 10;

    // ======= TIME SLOW =======
    private float originalRotationSpeed;
    private bool isSlowed = false;
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
            // Matikan collider agar tidak bisa dipukul lagi saat proses hancur
            if (GetComponent<Collider>()) GetComponent<Collider>().enabled = false;

            // ======= EFEK HANCUR BERKEPING =======
            LogShatter shatter = GetComponent<LogShatter>();
            if (shatter != null)
            {
                shatter.Shatter();
            }
            // =====================================

            GameManager.instance.LogDestroyed();
            Destroy(gameObject);
        }
    }

    // ======= TIME SLOW SYSTEM =======

    /// <summary>
    /// Perlambat rotasi log. Dipanggil oleh PowerUpTimeSlow.
    /// </summary>
    public void ApplyTimeSlow(float multiplier, float duration)
    {
        if (isSlowed)
        {
            // Jika sudah slow, reset timer saja (perpanjang durasi)
            CancelInvoke(nameof(RemoveTimeSlow));
            Invoke(nameof(RemoveTimeSlow), duration);
            return;
        }

        isSlowed = true;
        originalRotationSpeed = rotationSpeed;
        rotationSpeed *= multiplier;

        Debug.Log($"[TimeSlow] Speed: {originalRotationSpeed} → {rotationSpeed}");

        // Auto-remove setelah durasi habis
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
}