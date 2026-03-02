using UnityEngine;

/// <summary>
/// Log controller untuk mode Multiplayer.
/// Mirip LogController tapi track playerID terakhir yang mengenai.
/// Log bisa dihancurkan oleh kedua player.
/// </summary>
public class MultiplayerLogController : MonoBehaviour
{
    public float rotationSpeed = 120f;
    public int toughness = 8;

    // Track player terakhir yang mengenai log ini
    private int lastHitPlayerID = 0;

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Dipanggil oleh MultiplayerKnifeController saat knife mengenai log.
    /// </summary>
    public void TakeDamage(int damage, int playerID)
    {
        lastHitPlayerID = playerID;
        toughness -= damage;

        Debug.Log($"[Multi] Log HP: {toughness} (hit by P{playerID})");

        if (toughness <= 0)
        {
            if (GetComponent<Collider>()) GetComponent<Collider>().enabled = false;

            // Efek hancur
            LogShatter shatter = GetComponent<LogShatter>();
            if (shatter != null)
            {
                shatter.Shatter();
            }

            // Beritahu MultiplayerManager
            MultiplayerManager.instance.LogDestroyed(lastHitPlayerID);
            Destroy(gameObject);
        }
    }
}