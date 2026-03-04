using UnityEngine;

/// <summary>
/// Log khusus multiplayer: rotasi, HP, power up, nerf, shatter, disable collider saat hancur,
/// support boss mode (memberi tahu MultiplayerManager atau BossLogController jika ada parent boss).
/// Patch untuk multiplayer: damage, shatter, boss, efek global tetap work.
/// </summary>
public class MultiplayerLogController : MonoBehaviour
{
    public float rotationSpeed = 120f;
    public int toughness = 8;

    // For Time Slow system
    private float originalRotationSpeed;
    private bool isSlowed = false;

    // For Faster Log
    private bool isFaster = false;

    void Start()
    {
        originalRotationSpeed = rotationSpeed;
    }

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Dipanggil knife (MultiplayerKnifeController), playerID pemberi damage.
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="playerID"></param>
    public void TakeDamage(int damage, int playerID)
    {
        toughness -= damage;
        if (toughness <= 0)
        {
            // Matikan collider (termasuk jika ada banyak collider di anak)
            foreach (Collider col in GetComponentsInChildren<Collider>())
                col.enabled = false;

            // Efek hancur
            LogShatter shatter = GetComponent<LogShatter>();
            if (shatter != null)
            {
                shatter.Shatter();
            }

            // Jika parent boss, pakai sistem boss, jika tidak, multiplayer manager
            var boss = GetComponentInParent<BossLogController>();
            if (boss != null)
            {
                boss.OnLayerDestroyed();
                gameObject.SetActive(false);
            }
            else
            {
                MultiplayerManager.instance.LogDestroyed(playerID);
                Destroy(gameObject);
            }
        }
    }

    // ======= TIME SLOW SYSTEM =======

    /// <summary>
    /// Perlambat rotasi log multiplayer. Dipanggil power up.
    /// </summary>
    public void ApplyTimeSlow(float multiplier, float duration)
    {
        // Remove faster jika aktif
        if (isFaster) RemoveFasterLog();

        if (isSlowed)
        {
            CancelInvoke(nameof(RemoveTimeSlow));
            Invoke(nameof(RemoveTimeSlow), duration);
            return;
        }

        isSlowed = true;
        originalRotationSpeed = rotationSpeed;
        rotationSpeed *= multiplier;

        Invoke(nameof(RemoveTimeSlow), duration);
    }

    private void RemoveTimeSlow()
    {
        if (!isSlowed) return;

        rotationSpeed = originalRotationSpeed;
        isSlowed = false;
    }

    // ======= FASTER LOG (NERF) SYSTEM =======

    public void ApplyFasterLog(float multiplier, float duration)
    {
        // Remove slow kalau sedang slow
        if (isSlowed) RemoveTimeSlow();

        if (isFaster)
        {
            CancelInvoke(nameof(RemoveFasterLog));
            Invoke(nameof(RemoveFasterLog), duration);
            return;
        }

        isFaster = true;
        originalRotationSpeed = rotationSpeed;
        rotationSpeed *= multiplier;

        Invoke(nameof(RemoveFasterLog), duration);
    }

    private void RemoveFasterLog()
    {
        if (!isFaster) return;

        rotationSpeed = originalRotationSpeed;
        isFaster = false;
    }

    // ======= API PowerUp =======
    /// <summary>
    /// Untuk power-up: "global" log effect di multiplayer.
    /// </summary>
    public void ApplyEffect(string effectName, float amount, float duration)
    {
        if (effectName == "TimeSlow") ApplyTimeSlow(amount, duration);
        else if (effectName == "FasterLog") ApplyFasterLog(amount, duration);
    }
}